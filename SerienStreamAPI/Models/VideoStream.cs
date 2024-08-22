using SerienStreamAPI.Enums;

namespace SerienStreamAPI.Models;

public class VideoStream(
    string redirectId,
    Hoster hoster,
    MediaLanguage language)
{
    public string RedirectId { get; set; } = redirectId;

    public Hoster Hosters { get; set; } = hoster;

    public MediaLanguage Languages { get; set; } = language;
}