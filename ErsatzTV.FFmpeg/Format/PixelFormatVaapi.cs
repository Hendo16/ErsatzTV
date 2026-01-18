namespace ErsatzTV.FFmpeg.Format;

public class PixelFormatVaapi(string name, int bitDepth = 8) : IPixelFormat
{
    public string Name { get; } = name;

    public string FFmpegName => "vaapi";

    public int BitDepth { get; } = bitDepth;
}
