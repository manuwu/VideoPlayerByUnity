using System.Text;

namespace UMP
{
    public class MediaTrackInfo
    {
        private readonly int _trackId;
        private readonly string _trackName;

        /// <summary>
        /// Create a new track info.
        /// </summary>
        /// <param name="trackId">Track ID</param>
        /// <param name="trackName">Track Name</param>
        internal MediaTrackInfo(int trackId, string trackName)
        {
            _trackId = trackId;
            _trackName = trackName;
        }

        /// <summary>
        /// Get the track id.
        /// </summary>
        public int Id
        {
            get { return _trackId; }
        }

        /// <summary>
        /// Get the track name.
        /// </summary>
        public string Name
        {
            get { return _trackName; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("TrackInfo").Append('[');
            sb.Append("ID=").Append(_trackId).Append(',');
            sb.Append("NAME=").Append(_trackName).Append(']');
            return sb.ToString();
        }
    }
}
