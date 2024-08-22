using SerienStreamAPI.Enums;

namespace SerienStreamAPI.Models;

public class Media(
    int number,
    string title,
    string originalTitle,
    Hoster[] hosters,
    MediaLanguage[] languages)
{
    public int Number { get; set; } = number;

    public string Title { get; set; } = title;

    public string OriginalTitle { get; set; } = originalTitle;

    public Hoster[] Hosters { get; set; } = hosters;

    public MediaLanguage[] Languages { get; set; } = languages;
}