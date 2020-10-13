using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UMP.Wrappers;

namespace UMP
{
    public class MediaPlayerStandalone : IPlayer, IPlayerAudio, IPlayerSpu
    {
        private MonoBehaviour _monoObject;
        private WrapperStandalone _wrapper;

        /// Native VLC library callback pointers
        private IntPtr _lockPtr;
        private IntPtr _displayPtr;
        private IntPtr _formatSetupPtr;

        private IntPtr _audioPlayPtr;
        private IntPtr _audioFormatPtr;

        private IntPtr _eventHandlerPtr;
        private IntPtr _eventManagerPtr;

        private IntPtr _vlcObj;
        private IntPtr _mediaObj;
        private IntPtr _playerObj;

        private int _framesCounter;
        private float _frameRate;
        private float _tmpTime;
        private int _tmpFramesCounter;

        private bool _isStarted;
        private bool _isPlaying;
        private bool _isLoad;
        private bool _isReady;
        private bool _isImageReady;
        private bool _isTextureExist;

        private LogLevels _logDetail;
        private Action<PlayerManagerLogs.PlayerLog> _logListener;
        private string _dataSource;
        private PlayerBufferVideo _videoBuffer;
        private PlayerManagerLogs _logManager;
        private PlayerManagerEvents _eventManager;
        private PlayerOptionsStandalone _options;
        private MediaStats _mediaStats;
        private string[] _arguments;

        private Texture2D _videoTexture;
        private GameObject[] _videoOutputObjects;
        private PlayerManagerAudios _audioManager;
        private GCHandle _audioDataHandle = default(GCHandle);

        private delegate void ManageBufferSizeCallback(int width, int height);
        private ManageBufferSizeCallback _manageBufferSizeCallback;

        private IEnumerator _updateVideoTextureEnum;

        #region Constructors
        /// <summary>
        ///  Create instance of MediaPlayerStandalone object with additional arguments
        /// </summary>
        /// <param name="monoObject">MonoBehaviour instanse</param>
        /// <param name="videoOutputObjects">Objects that will be rendering video output</param>
        /// <param name="options">Additional player options</param>
        public MediaPlayerStandalone(MonoBehaviour monoObject, GameObject[] videoOutputObjects, PlayerOptionsStandalone options)
        {
            _monoObject = monoObject;
            _videoOutputObjects = videoOutputObjects;
            _options = options;

            _wrapper = new WrapperStandalone(_options);

            if (_wrapper.NativeIndex < 0)
            {
                Debug.LogError("Don't support video playback on current platform or you use incorrect UMP libraries!");
                throw new Exception();
            }

            if (_options != null)
            {
                if (!string.IsNullOrEmpty(_options.DirectAudioDevice))
                    _options.DirectAudioDevice = GetAudioDevice(_options.DirectAudioDevice);

                _wrapper.NativeSetPixelsVerticalFlip(_options.FlipVertically);

                if (_options.AudioOutputs != null && _options.AudioOutputs.Length > 0)
                {
                    _audioManager = new PlayerManagerAudios(_options.AudioOutputs);
                    _audioManager.AddListener(OnAudioFilterRead);
                }

                _arguments = _options.GetOptions('\n').Split('\n');
                _logDetail = _options.LogDetail;
                _logListener = _options.LogListener;
            }

            MediaPlayerInit();
        }
        #endregion

