using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace BooruDatasetGatherer.Data
{
    public class BooruProfile
    {
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;


        [JsonPropertyName("filter")]
        public string[] Filter { get; set; } = Array.Empty<string>();

        [JsonPropertyName("files")]
        public string[] FileFilters { get; set; } = { ".png", ".jpg", ".jpeg", ".gif", ".webp" };


        [JsonPropertyName("location")]
        public string SaveLocation { get; set; } = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Images");


        [JsonPropertyName("batch")]
        public int BatchSize { get; set; } = 20;

        [JsonPropertyName("size")]
        public int TotalSize { get; set; } = 1000;


        [JsonPropertyName("threads")]
        public byte Threads { get; set; } = 4;


        [JsonPropertyName("nsfw")]
        public bool MatureContentAllowed { get; set; } = false;

        [JsonPropertyName("download")]
        public bool DownloadImages { get; set; } = false;


        [JsonPropertyName("exceptionLimit")]
        public byte ExceptionLimit { get; set; } = 5;


        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("passwordHash")]
        public string Password { get; set; } = string.Empty;

        public bool HasAuth { get { return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password); } }

        public BooruProfile() { }

        public void ParseSettings(Dictionary<string, string> settings)
        {
            foreach (string key in settings.Keys)
            {
                switch (key)
                {
                    case "source":
                        Source = settings[key];
                        break;
                    case "batch":
                    case "batchsize":
                        if (int.TryParse(settings[key], out int batchSize))
                            BatchSize = batchSize;
                        break;
                    case "filter":
                        Filter = settings[key].Split(",").Select(x => x.Replace(" ", "")).ToArray();
                        break;
                    case "files":
                    case "filefilters":
                        FileFilters = settings[key].Split(",").Select(x => x.ToLower().Replace(" ", "")).ToArray();
                        break;
                    case "location":
                    case "savelocation":
                        if (Directory.Exists(settings[key]))
                            SaveLocation = settings[key];
                        break;
                    case "threads":
                        if (byte.TryParse(settings[key], out byte threads) && threads > 0 && threads < 64)
                            Threads = threads;
                        break;
                    case "nsfw":
                    case "maturecontent":
                        if (bool.TryParse(settings[key], out bool nsfw))
                            MatureContentAllowed = nsfw;
                        break;
                    case "size":
                        if (int.TryParse(settings[key], out int size))
                            TotalSize = size;
                        break;
                    case "download":
                    case "downloadimages":
                        if (bool.TryParse(settings[key], out bool download))
                            DownloadImages = download;
                        break;
                    case "username":
                        Username = settings[key];
                        break;
                    case "passwordhash":
                    case "password":
                        Password = settings[key];
                        break;
                    case "exceptionlimit":
                        if (byte.TryParse(settings[key], out byte limit))
                            ExceptionLimit = limit;
                        break;
                }
            }
        }
    }
}
