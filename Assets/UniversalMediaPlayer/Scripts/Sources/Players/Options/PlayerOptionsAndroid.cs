using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMP
{
    public class PlayerOptionsAndroid : PlayerOptions
    {
        private const string NETWORK_CACHING_KEY = "--network-caching";
        private const string CR_AVERAGE_KEY = ":cr-average";
        private const string CLOCK_SYNCHRO_KEY = ":clock-synchro";
        private const string CLOCK_JITTER_KEY = ":clock-jitter";

        private const string PLAYER_TYPE_KEY = "player-type";
        private const string HARDWARE_ACCELERATION_STATE_KEY = ":hw-state";
        private const string OPENGL_DECODING_STATE_KEY = "opengl-state";
        private const string OPENGL_DECODING_KEY = "--vout";
        private const string VIDEO_CHROMA_STATE_KEY = "chroma-state";
        private const string VIDEO_CHROMA_KEY = "--android-display-chroma";

        private const string PLAY_IN_BACKGROUND_KEY = ":play-in-background";
        private const string RTSP_OVER_TCP_KEY = ":rtsp-tcp";

        private const int DEFAULT_CR_AVERAGE_VALUE = 40;
        private const int DEFAULT_CLOCK_JITTER_VALUE = 5000;

        public enum PlayerTypes
        {
            Native = 1,
            LibVLC = 2,
            Exo = 4
        }

        public enum DecodingStates
        {
            Automatic = -1,
            Disabled = 0,
            DecodingAcceleration = 1,
            FullAcceleration = 2
        }

        public enum ChromaTypes
        {
            RGB32Bit,
            RGB16Bit,
            YUV
        }

        public PlayerOptionsAndroid(string[] options) : base(options)
        {
            PlayerType = PlayerTypes.Exo;
            NetworkCaching = DEFAULT_CACHING_VALUE;
            CrAverage = DEFAULT_CR_AVERAGE_VALUE;
            ClockSynchro = States.Default;
            ClockJitter = DEFAULT_CLOCK_JITTER_VALUE;
            HardwareAcceleration = DecodingStates.Automatic;
        }

        /// <summary>
        /// This allows to choose the usable player.
        /// </summary>
        public PlayerTypes PlayerType
        {
            get
            {
                return (PlayerTypes)GetValue<int>(PLAYER_TYPE_KEY);
            }
            set
            {
                var settings = UMPSettings.Instance;
                var playerTypes = Enum.GetValues(typeof(PlayerTypes));
                var result = PlayerTypes.Native;

                foreach (var type in playerTypes)
                {
                    var playerType = (PlayerTypes)type;

                    if ((settings.PlayersAndroid & playerType) == playerType)
                    {
                        result = playerType;

                        if (result == value)
                            break;
                    }
                }

                SetValue(PLAYER_TYPE_KEY, ((int)result).ToString());
            }
        }

        /// <summary>
        /// This allows hardware decoding when available.
        /// </summary>
        public DecodingStates HardwareAcceleration
        {
            get
            {
                return (DecodingStates)GetValue<int>(HARDWARE_ACCELERATION_STATE_KEY);
            }
            set
            {
                SetValue(HARDWARE_ACCELERATION_STATE_KEY, ((int)value).ToString());
            }
        }

        /// <summary>
        /// OpenGL ES2 is used for software decoding and hardware decoding when needed (360° videos), but can affect on correct video rendering.
        /// </summary>
        public States OpenGLDecoding
        {
            get
            {
                return (States)GetValue<int>(OPENGL_DECODING_STATE_KEY);
            }
            set
            {
                SetValue(OPENGL_DECODING_STATE_KEY, ((int)value).ToString());

                switch (value)
                {
                    case States.Default:
                        RemoveOption(OPENGL_DECODING_KEY);
                        break;

                    case States.Enable:
                        SetValue(OPENGL_DECODING_KEY, "gles2,none");
                        break;

                    default:
                        SetValue(OPENGL_DECODING_KEY, "android_display,none");
                        break;
                }
            }
        }

        /// <summary>
        /// Force video chroma.
        /// </summary>
        public ChromaTypes VideoChroma
        {
            get
            {
                return (ChromaTypes)GetValue<int>(VIDEO_CHROMA_STATE_KEY);
            }
            set
            {
                SetValue(VIDEO_CHROMA_STATE_KEY, ((int)value).ToString());

                switch (value)
                {
                    case ChromaTypes.RGB16Bit:
                        SetValue(VIDEO_CHROMA_KEY, "RV16");
                        break;

                    case ChromaTypes.YUV:
                        SetValue(VIDEO_CHROMA_KEY, "YV12");
                        break;

                    default:
                        SetValue(VIDEO_CHROMA_KEY, "RV32");
                        break;
                }
            }
        }

        /// <summary>
        /// Continue play video when application in background.
        /// </summary>
        public bool PlayInBackground
        {
            get
            {
                return GetValue<bool>(PLAY_IN_BACKGROUND_KEY);
            }
            set
            {
                if (value)
                    SetValue(PLAY_IN_BACKGROUND_KEY, string.Empty);
                else
                    RemoveOption(PLAY_IN_BACKGROUND_KEY);
            }
        }

        /// <summary>
        /// Use RTP over RTSP (TCP) (default disabled).
        /// </summary>
        public bool UseTCP
        {
            get
            {
                return GetValue<bool>(RTSP_OVER_TCP_KEY);
            }
            set
            {
                if (value)
                    SetValue(RTSP_OVER_TCP_KEY, string.Empty);
                else
                    RemoveOption(RTSP_OVER_TCP_KEY);
            }
        }

        /// <summary>
        /// The amount of time to buffer network media (in ms). Does not work with hardware decoding. Leave '0' to reset.
        /// </summary>
        public int NetworkCaching
        {
            get
            {
                return GetValue<int>(NETWORK_CACHING_KEY);
            }
            set
            {
                if (value > 0)
                    SetValue(NETWORK_CACHING_KEY, value.ToString());
                else
                    RemoveOption(NETWORK_CACHING_KEY);
            }
        }

        /// <summary>
        /// When using the PVR input (or a very irregular source), you should set this to 10000.
        /// </summary>
        public int CrAverage
        {
            get
            {
                return GetValue<int>(CR_AVERAGE_KEY);
            }
            set
            {
                SetValue(CR_AVERAGE_KEY, value.ToString());
            }
        }
        
        /// <summary>
        /// It is possible to disable the input clock synchronisation for
        /// real-time sources.Use this if you experience jerky playback of
        /// network streams.
        /// </summary>
        public States ClockSynchro
        {
            get
            {
                return (States)GetValue<int>(CLOCK_SYNCHRO_KEY);
            }
            set
            {
                SetValue(CR_AVERAGE_KEY, ((int)value).ToString());
            }
        }

        /// <summary>
        /// This defines the maximum input delay jitter that the synchronization
        /// algorithms should try to compensate(in milliseconds).
        /// </summary>
        public int ClockJitter
        {
            get
            {
                return GetValue<int>(CLOCK_JITTER_KEY);
            }
            set
            {
                SetValue(CLOCK_JITTER_KEY, value.ToString());
            }
        }
    }
}