namespace UMP
{
    public interface IMediaListener:
        IPlayerOpeningListener,
        IPlayerBufferingListener,
        IPlayerImageReadyListener,
        IPlayerPreparedListener,
        IPlayerPlayingListener,
        IPlayerPausedListener,
        IPlayerStoppedListener,
        IPlayerEndReachedListener,
        IPlayerEncounteredErrorListener
    {}

    public interface IPathPreparedListener
    {
        void OnPathPrepared(string path, bool isPreparing);
    }

    public interface IPlayerOpeningListener
    {
        void OnPlayerOpening();
    }

    public interface IPlayerBufferingListener
    {
        void OnPlayerBuffering(float percentage);
    }

    public interface IPlayerImageReadyListener
    {
        void OnPlayerImageReady(UnityEngine.Texture2D videoTexture);
    }

    public interface IPlayerPreparedListener
    {
        void OnPlayerPrepared(int videoWidth, int videoHeight);
    }

    public interface IPlayerPlayingListener
    {
        void OnPlayerPlaying();
    }

    public interface IPlayerPausedListener
    {
        void OnPlayerPaused();
    }

    public interface IPlayerStoppedListener
    {
        void OnPlayerStopped();
    }

    public interface IPlayerEndReachedListener
    {
        void OnPlayerEndReached();
    }

    public interface IPlayerEncounteredErrorListener
    {
        void OnPlayerEncounteredError();
    }

    public interface IPlayerTimeChangedListener
    {
        void OnPlayerTimeChanged(long time);
    }

    public interface IPlayerPositionChangedListener
    {
        void OnPlayerPositionChanged(float position);
    }

    public interface IPlayerSnapshotTakenListener
    {
        void OnPlayerSnapshotTaken(string path);
    }

    public interface ILogMessageListener
    {
        void OnLogMassage(PlayerManagerLogs.PlayerLog message);
    }
}
