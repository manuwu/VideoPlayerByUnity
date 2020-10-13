using System;

namespace UMP
{
    public class PlayerOptionsIPhone : PlayerOptions
    {
        private const string PLAYER_TYPE_KEY = ":player-type";
        private const string FLIP_VERTICALLY = ":flip-vertically";

        private const string VIDEOTOOLBOX_KEY = ":videotoolbox";
        private const string VIDEOTOOLBOX_FRAME_WIDTH_KEY = ":videotoolbox-max-frame-width";
        private const string VIDEOTOOLBOX_ASYNC_KEY = ":videotoolbox-async";
        private const string VIDEOTOOLBOX_WAIT_ASYNC_KEY = ":videotoolbox-wait-async";

        private const string PACKET_BUFFERING_KEY = ":packet-buffering";
        private const string MAX_BUFFER_SIZE_KEY = ":max-buffer-size";
        private const string MIN_FRAMES_KEY = ":min-frames";
        private const string INFBUF_KEY = ":infbuf";

        private const string FRAMEDROP_KEY = ":framedrop";
        private const string MAX_FPS_KEY = ":max-fps";

        private const string PLAY_IN_BACKGROUND_KEY = ":play-in-background";
        private const string RTSP_OVER_TCP_KEY = ":rtsp-tcp";

        private const int DEFAULT_VIDEOTOOLBOX_FRAME_WIDTH_VALUE = 4096;
        private const int DEFAULT_MAX_BUFFER_SIZE_VALUE = 15 * 1024 * 1024;
        private const int DEFAULT_MIN_FRAMES_VALUE = 50000;
        private const int DEFAULT_FRAMEDROP_VALUE = 0;
        private const int DEFAULT_MAX_FPS_VALUE = 31;

        public enum PlayerTypes
        {
            Native = 1,
            FFmpeg = 2
        }

        public PlayerOptionsIPhone(string[] options) : base(options) {
            PlayerType = PlayerTypes.FFmpeg;
            FlipVertically = true;
            VideoToolbox = true;
            VideoToolboxFrameWidth = DEFAULT_VIDEOTOOLBOX_FRAME_WIDTH_VALUE;
            VideoToolboxAsync = false;
            VideoToolboxWaitAsync = true;

            PlayInBackground = false;
            UseTCP = false;
            PacketBuffering = true;
            MaxBufferSize = DEFAULT_MAX_BUFFER_SIZE_VALUE;
            MinFrames = DEFAULT_MIN_FRAMES_VALUE;
            Infbuf = false;
            Framedrop = DEFAULT_FRAMEDROP_VALUE;
            MaxFps = DEFAULT_MAX_FPS_VALUE;
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

                    if ((settings.PlayersIPhone & playerType) == playerType)
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
        /// Flip video frame vertically when we get it from native library (CPU usage cost).
        /// </summary>
        public bool FlipVertically
        {
            get
            {
                return GetValue<bool>(FLIP_VERTICALLY);
            }
            set
            {
                if (value)
                    SetValue(FLIP_VERTICALLY, string.Empty);
                else
                    RemoveOption(FLIP_VERTICALLY);
            }
        }

        /// <summary>
        /// Enable/Disable low-level framework that provides direct access to hardware encoders and decoders.
        /// </summary>
        public bool VideoToolbox
        {
            get
            {
                return GetValue<bool>(VIDEOTOOLBOX_KEY);
            }
            set
            {
                if (value)
                    SetValue(VIDEOTOOLBOX_KEY, string.Empty);
                else
                    RemoveOption(VIDEOTOOLBOX_KEY);
            }
        }

        /// <summary>
        /// Max width of output frame.
        /// </summary>
        public int VideoToolboxFrameWidth
        {
            get
            {
                return GetValue<int>(VIDEOTOOLBOX_FRAME_WIDTH_KEY);
            }
            set
            {
                SetValue(VIDEOTOOLBOX_FRAME_WIDTH_KEY, value.ToString());
            }
        }

        /// <summary>
        /// Enable VideoToolbox asynchronous decompression.
        /// </summary>
        public bool VideoToolboxAsync
        {
            get
            {
                return GetValue<bool>(VIDEOTOOLBOX_ASYNC_KEY);
            }
            set
            {
                if (VideoToolbox && value)
                    SetValue(VIDEOTOOLBOX_ASYNC_KEY, string.Empty);
                else
                    RemoveOption(VIDEOTOOLBOX_ASYNC_KEY);
            }
        }

        /// <summary>
        /// Wait for asynchronous frames.
        /// </summary>
        public bool VideoToolboxWaitAsync
        {
            get
            {
                return GetValue<bool>(VIDEOTOOLBOX_WAIT_ASYNC_KEY);
            }
            set
            {
                if (VideoToolboxAsync && value)
                    SetValue(VIDEOTOOLBOX_WAIT_ASYNC_KEY, string.Empty);
                else
                    RemoveOption(VIDEOTOOLBOX_WAIT_ASYNC_KEY);
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
        /// Pause output until enough packets have been read after stalling.
        /// </summary>
        public bool PacketBuffering
        {
            get
            {
                return GetValue<bool>(PACKET_BUFFERING_KEY);
            }
            set
            {
                if (value)
                    SetValue(PACKET_BUFFERING_KEY, string.Empty);
                else
                    RemoveOption(PACKET_BUFFERING_KEY);
            }
        }

        /// <summary>
        /// Max buffer size should be pre-read (in bytes).
        /// </summary>
        public int MaxBufferSize
        {
            get
            {
                return GetValue<int>(MAX_BUFFER_SIZE_KEY);
            }
            set
            {
                SetValue(MAX_BUFFER_SIZE_KEY, value.ToString());
            }
        }

        /// <summary>
        /// Minimal frames to stop pre-reading.
        /// </summary>
        public int MinFrames
        {
            get
            {
                return GetValue<int>(MIN_FRAMES_KEY);
            }
            set
            {
                SetValue(MIN_FRAMES_KEY, value.ToString());
            }
        }

        /// <summary>
        /// Don't limit the input buffer size (useful with realtime streams).
        /// </summary>
        public bool Infbuf
        {
            get
            {
                return GetValue<bool>(INFBUF_KEY);
            }
            set
            {
                if (value)
                    SetValue(INFBUF_KEY, string.Empty);
                else
                    RemoveOption(INFBUF_KEY);
            }
        }

        /// <summary>
        /// Drop frames when cpu is too slow.
        /// </summary>
        public int Framedrop
        {
            get
            {
                return GetValue<int>(FRAMEDROP_KEY);
            }
            set
            {
                SetValue(FRAMEDROP_KEY, value.ToString());
            }
        }

        /// <summary>
        /// Drop frames in video whose fps is greater than MaxFps.
        /// </summary>
        public int MaxFps
        {
            get
            {
                return GetValue<int>(MAX_FPS_KEY);
            }
            set
            {
                SetValue(MAX_FPS_KEY, value.ToString());
            }
        }
    }
}