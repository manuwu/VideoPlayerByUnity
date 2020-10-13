using System;
using System.Runtime.InteropServices;

internal class PlayerBufferVideo
{
    private const string CHROMA = "RV32";
    private const int PIXEL_SIZE_RGBA = 32;

    private readonly int _width = 0;
    private readonly int _height = 0;
    private readonly int _pitch;
    private readonly int _lines;
    private readonly byte[] _framePixels;
    private GCHandle _gcHandle = default(GCHandle);

    /// <summary>
    ///   Create a new instance of the VideoBuffer
    /// </summary>
    /// <param name="width">
    ///   The width of the video.
    /// </param>
    /// <param name="height">
    ///   The height of the video.
    /// </param>
    public PlayerBufferVideo(int width, int height)
    {
        _width = width;
        _height = height;
        _lines = _height;
        _pitch = CalculatePitch(_width);
        _framePixels = new byte[_pitch * _lines];
    }

    /// <summary>
    /// Gets the width of the video in pixels.
    /// </summary>
    public int Width
    {
        get { return _width; }
    }

    /// <summary>
    /// Gets the height of the video in pixels.
    /// </summary>
    public int Height
    {
        get { return _height; }
    }

    /// <summary>
    /// Gets the stride of a video frame which is the width 
    /// multiplied by the bytes per pixel.
    /// </summary>
    public int Pitch
    {
        get { return _pitch; }
    }

    /// <summary>
    /// Gets the number of scan lines.
    /// </summary>
    public int Lines
    {
        get { return _lines; }
    }

    /// <summary>
    /// Gets or sets the video frame pixels.
    /// </summary>
    public byte[] FramePixels
    {
        get { return _framePixels; }
    }

    /// <summary>
    /// Four-characters string identifying the chroma.
    /// </summary>
    public static string Chroma
    {
        get { return CHROMA; }
    }

    /// <summary>
    /// Four-characters string identifying the chroma.
    /// </summary>
    public static int CalculatePitch(int width)
    {
        return width * PIXEL_SIZE_RGBA / 8;
    }

    internal IntPtr FramePixelsAddr
    {
        get
        {
            if (!_gcHandle.IsAllocated)
                _gcHandle = GCHandle.Alloc(_framePixels, GCHandleType.Pinned);

            return _gcHandle.AddrOfPinnedObject();
        }
    }

    internal void ClearFramePixels()
    {
        if (_gcHandle.IsAllocated)
            _gcHandle.Free();
    }
}
