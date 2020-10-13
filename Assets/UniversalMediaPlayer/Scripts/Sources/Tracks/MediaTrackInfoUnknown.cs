namespace UMP
{
    public class MediaTrackInfoUnknown : MediaTrackInfoExpanded
    {
        /// <summary>
        /// Create new unknown track info.
        /// </summary>
        /// <param name="trackId">Track id</param>
        /// <param name="trackCodec">Track Codec (fourcc)</param>
        /// <param name="trackProfile">Track Profile</param>
        /// <param name="trackLevel">Track Level</param>
        internal MediaTrackInfoUnknown(int trackId, int trackCodec, int trackProfile, int trackLevel) : base(trackId, trackCodec, trackProfile, trackLevel)
        {
        }
    }
}
