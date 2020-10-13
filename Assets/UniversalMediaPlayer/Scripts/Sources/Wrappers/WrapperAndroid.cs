using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UMP.Wrappers
{
    internal class WrapperAndroid : IWrapperNative, IWrapperPlayer, IWrapperSpu, IWrapperAudio
    {
        private const string CLASS_PATH_CORE = "unitydirectionkit/universalmediaplayer/core/UniversalMediaPlayer";
        private const string CLASS_PATH_NATIVE = "unitydirectionkit/universalmediaplayer/nativeplayer/MediaPlayerNative";
        private const string CLASS_PATH_LIBVLC = "unitydirectionkit/universalmediaplayer/libvlcplayer/MediaPlayerVLC";
        private const string CLASS_PATH_EXO = "unitydirectionkit/universalmediaplayer/exoplayer/MediaPlayerExo";

        private AndroidJavaObject _coreObj;
        private object _playerExpanded;

        private int _nativeIndex;

        #region Native Imports
        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern int UMPNativeInit();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativeUpdateIndex(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativeSetTexture(int index, IntPtr texture);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativeSetPixelsBuffer(int index, IntPtr buffer, int width, int height);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativeUpdateFrameBuffer(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetUnityRenderCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetPlayCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetUpdateSurfaceTextureCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetUpdateFrameBufferCallback();
        #endregion

        public WrapperAndroid(PlayerOptionsAndroid options)
        {
            _nativeIndex = UMPNativeInit();

            AndroidJavaObject player = null;

            if (options.PlayerType == PlayerOptionsAndroid.PlayerTypes.Native)
            {
                player = new AndroidJavaObject(CLASS_PATH_NATIVE, new object[0]);
            }
            else if (options.PlayerType == PlayerOptionsAndroid.PlayerTypes.LibVLC)
            {
                player = new AndroidJavaObject(CLASS_PATH_LIBVLC, new object[0]);
                _playerExpanded = new ExpandedAndroidLibVLC(player);
            }
            else if (options.PlayerType == PlayerOptionsAndroid.PlayerTypes.Exo)
            {
                player = new AndroidJavaObject(CLASS_PATH_EXO, new object[0]);
            }

            var arg = new object[3];
            arg[0] = _nativeIndex;
            arg[1] = player;
            arg[2] = options.GetOptions('\n');

            _coreObj = new AndroidJavaObject(CLASS_PATH_CORE, arg);
        }

        #region Native
        public int NativeIndex
        {
            get
            {
                return _nativeIndex;
            }
        }

        public void NativeUpdateIndex()
        {
            UMPNativeUpdateIndex(_nativeIndex);
        }

        public void NativeSetTexture(IntPtr texture)
        {
            UMPNativeSetTexture(_nativeIndex, texture);
        }

        public void NativeSetPixelsBuffer(IntPtr buffer, int width, int height)
        {
            UMPNativeSetPixelsBuffer(_nativeIndex, buffer, width, height);
        }

        public void NativeUpdatePixelsBuffer()
        {
            if (SystemInfo.graphicsMultiThreaded)
                GL.IssuePluginEvent(UMPNativeGetUpdateFrameBufferCallback(), _nativeIndex);
            else
                UMPNativeUpdateFrameBuffer(_nativeIndex);
        }

        public IntPtr NativeGetUnityRenderCallback()
        {
            return UMPNativeGetUnityRenderCallback();
        }
        #endregion

        #region Player
        public void PlayerSetDataSource(string path, object playerObject = null)
        {
            if (_coreObj != null)
                _coreObj.Call("exportSetDataSource", path);
        }

        public bool PlayerPlay(object playerObject = null)
        {
            var started = false;

            if (SystemInfo.graphicsMultiThreaded)// && !_isReady)
            {
                GL.IssuePluginEvent(UMPNativeGetPlayCallback(), (int)_coreObj.GetRawObject());
                started = true;
            }
            else
            {
                started = _coreObj.Call<bool>("exportPlay");
            }

            return started;
        }

        public void PlayerPause(object playerObject = null)
        {
            if (_coreObj != null)
                _coreObj.Call("exportPause");
        }

        public void PlayerStop(object playerObject = null)
        {
            if (_coreObj != null)
                _coreObj.Call("exportStop");
        }

        public void PlayerRelease(object playerObject = null)
        {
            if (_coreObj != null)
                _coreObj.Call("exportRelease");
        }

        public bool PlayerIsPlaying(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<bool>("exportIsPlaying");

            return false;
        }

        public bool PlayerIsReady(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<bool>("exportIsReady");

            return false;
        }

        public long PlayerGetLength(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<long>("exportDuration");

            return 0;
        }

        public long PlayerGetTime(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<long>("exportGetTime");

            return 0;
        }

        public void PlayerSetTime(long time, object playerObject = null)
        {
            if (_coreObj != null)
                _coreObj.Call("exportSetTime", time);
        }

        public float PlayerGetPosition(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<float>("exportGetPosition");

            return 0;
        }

        public void PlayerSetPosition(float pos, object playerObject = null)
        {
            if (_coreObj != null)
                _coreObj.Call("exportSetPosition", pos);
        }

        public float PlayerGetRate(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<float>("exportGetPlaybackRate");

            return 1;
        }

        public bool PlayerSetRate(float rate, object playerObject = null)
        {
            if (_coreObj != null)
            {
                _coreObj.Call("exportSetPlaybackRate", rate);
                return true;
            }

            return false;
        }

        public int PlayerGetVolume(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<int>("exportGetVolume");

            return 0;
        }

        public int PlayerSetVolume(int volume, object playerObject = null)
        {
            if (_coreObj != null)
                _coreObj.Call("exportSetVolume", volume);

            return 1;
        }

        public bool PlayerGetMute(object playerObject = null)
        {
            // Not used in this implementation
            return false;
        }

        public void PlayerSetMute(bool mute, object playerObject = null)
        {
            // Not used in this implementation
        }

        public int PlayerVideoWidth(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<int>("exportVideoWidth");

            return 0;
        }

        public int PlayerVideoHeight(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<int>("exportVideoHeight");

            return 0;
        }

        public int PlayerVideoFramesCounter(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<int>("exportVideoFramesCounter");

            return 0;
        }

        public PlayerState PlayerGetState()
        {
            var eventValue = 0;
            if (_coreObj != null)
                eventValue = _coreObj.Call<int>("exportGetState");

            return (PlayerState)eventValue;
        }

        public object PlayerGetStateValue()
        {
            object value = _coreObj.Call<float>("exportGetStateFloatValue");

            if ((float)value < 0)
            {
                value = _coreObj.Call<long>("exportGetStateLongValue");
                if ((long)value < 0)
                    value = _coreObj.Call<string>("exportGetStateStringValue");
            }

            return value;
        }
        #endregion

        #region Player Spu
        public MediaTrackInfo[] PlayerSpuGetTracks(object playerObject = null)
        {
            var tracks = new System.Collections.Generic.List<MediaTrackInfo>();

            if (_coreObj != null)
            {
                int tracksLength = _coreObj.Call<int>("exportGetSpuTracksLength");

                for (int i = 0; i < tracksLength; i++)
                {
                    int id = _coreObj.Call<int>("exportGetSpuTrackId", i);
                    string name = _coreObj.Call<string>("exportGetSpuTrackName", i);

                    tracks.Add(new MediaTrackInfo(id, name));
                }
            }

            return tracks.ToArray();
        }

        public int PlayerSpuGetTrack(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<int>("exportGetSpuTrack");

            return 0;
        }

        public int PlayerSpuSetTrack(int spuIndex, object playerObject = null)
        {
            if (_coreObj != null)
                _coreObj.Call("exportSetSpuTrack", spuIndex);

            return 0;
        }
        #endregion

        #region Player Audio
        public MediaTrackInfo[] PlayerAudioGetTracks(object playerObject = null)
        {
            var tracks = new System.Collections.Generic.List<MediaTrackInfo>();

            if (_coreObj != null)
            {
                int tracksLength = _coreObj.Call<int>("exportGetAudioTracksLength");

                for (int i = 0; i < tracksLength; i++)
                {
                    int id = _coreObj.Call<int>("exportGetAudioTrackId", i);
                    string name = _coreObj.Call<string>("exportGetAudioTrackName", i);

                    tracks.Add(new MediaTrackInfo(id, name));
                }
            }

            return tracks.ToArray();
        }

        public int PlayerAudioGetTrack(object playerObject = null)
        {
            if (_coreObj != null)
                return _coreObj.Call<int>("exportGetAudioTrack");

            return 0;
        }

        public int PlayerAudioSetTrack(int audioIndex, object playerObject = null)
        {
            if (_coreObj != null)
                _coreObj.Call("exportSetAudioTrack", audioIndex);

            return 0;
        }
        #endregion

        #region Platform dependent functionality
        public object PlayerExpanded
        {
            get
            {
                return _playerExpanded;
            }
        }

        public void PlayerUpdateSurfaceTexture()
        {
            if (_coreObj != null)
            {
                if (SystemInfo.graphicsMultiThreaded)
                    GL.IssuePluginEvent(UMPNativeGetUpdateSurfaceTextureCallback(), (int)_coreObj.GetRawObject());
                else
                    _coreObj.Call("exportUpdateSurfaceTexture");
            }
        }

        public bool PlayerSetSubtitleFile(Uri path)
        {
            if (_coreObj != null)
            {
                var arg = new object[1];
                arg[0] = path.AbsoluteUri;

                return _coreObj.Call<bool>("exportSetSubtitleFile", arg);
            }

            return false;
        }
        #endregion
    }
}