        private void MediaPlayerInit()
        {
            _vlcObj = _wrapper.ExpandedLibVLCNew(_arguments);

            if (_vlcObj == IntPtr.Zero)
                throw new Exception("Can't create new libVLC object instance");

            _playerObj = _wrapper.ExpandedMediaPlayerNew(_vlcObj);

            if (_playerObj == IntPtr.Zero)
                throw new Exception("Can't create new media player object instance");

            _eventManagerPtr = _wrapper.ExpandedEventManager(_playerObj);
            _eventHandlerPtr = _wrapper.NativeMediaPlayerEventCallback();
            EventsAttach(_eventManagerPtr, _eventHandlerPtr);

            _eventManager = new PlayerManagerEvents(_monoObject, this);
            _eventManager.PlayerPlayingListener += OnPlayerPlaying;
            _eventManager.PlayerPausedListener += OnPlayerPaused;

            if (_logDetail != LogLevels.Disable)
                _wrapper.ExpandedLogSet(_vlcObj, _wrapper.NativeGetLogMessageCallback(), new IntPtr(_wrapper.NativeIndex));

            _logManager = new PlayerManagerLogs(_monoObject, this);
            _logManager.LogDetail = _logDetail;
            _logManager.LogMessageListener += _logListener;

            _lockPtr = _wrapper.NativeGetVideoLockCallback();
            _displayPtr = _wrapper.NativeGetVideoDisplayCallback();
            _formatSetupPtr = _wrapper.NativeGetVideoFormatCallback();

            _audioFormatPtr = _wrapper.NativeGetAudioSetupCallback();
            _audioPlayPtr = _wrapper.NativeGetAudioPlayCallback();

            _wrapper.ExpandedVideoSetCallbacks(_playerObj, _lockPtr, IntPtr.Zero, _displayPtr, new IntPtr(_wrapper.NativeIndex));

            if (_options.FixedVideoSize == Vector2.zero)
                _wrapper.ExpandedVideoSetFormatCallbacks(_playerObj, _formatSetupPtr, IntPtr.Zero);
            else
                _wrapper.ExpandedVideoSetFormat(_playerObj, PlayerBufferVideo.Chroma, (int)_options.FixedVideoSize.x, (int)_options.FixedVideoSize.y, PlayerBufferVideo.CalculatePitch((int)_options.FixedVideoSize.x));

            _manageBufferSizeCallback = InitBufferSize;
            _wrapper.NativeSetBufferSizeCallback(Marshal.GetFunctionPointerForDelegate(_manageBufferSizeCallback));

            if (_audioManager != null && _audioManager.IsValid)
            {
                _wrapper.ExpandedAudioSetFormatCallbacks(_playerObj, _audioFormatPtr, IntPtr.Zero);
                _wrapper.ExpandedAudioSetCallbacks(_playerObj, _audioPlayPtr, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, new IntPtr(_wrapper.NativeIndex));
                _wrapper.NativeSetAudioParams(2, AudioSettings.outputSampleRate);
            }

            _mediaStats = new MediaStats();
        }

        #region Private methods
        private void OnRelease()
        {
            bool threadDone = false;

            while (!threadDone)
            {
                threadDone = _monoObject == null;
                Thread.Sleep(1000);
            }

            Release();
        }

        private void InitBufferSize(int width, int height)
        {
            _wrapper.NativePixelsBufferRelease();

            if (_videoBuffer != null)
            {
                if (_videoBuffer.Width != width ||
                    _videoBuffer.Height != height)
                {
                    _videoBuffer.ClearFramePixels();
                    _videoBuffer = null;
                }
            }

            if (_videoBuffer == null)
            {
                _videoBuffer = new PlayerBufferVideo(width, height);
                _wrapper.NativeSetPixelsBuffer(_videoBuffer.FramePixelsAddr, _videoBuffer.Width, _videoBuffer.Height);
                _isTextureExist = false;
            }
        }

        private string GetAudioDevice(string description)
        {
            var audioDevicesAoutNames = new string[] { "directsound", "directx" };

            var vlcObj = _wrapper.ExpandedLibVLCNew(null);
            var audioDevicePtr = _wrapper.ExpandedAudioOutputListGet(vlcObj);
            var audioDevicesPtr = IntPtr.Zero;
            var audioDeviceName = string.Empty;

            var audioDevice = new AudioDescription
            {
                NextDescription = audioDevicePtr,
                Description = null,
                Name = null
            };


            while (audioDevice.NextDescription != IntPtr.Zero)
            {
                audioDevice = (AudioDescription)Marshal.PtrToStructure(audioDevice.NextDescription, typeof(AudioDescription));

                for (int i = 0; i < audioDevicesAoutNames.Length; i += 2)
                {
                    if (audioDevice.Name.Contains(audioDevicesAoutNames[i]))
                    {
                        audioDevicesPtr = _wrapper.ExpandedAudioOutputDeviceListGet(vlcObj, audioDevicesAoutNames[i + 1]);
                        break;
                    }
                }
            }

            if (audioDevicesPtr == IntPtr.Zero)
            {
                Debug.Log("GetAudioDevice: Can't get audio output device list for " + audioDevice.Name);
                return audioDeviceName;
            }

            AudioOutputDevice outputDevice = new AudioOutputDevice
            {
                NextDevice = audioDevicesPtr,
                Description = null,
                Device = null
            };

            try
            {
                while (outputDevice.NextDevice != IntPtr.Zero)
                {
                    outputDevice = (AudioOutputDevice)Marshal.PtrToStructure(outputDevice.NextDevice, typeof(AudioOutputDevice));
                    if (outputDevice.Description.Contains(description))
                    {
                        Debug.Log("GetAudioDevice: New audio output device \n" +
                            "Device: " + outputDevice.Device + "\n" +
                            "Description: " + outputDevice.Description);

                        audioDeviceName = outputDevice.Device;
                    }
                }
            }
            finally
            {
                if (audioDevicePtr != IntPtr.Zero)
                    _wrapper.ExpandedAudioOutputListRelease(audioDevicePtr);

                if (audioDevicesPtr != IntPtr.Zero)
                    _wrapper.ExpandedAudioOutputDeviceListRelease(audioDevicesPtr);

                _wrapper.ExpandedLibVLCRelease(vlcObj);
            }

            if (string.IsNullOrEmpty(audioDeviceName))
                Debug.Log("GetAudioDevice: Can't find audio output device - switched to Default");

            return audioDeviceName;
        }

