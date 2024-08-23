namespace SerienStreamAPI.Models;

public class EncodingProgress(
    int framesProcessed,
    double fps,
    double quality,
    int outputFileSizeKb,
    TimeSpan timeElapsed,
    double bitrateKbps,
    double speedMultiplier)
{
    public int FramesProcessed { get; set; } = framesProcessed;

    public double Fps { get; set; } = fps;

    public double Quality { get; set; } = quality;

    public int OutputFileSizeKb { get; set; } = outputFileSizeKb;

    public TimeSpan TimeElapsed { get; set; } = timeElapsed;

    public double BitrateKbps { get; set; } = bitrateKbps;

    public double SpeedMultiplier { get; set; } = speedMultiplier;
}