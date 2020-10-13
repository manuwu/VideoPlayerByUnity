using System.Text;

namespace UMP
{
    public class MediaTrackInfoVideo : MediaTrackInfoExpanded
    {
        private readonly int _trackWidth;
        private readonly int _trackHeight;

        /// <summary>
        /// Create new unknown track info.
        /// </summary>
        /// <param name="trackId">Track ID</param>
        /// <param name="trackCodec">Track Codec (fourcc)</param>
        /// <param name="trackProfile">Track Profile</param>
        /// <param name="trackLevel">Track Level</param>
        /// <param name="trackWidth">Track Video Width</param>
        /// <param name="trackHeight">Track Video Height</param>
        internal MediaTrackInfoVideo(int trackId, int trackCodec, int trackProfile, int trackLevel, int trackWidth, int trackHeight) : base(trackId, trackCodec, trackProfile, trackLevel)
        {
            _trackWidth = trackWidth;
            _trackHeight = trackHeight;
        }

        /// <summary>
        /// Get the video width.
        /// </summary>
        public int Width
        {
            get { return _trackWidth; }
        }

        /// <summary>
        /// Get the video height.
        /// </summary>
        public int Height
        {
            get { return _trackHeight; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(200);
            sb.Append(base.ToString()).Append('[');
            sb.Append("WIDTH=").Append(_trackWidth).Append(", ");
            sb.Append("HEIGHT=").Append(_trackHeight).Append(']');
            return sb.ToString();
        }
    }
}