        private void UpdateFpsCounter(int frameCounter)
        {
            float currentTime = UnityEngine.Time.time;
            currentTime = (currentTime > _tmpTime) ? currentTime - _tmpTime : 0;
            if (currentTime >= 1f)
            {
                _frameRate = frameCounter - _tmpFramesCounter;
                _tmpFramesCounter = frameCounter;
                _tmpTime = UnityEngine.Time.time;
            }
        }

        private void EventsAttach(IntPtr eventManager, IntPtr enentHandlerPtr)
        {
            string exceptionMsg = "Failed to subscribe to event notification";

            if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerOpening, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
                throw new OutOfMemoryException(exceptionMsg + " (Opening)");

            if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerBuffering, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
                throw new OutOfMemoryException(exceptionMsg + " (Buffering)");

            //if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerPlaying, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
            //    throw new OutOfMemoryException(exceptionMsg + " (Playing)");

            if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerPaused, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
                throw new OutOfMemoryException(exceptionMsg + " (Paused)");

            if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerStopped, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
                throw new OutOfMemoryException(exceptionMsg + " (Stopped)");

            if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerEndReached, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
                throw new OutOfMemoryException(exceptionMsg + " (EndReached)");

            if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerEncounteredError, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
                throw new OutOfMemoryException(exceptionMsg + " (EncounteredError)");

            if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerTimeChanged, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
                throw new OutOfMemoryException(exceptionMsg + " (TimeChanged)");

            if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerPositionChanged, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
                throw new OutOfMemoryException(exceptionMsg + " (PositionChanged)");

            if (_wrapper.ExpandedEventAttach(eventManager, EventTypes.MediaPlayerSnapshotTaken, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex) != 0)
                throw new OutOfMemoryException(exceptionMsg + " (SnapshotTaken)");
        }

        private void EventsDettach(IntPtr eventManager, IntPtr enentHandlerPtr)
        {
            _wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerOpening, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
            _wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerBuffering, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
            //_wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerPlaying, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
            _wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerPaused, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
            _wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerStopped, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
            _wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerEndReached, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
            _wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerEncounteredError, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
            _wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerTimeChanged, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
            _wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerPositionChanged, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
            _wrapper.ExpandedEventDetach(eventManager, EventTypes.MediaPlayerSnapshotTaken, enentHandlerPtr, (IntPtr)_wrapper.NativeIndex);
        }

