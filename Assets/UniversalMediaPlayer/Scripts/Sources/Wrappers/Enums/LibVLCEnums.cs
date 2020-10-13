namespace UMP.Wrappers
{
    internal enum LogLevels
    {
        Debug = 0,
        Notice = 2,
        Warning = 3,
        Error = 4
    }

    internal enum ParsedStatus
    {
        Skipped = 0,
        Failed,
        Timeout,
        Done
    }

    internal enum MediaStates
    {
        NothingSpecial = 0,
        Opening,
        Buffering,
        Playing,
        Paused,
        Stopped,
        Ended,
        Error
    }

    internal enum MediaSlaveType
    {
        Subtitle,
        Audio
    }

    internal enum MediaMetadatas
    {
        Title = 0,
        Artist,
        Genre,
        Copyright,
        Album,
        TrackNumber,
        Description,
        Rating,
        Date,
        Setting,
        URL,
        Language,
        NowPlaying,
        Publisher,
        EncodedBy,
        ArtworkURL,
        TrackID
    }

    internal enum TrackTypes
      : int
    {
        Unknown = -1,
        Audio = 0,
        Video = 1,
        Text = 2
    }

    internal enum EventTypes
          : int
    {
        MediaMetaChanged = 0,
        MediaSubItemAdded,
        MediaDurationChanged,
        MediaParsedChanged,
        MediaFreed,
        MediaStateChanged,
        MediaSubItemTreeAdded,

        MediaPlayerMediaChanged = 0x100,
        MediaPlayerNothingSpecial,
        MediaPlayerOpening,
        MediaPlayerBuffering,
        MediaPlayerPlaying,
        MediaPlayerPaused,
        MediaPlayerStopped,
        MediaPlayerForward,
        MediaPlayerBackward,
        MediaPlayerEndReached,
        MediaPlayerEncounteredError,
        MediaPlayerTimeChanged,
        MediaPlayerPositionChanged,
        MediaPlayerSeekableChanged,
        MediaPlayerPausableChanged,
        MediaPlayerTitleChanged,
        MediaPlayerSnapshotTaken,
        MediaPlayerLengthChanged,
        MediaPlayerVout,
        MediaPlayerScrambledChanged,

        MediaListItemAdded = 0x200,
        MediaListWillAddItem,
        MediaListItemDeleted,
        MediaListWillDeleteItem,

        MediaListViewItemAdded = 0x300,
        MediaListViewWillAddItem,
        MediaListViewItemDeleted,
        MediaListViewWillDeleteItem,

        MediaListPlayerPlayed = 0x400,
        MediaListPlayerNextItemSet,
        MediaListPlayerStopped,

        MediaDiscovererStarted = 0x500,
        MediaDiscovererEnded,

        VlmMediaAdded = 0x600,
        VlmMediaRemoved,
        VlmMediaChanged,
        VlmMediaInstanceStarted,
        VlmMediaInstanceStopped,
        VlmMediaInstanceStatusInit,
        VlmMediaInstanceStatusOpening,
        VlmMediaInstanceStatusPlaying,
        VlmMediaInstanceStatusPause,
        VlmMediaInstanceStatusEnd,
        VlmMediaInstanceStatusError
    }
}
