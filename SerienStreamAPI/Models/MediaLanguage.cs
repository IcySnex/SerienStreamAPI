using SerienStreamAPI.Enums;

namespace SerienStreamAPI.Models;

public class MediaLanguage(
    Language audio,
    Language? subtitle)
{
    public Language Audio { get; } = audio;

    public Language? Subtitle { get; } = subtitle;
}