        private IEnumerator UpdateVideoTexture()
        {
            MediaTrackInfoExpanded[] tracks = null;
            var hasVideo = false;

            while (true)
            {
                if (_playerObj != IntPtr.Zero && _wrapper.PlayerIsPlaying(_playerObj))
                {
                    if (tracks == null)
                    {
                        tracks = TracksInfo;

                        if (tracks != null)
                        {
                            foreach (var track in tracks)
                            {
                                if (track is MediaTrackInfoVideo)
                                    hasVideo = true;
                            }
                        }
                        else
                        {
                            yield return null;
                            continue;
                        }
                    }

                    if (FramesCounter != _framesCounter)
                    {
                        _framesCounter = FramesCounter;
                        UpdateFpsCounter(_framesCounter);

                        if (!_isTextureExist)
                        {
                            if (_videoTexture != null)
                            {
                                UnityEngine.Object.Destroy(_videoTexture);
                                _videoTexture = null;
                            }

                            _videoTexture = MediaPlayerHelper.GenPluginTexture(_videoBuffer.Width, _videoBuffer.Height);
                            MediaPlayerHelper.ApplyTextureToRenderingObjects(_videoTexture, _videoOutputObjects);
                            _wrapper.NativeSetTexture(_videoTexture.GetNativeTexturePtr());
                            _isTextureExist = true;
                            _isImageReady = false;
                        }

                        GL.IssuePluginEvent(_wrapper.NativeGetUnityRenderCallback(), _wrapper.NativeIndex);

                        if (!_isImageReady)
                        {
                            _eventManager.SetEvent(PlayerState.ImageReady, _videoTexture);
                            _isImageReady = true;
                        }
                    }

                    if (!_isReady && (hasVideo ? (_videoTexture != null && _videoBuffer != null) : tracks != null))
                    {
                        _isReady = true;

                        if (_isLoad)
                        {
                            _eventManager.ReplaceEvent(PlayerState.Paused, PlayerState.Prepared, new Vector2(VideoWidth, VideoHeight));
                            Pause();
                        }
                        else
                        {
                            _eventManager.SetEvent(PlayerState.Prepared, new Vector2(VideoWidth, VideoHeight));
                            _eventManager.SetEvent(PlayerState.Playing);
                        }
                    }
                }

                yield return null;
            }
        }

        private IEnumerator InitAudioOutput()
        {
            while (true)
            {
                var paramsLine = _wrapper.NativeGetAudioParams('@');
                int channels, rate = 0;

                if (!string.IsNullOrEmpty(paramsLine))
                {
                    var options = paramsLine.Split('@');
                    if (options.Length >= 1)
                    {
                        int.TryParse(options[0], out channels);
                        int.TryParse(options[1], out rate);

                        if (channels > 0 || rate > 0)
                            break;
                    }

                }

                yield return null;
            }
        }

        private void OnAudioFilterRead(int id, float[] data, AudioOutput.AudioChannels audioChannel)
        {
            var samplesCount = _wrapper.NativeGetAudioSamples(IntPtr.Zero, 0, audioChannel);

            if (samplesCount >= data.Length)
            {
                if (!_audioDataHandle.IsAllocated)
                    _audioDataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

                _wrapper.NativeGetAudioSamples(_audioDataHandle.AddrOfPinnedObject(), data.Length, audioChannel);
                _audioManager.SetOutputData(id, data);
            }

            if (_audioManager.OutputsDataUpdated)
            {
                _wrapper.NativeClearAudioSamples(data.Length);
                _audioManager.ResetOutputsData();
            }
        }

        private void OnPlayerPlaying()
        {
            _isPlaying = true;
        }

        private void OnPlayerPaused()
        {
            _isPlaying = false;
        }
        #endregion

        #region Public methods 
        public GameObject[] VideoOutputObjects
        {
            set
            {
                _videoOutputObjects = value;
                MediaPlayerHelper.ApplyTextureToRenderingObjects(_videoTexture, _videoOutputObjects);
            }

            get { return _videoOutputObjects; }
        }

        public PlayerManagerEvents EventManager
        {
            get { return _eventManager; }
        }

        public PlayerOptions Options
        {
            get
            {
                return _options;
            }
        }

        public PlayerState State
        {
            get
            {
                if (_eventManagerPtr != IntPtr.Zero)
                    return _wrapper.PlayerGetState();

                return PlayerState.Empty;
            }
        }

        public object StateValue
        {
            get
            {
                if (_eventManagerPtr != IntPtr.Zero)
                    return _wrapper.PlayerGetStateValue();

                return null;
            }
        }

        public void AddMediaListener(IMediaListener listener)
        {
            if (_eventManager != null)
            {
                _eventManager.PlayerOpeningListener += listener.OnPlayerOpening;
                _eventManager.PlayerBufferingListener += listener.OnPlayerBuffering;
                _eventManager.PlayerImageReadyListener += listener.OnPlayerImageReady;
                _eventManager.PlayerPreparedListener += listener.OnPlayerPrepared;
                _eventManager.PlayerPlayingListener += listener.OnPlayerPlaying;
                _eventManager.PlayerPausedListener += listener.OnPlayerPaused;
                _eventManager.PlayerStoppedListener += listener.OnPlayerStopped;
                _eventManager.PlayerEndReachedListener += listener.OnPlayerEndReached;
                _eventManager.PlayerEncounteredErrorListener += listener.OnPlayerEncounteredError;
            }
        }

