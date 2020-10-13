using System;
using System.Runtime.InteropServices;

namespace UMP.Wrappers
{
    internal class WrapperInternal : IWrapperNative, IWrapperPlayer
    {
        #region iOS/WebGL Imports
#if UNITY_IOS || UNITY_WEBGL
        private int _nativeIndex;

        [DllImport("__Internal")]
        private static extern IntPtr UMPNativeInit();

        [DllImport("__Internal")]
        private static extern void UMPNativeInitPlayer(int index, string options);

        [DllImport("__Internal")]
        private static extern void UMPNativeUpdateTexture(int index, IntPtr texture);

        [DllImport("__Internal")]
        private static extern IntPtr UMPNativeGetTexturePointer(int index);

        [DllImport("__Internal")]
        private static extern void UMPNativeSetPixelsBuffer(int index, IntPtr buffer, int width, int height);

        [DllImport("__Internal")]
        private static extern void UMPNativeUpdateFrameBuffer(int index);

        [DllImport("__Internal")]
        private static extern void UMPSetDataSource(int index, string path);

        [DllImport("__Internal")]
        private static extern bool UMPPlay(int index);

        [DllImport("__Internal")]
        private static extern void UMPPause(int index);

        [DllImport("__Internal")]
        private static extern void UMPStop(int index);

        [DllImport("__Internal")]
        private static extern void UMPRelease(int index);

        [DllImport("__Internal")]
        private static extern bool UMPIsPlaying(int index);

        [DllImport("__Internal")]
        private static extern bool UMPIsReady(int index);

        [DllImport("__Internal")]
        private static extern int UMPGetLength(int index);

        [DllImport("__Internal")]
        private static extern int UMPGetTime(int index);

        [DllImport("__Internal")]
        private static extern void UMPSetTime(int index, int time);

        [DllImport("__Internal")]
        private static extern float UMPGetPosition(int index);

        [DllImport("__Internal")]
        private static extern void UMPSetPosition(int index, float position);

        [DllImport("__Internal")]
        private static extern float UMPGetRate(int index);

        [DllImport("__Internal")]
        private static extern void UMPSetRate(int index, float rate);

        [DllImport("__Internal")]
        private static extern int UMPGetVolume(int index);

        [DllImport("__Internal")]
        private static extern void UMPSetVolume(int index, int volume);

        [DllImport("__Internal")]
        private static extern bool UMPGetMute(int index);

        [DllImport("__Internal")]
        private static extern void UMPSetMute(int index, bool state);

        [DllImport("__Internal")]
        private static extern int UMPVideoWidth(int index);

        [DllImport("__Internal")]
        private static extern int UMPVideoHeight(int index);

        [DllImport("__Internal")]
        private static extern int UMPVideoFrameCount(int index);

        [DllImport("__Internal")]
        private static extern int UMPGetState(int index);

        [DllImport("__Internal")]
        private static extern float UMPGetStateFloatValue(int index);

        [DllImport("__Internal")]
        private static extern long UMPGetStateLongValue(int index);

        [DllImport("__Internal")]
        private static extern IntPtr UMPGetStateStringValue(int index);
#endif
        #endregion

        public WrapperInternal(PlayerOptionsIPhone options)
        {
#if UNITY_IOS || UNITY_WEBGL
            _nativeIndex = (int)UMPNativeInit();
#endif
        }

        #region Native
        public int NativeIndex
        {
            get
            {
#if UNITY_IOS || UNITY_WEBGL
                return _nativeIndex;
#else
                return 0;
#endif
            }
        }

        public void NativeInitPlayer(string options)
        {
#if UNITY_IOS || UNITY_WEBGL
            UMPNativeInitPlayer(_nativeIndex, options);
#endif
        }

        public IntPtr NativeGetTexture()
        {
#if UNITY_IOS || UNITY_WEBGL
            return UMPNativeGetTexturePointer(_nativeIndex);
#else
            return IntPtr.Zero;
#endif
        }

