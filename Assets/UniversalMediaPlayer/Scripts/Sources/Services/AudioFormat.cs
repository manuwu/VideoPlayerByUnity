namespace UMP.Services
{
    /// <summary>
    /// Audio format.
    /// </summary>
    public enum AudioFormat
    {
        /// <summary>
        /// MPEG-2 Audio Layer III.
        /// </summary>
        Mp3,

        /// <summary>
        /// MPEG-4 Part 3, Advanced Audio Coding (AAC).
        /// </summary>
        Aac,

        /// <summary>
        /// Vorbis.
        /// </summary>
        Vorbis,

        /// <summary>
        /// Opus.
        /// </summary>
        Opus,

        /// <summary>
        /// The audio type is unknown.
        /// </summary>
        Unknown
    }
}