        public void RemoveMediaListener(IMediaListener listener)
        {
            if (_eventManager != null)
            {
                _eventManager.PlayerOpeningListener -= listener.OnPlayerOpening;
                _eventManager.PlayerBufferingListener -= listener.OnPlayerBuffering;
                _eventManager.PlayerImageReadyListener -= listener.OnPlayerImageReady;
                _eventManager.PlayerPreparedListener -= listener.OnPlayerPrepared;
                _eventManager.PlayerPlayingListener -= listener.OnPlayerPlaying;
                _eventManager.PlayerPausedListener -= listener.OnPlayerPaused;
                _eventManager.PlayerStoppedListener -= listener.OnPlayerStopped;
                _eventManager.PlayerEndReachedListener -= listener.OnPlayerEndReached;
                _eventManager.PlayerEncounteredErrorListener -= listener.OnPlayerEncounteredError;
            }
        }

        public void Prepare()
        {
            _isLoad = true;
            Play();
        }

        /// <summary>
        /// Play or resume (True if playback started (and was already started), or False on error.
        /// </summary>
        /// <returns></returns>
        public bool Play()
        {
            if (_playerObj != IntPtr.Zero)
            {
                if (!_isStarted)
                {
                    if (_eventManager != null)
                        _eventManager.StartListener();

                    if (_logManager != null)
                        _logManager.StartListener();

                    if (_options.FixedVideoSize != Vector2.zero)
                        InitBufferSize((int)_options.FixedVideoSize.x, (int)_options.FixedVideoSize.y);

                    _wrapper.NativeUpdateFramesCounter(0);
                }

                if (_updateVideoTextureEnum == null)
                {
                    _updateVideoTextureEnum = UpdateVideoTexture();
                    _monoObject.StartCoroutine(_updateVideoTextureEnum);
                }

                _isStarted = _wrapper.PlayerPlay(_playerObj);

                if (_isStarted)
                {
                    if (_audioManager != null)
                        _audioManager.Play();

                    if (_isReady && !_isPlaying)
                        _eventManager.SetEvent(PlayerState.Playing);
                }
                else
                {
                    Stop();
                }
            }

            return _isStarted;
        }
        
        /// <summary>
        /// Pause current video playback
        /// </summary>
        public void Pause()
        {
            if (_playerObj != IntPtr.Zero)
            {
                if (_videoOutputObjects == null && _updateVideoTextureEnum != null)
                {
                    _monoObject.StopCoroutine(_updateVideoTextureEnum);
                    _updateVideoTextureEnum = null;
                }

                _wrapper.PlayerPause(_playerObj);

                if (_audioManager != null && _videoOutputObjects != null)
                    _audioManager.Pause();
            }
        }

        /// <summary>
        /// Stop current video playback
        /// </summary>
        /// <param name="resetTexture">Clear previous playback texture</param>
        public void Stop(bool resetTexture)
        {
            if (_playerObj != IntPtr.Zero && _isStarted)
            {
                _wrapper.PlayerStop(_playerObj);

                if (_updateVideoTextureEnum != null)
                {
                    _monoObject.StopCoroutine(_updateVideoTextureEnum);
                    _updateVideoTextureEnum = null;
                }

                _framesCounter = 0;
                _frameRate = 0;
                _tmpFramesCounter = 0;
                _tmpTime = 0;

                _isStarted = false;
                _isPlaying = false;
                _isLoad = false;
                _isReady = false;
                _isImageReady = false;

                _wrapper.NativeUpdateFramesCounter(0);
                _wrapper.NativeClearAudioSamples(0);
                _wrapper.NativePixelsBufferRelease();

                _isTextureExist = !resetTexture;

                if (resetTexture)
                {
                    if (_videoTexture != null)
                    {
                        UnityEngine.Object.Destroy(_videoTexture);
                        _videoTexture = null;
                    }

                    if (_videoBuffer != null)
                    {
                        _videoBuffer.ClearFramePixels();
                        _videoBuffer = null;
                    }
                }

                if (_audioDataHandle.IsAllocated)
                    _audioDataHandle.Free();

                if (_audioManager != null)
                    _audioManager.Stop();

                if (_eventManager != null)
                    _eventManager.StopListener();

                if (_logManager != null)
                    _logManager.StopListener();
            }
        }

