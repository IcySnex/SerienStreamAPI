# SerienStreamAPI
This library allows you to search for any series on [SerienStream.to (s.to)](https://serien.domains/) and [AniWorld.to](https://anicloud.domains/). You can get all meta info about all episodes and movies of each series.

This library also allows you to extract the video stream url of common host-providers (like VOE, StreamTape, Doodstream, Vidoza) and directly download them via FFmpeg.
This API works by scraping the HTML data of SerienStream/AniWorld and then parsing it into easy-to-use models.

---

## Setup
Setting up is pretty easy. Just create a new instance of `SerienStreamClient` with the given hostUrl (e.g. https://s.to/ or https://aniworld.to/) and "site" (for SerienStream it's "serie", for AniWorld its "anime"). If the host url is marked as "unsafe" you can set "ignoreCerficiateValidation" to true which will bypass SSL certificate verification. If you aim to download videos as well, you will have to create an instance of `DownloadClient`.
```cs
// Create a new SerienStreamClient
SerienStreamClient client = new(hostUrl: "https://aniworld.to/", site: "anime", ignoreCerficiateValidation: false, logger: null);

// Create a new DownloadClient
DownloadClient downloadClient = new(ffmpegLocation: "ffmpeg.exe", ignoreCerficiateValidation: false, logger: null);
```

## Searching
```cs
// Search for a series via the title
Series series = await client.GetSeriesAsync("My Dress-Up Darling");
Console.WriteLine("Title: {title}, Description: {description}", series.Title, series.Description);


// Get all episodes of a season
Media[] episodesOfSeason1 = await client.GetEpisodesAsync("My Dress-Up Darling", 1);
foreach(Media episode in episodesOfSeason1)
   Console.WriteLine("[{number}] Title: {title}", episode.Number, episode.Title);

Media[] movies = await client.GetMoviesAsync("My Dress-Up Darling");
foreach(Media movie in movies)
   Console.WriteLine("[{number}] Title: {title}", movie.Number, movie.Title);

// Get video details
VideoDetails episodeVideoDetails = await client.GetEpisodeVideoInfoAsync("My Dress-Up Darling", 1, 1);
foreach (VideoStream videoStream in episodeVideoDetails.Streams)
   Console.WriteLine("Video Stream [{videoUrl]: {hoster}-{audioLanguage}-{subtitleLanguage}", videoStream.VideoUrl, videoStream.Hoster, videoStream.Language.Audio, videoStream.Language.Subtitle);

VideoDetails movieVideoDetails = await client.GetEpisodeVideoInfoAsync("My Dress-Up Darling", 1, 1);
foreach (VideoStream videoStream in movieVideoDetails.Streams)
   Console.WriteLine("Video Stream [{videoUrl]: {hoster}-{audioLanguage}-{subtitleLanguage}", videoStream.VideoUrl, videoStream.Hoster, videoStream.Language.Audio, videoStream.Language.Subtitle);
```

## Downloading
In order to download any video from SerienStream/Aniworld you first have to get the video details of the episode/movie. Then you can select one of the available streams and extract the stream url. Then you can use FFmpeg to download the stream to a filepath.
```cs
// Get the video details of the first episode of the first season of the series "My Dress-Up Darling"
VideoDetails videoDetails = await client.GetEpisodeVideoInfoAsync(series.Title, 1, 1);
// Select the first German-audio video stream
VideoStream videoStream = videoDetails.Streams.FirstOrDefault(stream => stream.Language.Audio == Language.German) ?? throw new Exception("No German video stream found!");

// Extract hoster stream url from video stream
string streamUrl = videoStream.Hoster switch
{
    Hoster.VOE => await downloadClient.GetVoeStreamUrlAsync(bestStream.VideoUrl),
    Hoster.Streamtape => await downloadClient.GetStreamtapeStreamUrlAsync(bestStream.VideoUrl),
    Hoster.Doodstream => await downloadClient.GetDoodstreamStreamUrlAsync(bestStream.VideoUrl),
    Hoster.Vidoza => await downloadClient.GetVidozaStreamUrlAsync(bestStream.VideoUrl),
    _ => throw new Exception("Hoster is not supported!")
};

// Download video stream
await downloadClient.DownloadAsync(streamUrl, "video.mp4", null, new Progress<EncodingProgress>(progress =>
    logger.LogInformation("Downloading episode... Time: {timeElapsed}, Speed: {speed}x", progress.TimeElapsed, progress.SpeedMultiplier)));
```

---

## Questions/Issues
If you have any questions about this library or questions on how to use it. Please [open a new issue](https://github.com/IcySnex/SerienStreamAPI/issues) and I will quickly respond. For more usage examples you can look at the [Tests](https://github.com/IcySnex/SerienStreamAPI/tree/main/SerienStreamAPI.Tests)

---

## Legal Notice
This library is for educational purposes only. Downloading or accessing copyrighted content without permission may be illegal in your country. I am not responsible for any misuse or legal consequences. **Use at your own risk**.
