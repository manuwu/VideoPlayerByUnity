using System;
using System.Runtime.InteropServices;

namespace UMP
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MediaStats
    {
        public int InputReadBytes;
        public float InputBitrate;

        public int DemuxReadBytes;
        public float DemuxBitrate;
        public int DemuxCorrupted;
        public int DemuxDiscontinuity;

        public int DecodedVideo;
        public int DecodedAudio;

        public int VideoDisplayedPictures;
        public int VideoLostPictures;

        public int AudioPlayedAbuffers;
        public int AudioLostAbuffers;

        public int StreamSentPackets;
        public int StreamSentBytes;
        public float StreamSendBitrate;
    }
}

namespace UMP.Wrappers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct EventStruct
    {
        public EventTypes Type;
        public IntPtr PObj;
        public MediaDescriptorUnion MediaDescriptor;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TrackDescription
    {
        public int Id;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        public IntPtr NextDescription;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AudioDescription
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Description;
        public IntPtr NextDescription;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AudioOutputDevice
    {
        public IntPtr NextDevice;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Device;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Description;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AudioTrackInfo
    {
        public int Channels;
        public int Rate;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct VideoTrackInfo
    {
        public int Height;
        public int Width;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct TrackInfo
    {
        [FieldOffset(0)]
        public int Codec;

        [FieldOffset(4)]
        public int Id;

        [FieldOffset(8)]
        public TrackTypes Type;

        [FieldOffset(12)]
        public int Profile;

        [FieldOffset(16)]
        public int Level;

        [FieldOffset(20)]
        public AudioTrackInfo Audio;

        [FieldOffset(20)]
        public VideoTrackInfo Video;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct MediaDescriptorUnion
    {
        [FieldOffset(0)]
        public MetaChanged MetaChanged;

        [FieldOffset(0)]
        public SubitemAdded SubitemAdded;

        [FieldOffset(0)]
        public PlayerBuffering PlayerBuffering;

        [FieldOffset(0)]
        public DurationChanged DurationChanged;

        [FieldOffset(0)]
        public ParsedChanged ParsedChanged;

        [FieldOffset(0)]
        public Freed Freed;

        [FieldOffset(0)]
        public StateChanged StateChanged;

        [FieldOffset(0)]
        public PlayerPositionChanged PlayerPositionChanged;

        [FieldOffset(0)]
        public PlayerTimeChanged PlayerTimeChanged;

        [FieldOffset(0)]
        public PlayerTitleChanged PlayerTitleChanged;

        [FieldOffset(0)]
        public PlayerSeekableChanged PlayerSeekableChanged;

        [FieldOffset(0)]
        public PlayerPausableChanged PlayerPausableChanged;

        [FieldOffset(0)]
        public ListItemAdded ListItemAdded;

        [FieldOffset(0)]
        public ListWillAddItem ListWillAddItem;

        [FieldOffset(0)]
        public ListItemDeleted ListItemDeleted;

        [FieldOffset(0)]
        public ListWillDeleteItem ListWillDeleteItem;

        [FieldOffset(0)]
        public ListPlayerNextItemSet ListPlayerNextItemSet;

        [FieldOffset(0)]
        public PlayerSnapshotTaken PlayerSnapshotTaken;

        [FieldOffset(0)]
        public PlayerLengthChanged PlayerLengthChanged;

        [FieldOffset(0)]
        public VlmMediaEvent VlmMediaEvent;

        [FieldOffset(0)]
        public PlayerMediaChanged PlayerMediaChanged;
    }

    #region Media descriptor
    [StructLayout(LayoutKind.Sequential)]
    internal struct MetaChanged
    {
        public MediaMetadatas MetaType;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SubitemAdded
    {
        public IntPtr NewChild;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DurationChanged
    {
        public long NewDuration;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ParsedChanged
    {
        public int NewStatus;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Freed
    {
        public IntPtr Md;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StateChanged
    {
        public MediaStates NewState;
    }

    /* media instance */
    [StructLayout(LayoutKind.Sequential)]
    internal struct PlayerPositionChanged
    {
        public float NewPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PlayerTimeChanged
    {
        public long NewTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PlayerTitleChanged
    {
        public int NewTitle;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PlayerSeekableChanged
    {
        public int NewSeekable;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PlayerPausableChanged
    {
        public int NewPausable;
    }
#endregion

#region List
    [StructLayout(LayoutKind.Sequential)]
    internal struct ListItemAdded
    {
        public IntPtr Item;
        public int Index;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ListWillAddItem
    {
        public IntPtr Item;
        public int Index;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ListItemDeleted
    {
        public IntPtr Item;
        public int Index;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ListWillDeleteItem
    {
        public IntPtr Item;
        public int Index;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ListPlayerNextItemSet
    {
        public IntPtr Item;
    }
#endregion

    /// <summary>
    /// Snapshot
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PlayerSnapshotTaken
    {
        public IntPtr Filename;
    }

    /// <summary>
    /// Length
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PlayerLengthChanged
    {
        public long NewLength;
    }

    /// <summary>
    /// VLM media
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct VlmMediaEvent
    {
        public IntPtr MediaName;
        public IntPtr InstanceName;
    }

    /// <summary>
    /// Buffering value
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PlayerBuffering
    {
        public float NewCache;
    }

    /// <summary>
    /// Extra MediaPlayer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PlayerMediaChanged
    {
        public IntPtr NewMedia;
    }
}