        /// <summary>
        /// Stop current video playback
        /// </summary>
        public void Stop()
        {
            Stop(true);
        }

        /// <summary>
        /// Release current video player
        /// </summary>
        public void Release()
        {
            if (_playerObj != IntPtr.Zero)
                Stop();

            if (_eventManager != null)
            {
                _eventManager.RemoveAllEvents();
                _eventManager = null;

                if (_eventHandlerPtr != IntPtr.Zero)
                    EventsDettach(_eventManagerPtr, _eventHandlerPtr);
            }

            if (_logManager != null)
            {
                _logManager.RemoveAllEvents();

                if (_logDetail != LogLevels.Disable && _vlcObj != IntPtr.Zero)
                    _wrapper.ExpandedLogUnset(_vlcObj);
            }

            if (_audioManager != null)
                _audioManager.RemoveAllListeners();

            if (_playerObj != IntPtr.Zero)
                _wrapper.PlayerRelease(_playerObj);
            _playerObj = IntPtr.Zero;

            if (_vlcObj != IntPtr.Zero)
                _wrapper.ExpandedLibVLCRelease(_vlcObj);
            _vlcObj = IntPtr.Zero;
        }

        public string DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                if (_playerObj != IntPtr.Zero)
                {
                    _dataSource = value;

                    if (_mediaObj != IntPtr.Zero)
                        _wrapper.ExpandedMediaRelease(_mediaObj);

                    _mediaObj = _wrapper.ExpandedMediaNewLocation(_vlcObj, MediaPlayerHelper.GetDataSourcePath(_dataSource));

                    if (_arguments != null)
                    {
                        foreach (string option in _arguments)
                        {
                            if (option.Contains(":"))
                                _wrapper.ExpandedAddOption(_mediaObj, option);
                        }
                    }

                    _wrapper.ExpandedSetMedia(_playerObj, _mediaObj);
                }
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
        }

        public bool IsReady
        {
            get { return _isReady; }
        }

        public bool AbleToPlay
        {
            get
            {
                if (_playerObj != IntPtr.Zero)
                    return _wrapper.PlayerWillPlay(_playerObj);

                return false;
            }
        }

        /// <summary>
        /// Get the current movie length (in ms).
        /// </summary>
        /// <returns></returns>
        public long Length
        {
            get
            {
                if (_playerObj != IntPtr.Zero)
                    return _wrapper.PlayerGetLength(_playerObj);

                return 0;
            }
        }

        /// <summary>
        /// Get the current movie formatted length (hh:mm:ss[:ms]).
        /// </summary>
        /// <param name="detail">True: formatted length will be with [:ms]</param>
        /// <returns></returns>
        public string GetFormattedLength(bool detail)
        {
            var length = TimeSpan.FromMilliseconds(Length);

            var format = detail ? "{0:D2}:{1:D2}:{2:D2}:{3:D3}" : "{0:D2}:{1:D2}:{2:D2}";

            return string.Format(format,
                length.Hours,
                length.Minutes,
                length.Seconds,
                length.Milliseconds);
        }

        public float FrameRate
        {
            get { return _frameRate; }
        }

        public byte[] FramePixels
        {
            get
            {
                if (_videoBuffer != null)
                    return _videoBuffer.FramePixels;

                return new byte[] { };
            }
        }

        public int FramesCounter
        {
            get { return _wrapper.NativeGetFramesCounter(); }
        }

        public long Time
        {
            get
            {
                if (_playerObj != IntPtr.Zero)
                    return _wrapper.PlayerGetTime(_playerObj);

                return 0;
            }
            set
            {
                _wrapper.NativeClearAudioSamples(0);

                if (_playerObj != IntPtr.Zero)
                    _wrapper.PlayerSetTime(value, _playerObj);
            }
        }

