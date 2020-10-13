namespace UMP
{
    interface IPlayerAudio
    {
        MediaTrackInfo[] AudioTracks { get; }
        MediaTrackInfo AudioTrack { get; set; }
    }
}
