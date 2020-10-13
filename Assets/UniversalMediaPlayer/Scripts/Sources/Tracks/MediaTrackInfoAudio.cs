using System.Text;

namespace UMP
{
    public class MediaTrackInfoAudio : MediaTrackInfoExpanded
    {
        private readonly int _trackChannels;
        private readonly int _trackRate;

        /// <summary>
        /// Create a new audio track info.
        /// </summary>
        /// <param name="trackId">Track ID</param>
        /// <param name="trackCodec">Track Codec (fourcc)</param>
        /// <param name="trackProfile">Track Profile</param>
        /// <param name="trackLevel">Track Level</param>
        /// <param name="trackChannels">Track Channels Number</param>
        /// <param name="trackRate">Track Rate</param>
        internal MediaTrackInfoAudio(int trackId, int trackCodec, int trackProfile, int trackLevel, int trackChannels, int trackRate) : base(trackId, trackCodec, trackProfile, trackLevel)
        {
            _trackChannels = trackChannels;
            _trackRate = trackRate;
        }

        /// <summary>
        /// Get the number of channels.
        /// </summary>
        public int Channels
        {
            get { return _trackChannels; }
        }

        /// <summary>
        /// Get the rate.
        /// </summary>
        public int Rate
        {
            get { return _trackRate; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(200);
            sb.Append(base.ToString()).Append('[');
            sb.Append("CHANNELS=").Append(_trackChannels).Append(", ");
            sb.Append("RATE=").Append(_trackRate).Append(']');
            return sb.ToString();
        }
    }
}