        public float Position
        {
            get
            {
                if (_playerObj != IntPtr.Zero)
                    return _wrapper.PlayerGetPosition(_playerObj);

                return 0;
            }
            set
            {
                _wrapper.NativeClearAudioSamples(0);

                if (_playerObj != IntPtr.Zero)
                    _wrapper.PlayerSetPosition(value, _playerObj);

                _wrapper.NativeUpdateFramesCounter(FramesAmount > 0 ? (int)(value * FramesAmount) : 0);
            }
        }

        public float PlaybackRate
        {
            get
            {
                if (_playerObj != IntPtr.Zero)
                    return _wrapper.PlayerGetRate(_playerObj);

                return 0;
            }
            set
            {
                if (_playerObj != IntPtr.Zero)
                {
                    bool res = _wrapper.PlayerSetRate(value, _playerObj);
                    if (!res)
                    {
                        throw new Exception("Native library problem: can't change playback rate");
                    }
                }
            }
        }

        public int Volume
        {
            get
            {
                if (_playerObj != IntPtr.Zero)
                    return _wrapper.PlayerGetVolume(_playerObj);

                return 0;
            }
            set
            {
                if (_playerObj != IntPtr.Zero)
                    _wrapper.PlayerSetVolume(value, _playerObj);
            }
        }

        public bool Mute
        {
            get
            {
                if (_playerObj != IntPtr.Zero)
                    return _wrapper.PlayerGetMute(_playerObj);

                return false;
            }
            set
            {
                if (_playerObj != IntPtr.Zero)
                    _wrapper.PlayerSetMute(value, _playerObj);
            }
        }

        public int VideoWidth
        {
            get
            {
                var width = 0;
                if (_playerObj != IntPtr.Zero)
                {
                    width = _wrapper.PlayerVideoWidth(_playerObj);

                    if (_videoBuffer != null && (width <= 0 || _options.FixedVideoSize != Vector2.zero ||
                        _options.VideoBufferSize))
                        width = _videoBuffer.Width;
                }

                return width;
            }
        }

        public int VideoHeight
        {
            get
            {
                var height = 0;

                if (_playerObj != IntPtr.Zero)
                {
                    height = _wrapper.PlayerVideoHeight(_playerObj);

                    if (_videoBuffer != null && (height <= 0 || _options.FixedVideoSize != Vector2.zero ||
                        _options.VideoBufferSize))
                        height = _videoBuffer.Height;
                }

                return height;
            }
        }

        public Vector2 VideoSize
        {
            get
            {
                return new Vector2(VideoWidth, VideoHeight);
            }
        }

        /// <summary>
        /// Get available audio tracks.
        /// </summary>
        public MediaTrackInfo[] AudioTracks
        {
            get {
                return _playerObj != IntPtr.Zero ? _wrapper.PlayerAudioGetTracks(_playerObj) : null;
            }
        }

        /// <summary>
        /// Get/Set current audio track.
        /// </summary>
        public MediaTrackInfo AudioTrack
        {
            get
            {
                if (_playerObj == IntPtr.Zero) return null;
                
                var id = _wrapper.PlayerAudioGetTrack(_playerObj);
                return AudioTracks.SingleOrDefault(t => t.Id == id);
            }
            set
            {
                var status = _wrapper.PlayerAudioSetTrack(value.Id, _playerObj);

                if (status == -1)
                    throw new Exception("Native library problem: can't set new audio track");
            }
        }

        /// <summary>
        /// Get available spu tracks.
        /// </summary>
        public MediaTrackInfo[] SpuTracks
        {
            get {
                return _playerObj != IntPtr.Zero ? _wrapper.PlayerSpuGetTracks(_playerObj) : null;
            }
        }

        /// <summary>
        /// Get/Set current spu track.
        /// </summary>
        public MediaTrackInfo SpuTrack
        {
            get
            {
                if (_playerObj == IntPtr.Zero) return null;
                
                var id = _wrapper.PlayerSpuGetTrack(_playerObj);
                return SpuTracks.SingleOrDefault(t => t.Id == id);
            }
            set
            {
                var status = _wrapper.PlayerSpuSetTrack(value.Id, _playerObj);

                if (status == -1)
                    throw new Exception("Native library problem: can't set new spu track");
            }
        }

        #region Platform dependent functionality
        /// <summary>
        /// Set new video subtitle file
        /// </summary>
        /// <param name="path">Path to the new video subtitle file</param>
        /// <returns></returns>
        public bool SetSubtitleFile(Uri path)
        {
            if (_playerObj != IntPtr.Zero)
                return _wrapper.ExpandedSpuSetFile(_playerObj, path.AbsolutePath) == 1;

            return false;
        }

