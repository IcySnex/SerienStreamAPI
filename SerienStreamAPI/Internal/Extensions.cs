using HtmlAgilityPack;
using SerienStreamAPI.Enums;
using SerienStreamAPI.Models;
using System.Xml;

namespace SerienStreamAPI.Internal;

internal static class Extensions
{
    public static string AddRelativePath(
        this string baseUrl,
        string relativePath) =>
        $"{baseUrl.Trim('/')}/{relativePath.Trim('/')}";

    public static string ToRelativePath(
        this string text) =>
        text.ToLower().Replace(' ', '-');


    public static int ToInt32(
        this string text) =>
        int.Parse(text);

    public static int ToInt32(
        this bool boolean) =>
        boolean ? 1 : 0;

    public static Hoster ToHoster(
        this string text) =>
        text.ToLowerInvariant() switch
        {
            "voe" => Hoster.VOE,
            "doodstream" => Hoster.Doodstream,
            "vidoza" => Hoster.Vidoza,
            "streamtape" => Hoster.Streamtape,
            _ => Hoster.Unknown
        };
    
    public static Language ToLanguage(
        this string text) =>
        text.ToLowerInvariant() switch
        {
            "german" => Language.German,
            "english" => Language.English,
            "japanese" => Language.Japanese,
            _ => Language.Unknown
        };

    public static MediaLanguage ToMediaLanguage(
        this string text)
    {
        if (text.Length < 15)
            return new(Language.Unknown, null);

        string[] languageData = text[11..^4].Split('-', StringSplitOptions.RemoveEmptyEntries);
        return languageData.Length switch
        {
            1 => new(languageData[0].ToLanguage(), null),
            2 => new(languageData[0].ToLanguage(), languageData[1].ToLanguage()),
            _ => new(Language.Unknown, null)
        };
    }


    public static string GetInnerText(
        this HtmlNode? node) =>
        node?.InnerText.Trim('/') ?? string.Empty;

    public static string GetAttributeValue(
        this HtmlNode? node,
        string attributeName) =>
        node?.GetAttributeValue(attributeName, null).Trim('/') ?? string.Empty;


    public static string? SelectSingleNodeTextOrDefault(
        this HtmlNode node,
        string xpath)
    {
        HtmlNode? result = node.SelectSingleNode(xpath);
        return result.GetInnerText();
    }
    
    public static string? SelectSingleNodeAttributeOrDefault(
        this HtmlNode node,
        string xpath,
        string attributeName)
    {
        HtmlNode? result = node.SelectSingleNode(xpath);
        return result.GetAttributeValue(attributeName);
    }


    public static string SelectSingleNodeText(
        this HtmlNode node,
        string xpath) =>
        node.SelectSingleNodeTextOrDefault(xpath) ?? throw new NodeNotFoundException($"Could not find node: \"{xpath}\".");

    public static string SelectSingleNodeAttribute(
        this HtmlNode node,
        string xpath,
        string attributeName) =>
        node.SelectSingleNodeAttributeOrDefault(xpath, attributeName) ?? throw new NodeAttributeNotFoundException($"Could not find node or attribute: \"{xpath}\" - \"{attributeName}\".");


    public static bool Any(
        this HtmlNode node,
        string xpath) =>
        node.SelectSingleNode(xpath) is not null;

    public static T[] Select<T>(
        this HtmlNode node,
        string xpath,
        Func<HtmlNode, T> selector)
    {
        HtmlNodeCollection? nodes = node.SelectNodes(xpath);
        if (nodes is null)
            return [];

        T[] result = new T[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
            result[i] = selector(nodes[i]);

        return result;
    }
    
    public static Dictionary<TKey, TValue> Map<TKey, TValue>(
        this HtmlNode node,
        string xpath,
        Func<HtmlNode, (TKey, TValue)> selector) where TKey : notnull
    {
        HtmlNodeCollection? nodes = node.SelectNodes(xpath);
        if (nodes is null)
            return [];

        Dictionary<TKey, TValue> result = [];
        foreach (HtmlNode childNode in nodes)
        {
            var (key, value) = selector(childNode);
            result[key] = value;
        }

        return result;
    }
}