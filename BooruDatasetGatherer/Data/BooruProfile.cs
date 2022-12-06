using System;
using System.IO;
using System.Text.Json.Serialization;

namespace BooruDatasetGatherer.Data
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
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
    }
}