        public void NativeUpdateTexture(IntPtr texture)
        {
#if UNITY_IOS || UNITY_WEBGL
            UMPNativeUpdateTexture(_nativeIndex, texture);
#endif
        }

        public void NativeSetPixelsBuffer(IntPtr buffer, int width, int height)
        {
#if UNITY_IOS || UNITY_WEBGL
            UMPNativeSetPixelsBuffer(_nativeIndex, buffer, width, height);
#endif
        }

        public void NativeUpdatePixelsBuffer()
        {
#if UNITY_IOS || UNITY_WEBGL
            UMPNativeUpdateFrameBuffer(_nativeIndex);
#endif
        }
#endregion

#region Player
        public void PlayerSetDataSource(string path, object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                UMPSetDataSource(_nativeIndex, path);
#endif
        }

        public bool PlayerPlay(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            return UMPPlay(_nativeIndex);
#else
            return false;
#endif
        }

        public void PlayerPause(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                UMPPause(_nativeIndex);
#endif
        }

        public void PlayerStop(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                UMPStop(_nativeIndex);
#endif
        }

        public void PlayerRelease(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                UMPRelease(_nativeIndex);
#endif
        }

        public bool PlayerIsPlaying(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPIsPlaying(_nativeIndex);
#endif
            return false;
        }

        public bool PlayerIsReady(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPIsReady(_nativeIndex);
#endif
            return false;
        }

        public long PlayerGetLength(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPGetLength(_nativeIndex);
#endif
            return 0;
        }

        public long PlayerGetTime(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPGetTime(_nativeIndex);
#endif
            return 0;
        }

        public void PlayerSetTime(long time, object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                UMPSetTime(_nativeIndex, (int)time);
#endif
        }

        public float PlayerGetPosition(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPGetPosition(_nativeIndex);
#endif
            return 0;
        }

        public void PlayerSetPosition(float pos, object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                UMPSetPosition(_nativeIndex, pos);
#endif
        }

        public float PlayerGetRate(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPGetRate(_nativeIndex);
#endif
            return 1;
        }

        public bool PlayerSetRate(float rate, object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                UMPSetRate(_nativeIndex, rate);
#endif
            return true;
        }

        public int PlayerGetVolume(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPGetVolume(_nativeIndex);
#endif
            return 0;
        }

        public int PlayerSetVolume(int volume, object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                UMPSetVolume(_nativeIndex, volume);
#endif
            return 1;
        }

        public bool PlayerGetMute(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPGetMute(_nativeIndex);
#endif
            return false;
        }

        public void PlayerSetMute(bool mute, object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                UMPSetMute(_nativeIndex, mute);
#endif
        }

        public int PlayerVideoWidth(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPVideoWidth(_nativeIndex);
#endif
            return 0;
        }

        public int PlayerVideoHeight(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPVideoHeight(_nativeIndex);
#endif
            return 0;
        }

        public int PlayerVideoFramesCounter(object playerObject = null)
        {
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                return UMPVideoFrameCount(_nativeIndex);
#endif
            return 0;
        }

        public PlayerState PlayerGetState()
        {
            var eventValue = PlayerState.Empty;
#if UNITY_IOS || UNITY_WEBGL
            if (_nativeIndex >= 0)
                eventValue = (PlayerState)UMPGetState(_nativeIndex);
#endif
            return eventValue;
        }

        public object PlayerGetStateValue()
        {
#if UNITY_IOS || UNITY_WEBGL
            object value = UMPGetStateFloatValue(_nativeIndex);

            if ((float)value < 0)
            {
                value = UMPGetStateLongValue(_nativeIndex);
                if ((long)value < 0)
                    value = UMPGetStateStringValue(_nativeIndex);
            }

            return value;
#else
            return null;
#endif
        }
#endregion
    }
}
