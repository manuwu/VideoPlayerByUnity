using System.Text;

namespace UMP
{
    public abstract class MediaTrackInfoExpanded : MediaTrackInfo
    {
        private readonly int _trackCodec;
        private readonly int _trackProfile;
        private readonly int _trackLevel;

        /// <summary>
        /// Create a new expanded track info.
        /// </summary>
        /// <param name="trackId">Track id</param>
        /// <param name="trackCodec">Track Codec (fourcc)</param>
        /// <param name="trackProfile">Track Profile</param>
        /// <param name="trackLevel">Track Level</param>
        internal MediaTrackInfoExpanded(int trackId, int trackCodec, int trackProfile, int trackLevel) : base(trackId, trackCodec != 0 ? Encoding.ASCII.GetString(new byte[] { (byte)trackCodec, (byte)(trackCodec >> 8), (byte)(trackCodec >> 16), (byte)(trackCodec >> 24) }).Trim() : null)
        {
            _trackCodec = trackCodec;
            _trackProfile = trackProfile;
            _trackLevel = trackLevel;
        }

        /// <summary>
        /// Get the codec (fourcc).
        /// </summary>
        public int Codec
        {
            get { return _trackCodec; }
        }

        /// <summary>
        /// Get the profile.
        /// </summary>
        public int Profile
        {
            get { return _trackProfile; }
        }

        /// <summary>
        /// Get the level.
        /// </summary>
        public int Level
        {
            get { return _trackLevel; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("TrackInfoExpanded").Append('[');
            sb.Append("ID=").Append(Id).Append(", ");
            sb.Append("NAME=").Append(Name).Append(", ");
            sb.Append("CODEC=").Append(Codec).Append(", ");
            sb.Append("PROFILE=").Append(Profile).Append(", ");
            sb.Append("LEVEL=").Append(Level).Append(']');
            return sb.ToString();
        }
    }
}