        /// <summary>
        /// Get the current statistics about the media
        /// </summary>
        public MediaStats MediaStats
        {
            get
            {
                if (_mediaObj != IntPtr.Zero)
                    _wrapper.ExpandedMediaGetStats(_mediaObj, out _mediaStats);

                return _mediaStats;
            }
        }

        /// <summary>
        /// Get media descriptor's elementary streams description.
        /// </summary>
        public MediaTrackInfoExpanded[] TracksInfo
        {
            get
            {
                if (_mediaObj != IntPtr.Zero)
                {
                    var tracks = _wrapper.ExpandedMediaGetTracksInfo(_mediaObj);

                    if (tracks != null && tracks.Length > 0)
                    {
                        var result = new List<MediaTrackInfoExpanded>(tracks.Length);
                        foreach (var trackInfo in tracks)
                        {
                            switch (trackInfo.Type)
                            {
                                case TrackTypes.Unknown:
                                    result.Add(new MediaTrackInfoUnknown(trackInfo.Codec, trackInfo.Id, trackInfo.Profile, trackInfo.Level));
                                    break;

                                case TrackTypes.Video:
                                    result.Add(new MediaTrackInfoVideo(trackInfo.Codec, trackInfo.Id, trackInfo.Profile, trackInfo.Level, trackInfo.Video.Width, trackInfo.Video.Height));
                                    break;

                                case TrackTypes.Audio:
                                    result.Add(new MediaTrackInfoAudio(trackInfo.Codec, trackInfo.Id, trackInfo.Profile, trackInfo.Level, trackInfo.Audio.Channels, trackInfo.Audio.Rate));
                                    break;

                                case TrackTypes.Text:
                                    result.Add(new MediaTrackInfoSpu(trackInfo.Codec, trackInfo.Id, trackInfo.Profile, trackInfo.Level));
                                    break;
                            }
                        }
                        return result.ToArray();
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Get/Set current audio delay.
        /// </summary>
        public long AudioDelay
        {
            get
            {
                if (_playerObj != IntPtr.Zero)
                    return _wrapper.ExpandedGetAudioDelay(_playerObj);

                return 0;
            }
            set
            {
                if (_playerObj != IntPtr.Zero)
                    _wrapper.ExpandedSetAudioDelay(_playerObj, value);
            }
        }

        /// <summary>
        /// Get/Set video scale.
        /// </summary>
        public float VideoScale
        {
            get
            {
                if (_playerObj != IntPtr.Zero)
                    return _wrapper.ExpandedVideoGetScale(_playerObj);

                return 0;
            }
            set
            {
                if (_playerObj != IntPtr.Zero)
                    _wrapper.ExpandedVideoSetScale(_playerObj, value);
            }
        }

        /// <summary>
        /// Get parsed media fps rate
        /// </summary>
        public float FrameRateStable
        {
            get
            {
                if (_playerObj != IntPtr.Zero && IsReady)
                    return _wrapper.ExpandedVideoFrameRate(_playerObj);

                return 0;
            }
        }

        /// <summary>
        /// Get current video frames amount
        /// </summary>
        public int FramesAmount
        {
            get
            {
                if (_playerObj != IntPtr.Zero && IsReady)
                    return (int)(Length * FrameRateStable * 0.001f);

                return 0;
            }
        }

        /// <summary>
        /// Take a snapshot of the current video window.
        /// </summary>
        /// <param name="path">The path of a file or a folder to save the screenshot into</param>
        public void TakeSnapShot(string path)
        {
            if (_playerObj != IntPtr.Zero)
                _wrapper.ExpandedVideoTakeSnapshot(_playerObj, 0, path, 0, 0);
        }

        public string LogMessage
        {
            get
            {
                if (_vlcObj != IntPtr.Zero)
                    return _wrapper.NativeGetLogMessage();

                return null;
            }
        }

        public int LogLevel
        {
            get
            {
                if (_vlcObj != IntPtr.Zero)
                    return _wrapper.NativeGetLogLevel();

                return -1;
            }
        }

        public string GetLastError()
        {
            if (_logManager != null)
                return _logManager.LastError;

            return string.Empty;
        }
        #endregion
        #endregion
    }
}