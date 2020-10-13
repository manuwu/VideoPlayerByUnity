namespace UMP
{
    public class MediaTrackInfoSpu : MediaTrackInfoExpanded
    {
        /// <summary>
        /// Create new sub-picture/sub-title track info.
        /// </summary>
        /// <param name="trackId">Track ID</param>
        /// <param name="trackCodec">Track Codec (fourcc)</param>
        /// <param name="trackProfile">Track Profile</param>
        /// <param name="trackLevel">Track Level</param>
        internal MediaTrackInfoSpu(int trackId, int trackCodec, int trackProfile, int trackLevel) : base(trackId, trackCodec, trackProfile, trackLevel)
        {
        }
    }
}
