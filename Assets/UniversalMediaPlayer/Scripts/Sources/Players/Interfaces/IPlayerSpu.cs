using System;

namespace UMP
{
    interface IPlayerSpu
    {
        MediaTrackInfo[] SpuTracks { get; }
        MediaTrackInfo SpuTrack { get; set; }
        bool SetSubtitleFile(Uri path);
    }
}
