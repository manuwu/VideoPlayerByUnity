using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UMP.Wrappers;

/// <summary>
/// VLC sound formats
/// </summary>
internal enum SoundType
{
    /// <summary>
    /// 16 bits per sample
    /// </summary>
    S16N
}

internal class PlayerBufferSound
{
    public const int DEFAULT_CLIP_SAMPLES_SIZE = 65536;
    public const int BITS_PER_SAMPLE = 16;

    private readonly float[] _samplesData;
    private GCHandle _gcHandle = default(GCHandle);

    public static SoundType GetSoundType(string format)
    {
        switch (format)
        {
            case "S16N":
                return SoundType.S16N;

            default:
                return SoundType.S16N;
        }
    }

    /// <summary>
    /// Initializes new instance of SoundFormat class
    /// </summary>
    /// <param name="rate"></param>
    /// <param name="channels"></param>
    public PlayerBufferSound(int rate, int channels)
    {
        SoundType = SoundType.S16N;
        Rate = rate;
        Channels = channels;
        _samplesData = new float[2048];
    }

    public void UpdateSamplesData(IntPtr samples, uint count)
    {
        /*_nativeHelper.NativeHelperGetAudioSamples()
        lock (_samplesData)
        {
            int audioFrameLength = BlockSize * (int)count;
            IntPtr point = IntPtr.Zero;//_nativeHelper.NativeHelperGetAudioSamples(samples, audioFrameLength);
            float[] buffer = new float[audioFrameLength / 2];
            Marshal.Copy(point, buffer, 0, buffer.Length);

            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Win)
                Marshal.FreeCoTaskMem(point);

            _samplesData.AddRange(buffer);
            buffer = null;
        }*/
    }

    /// <summary>
    /// Sampling rate in Hz
    /// </summary>
    public int Rate { get; private set; }

    /// <summary>
    /// Number of channels used by audio sample
    /// </summary>
    public int Channels { get; private set; }

    /// <summary>
    /// Size of single audio sample in bytes
    /// </summary>
    public int BitsPerSample { get; private set; }

    /// <summary>
    /// Specifies sound sample format
    /// </summary>
    public SoundType SoundType { get; private set; }

    /// <summary>
    /// Size of audio block (BitsPerSample / 8 * Channels)
    /// </summary>
    public int BlockSize { get; private set; }

    /// <summary>
    /// Gets the decoded audio samples.
    /// </summary>
    public float[] SamplesData
    {
        get { return _samplesData; }
    }

    internal IntPtr SamplesDataAddr
    {
        get
        {
            if (!_gcHandle.IsAllocated)
                _gcHandle = GCHandle.Alloc(_samplesData, GCHandleType.Pinned);

            return _gcHandle.AddrOfPinnedObject();
        }
    }

    /// <summary>
    /// Playback time stamp in microseconds
    /// </summary>
    public long Pts { get; set; }
}
