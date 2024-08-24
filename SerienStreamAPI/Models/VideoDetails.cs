namespace SerienStreamAPI.Models;

public class VideoDetails(
    int number,
    string title,
    string originalTitle,
    string description,
    VideoStream[] streams)
{
    public int Number { get; set; } = number;

    public string Title { get; set; } = title;

    public string OriginalTitle { get; set; } = originalTitle;

    public string Description { get; set; } = description;

    public VideoStream[] Streams { get; set; } = streams;
}