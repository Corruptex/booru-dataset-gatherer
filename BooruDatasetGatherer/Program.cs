using BooruDatasetGatherer.Data;
using BooruDatasetGatherer.Factories;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using System.Text.Json;

namespace BooruDatasetGatherer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            if (args == null || args.Length == 0)
                return;

            Dictionary<string, string> argMap = new();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i][0] == '-' && args[i][1] == '-')
                    argMap.Add(args[i][2..], args[i + 1]);
            }

            BooruProfile profile = new();

            if (!File.Exists(argMap["profile"]) || File.Exists(Path.Join(AppDomain.CurrentDomain.BaseDirectory, $"{argMap["profile"]}.json")))
                argMap["profile"] = Path.Join(AppDomain.CurrentDomain.BaseDirectory, $"{argMap["profile"]}.json");

            if (argMap.ContainsKey("profile") && File.Exists(argMap["profile"]))
            {
                using FileStream stream = File.OpenRead(argMap["profile"]);
                BooruProfile? booruProfile = await JsonSerializer.DeserializeAsync<BooruProfile>(stream);

                if (booruProfile != null)
                    profile = booruProfile;
            }

            profile.ParseSettings(argMap);

            if (string.IsNullOrEmpty(profile.Source))
                return;

            BooruFactory factory = new();
            if (!factory.Contains(profile.Source))
                return;

            if (!Directory.Exists(profile.SaveLocation))
                Directory.CreateDirectory(profile.SaveLocation);

            Console.WriteLine("Running with settings: \n");
            Console.WriteLine(JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));

            ABooru booru = factory.GetBooru(profile.Source)!;
            if (!booru.HasMultipleRandomAPI)
            {
                Console.WriteLine("Selected source does not support getting random posts. Exiting.");
                return;
            }

            int perThread = profile.TotalSize / profile.Threads;
            Task[] threads = new Task[profile.Threads];

            Console.WriteLine($"\nSpreading {profile.TotalSize} posts over {profile.Threads} threads.");
            Console.WriteLine($"Each thread will handle {perThread} posts, divided under (circa) {perThread / profile.BatchSize} cycles.\n");

            string fileLocation = Path.Join(profile.SaveLocation, $"results-{profile.Source}-{DateTime.Now.ToShortDateString()}.csv");
            using (StreamWriter stream = new(File.Create(fileLocation)))
            {
                await stream.WriteLineAsync("FILEURL, PREVIEWURL, POSTURL, SAMPLEURI, RATING, TAGS, ID, HEIGHT, WIDTH, PREVIEWHEIGHT, PREVIEWWIDTH, CREATION, SOURCE, SCORE, MD5");

                for (int i = 0; i < threads.Length; i++)
                    threads[i] = GetPostsAsync(factory.GetBooru(profile.Source)!, profile, stream, perThread, profile.BatchSize);

                await Task.WhenAll(threads);
            }

            Console.WriteLine("Finished");
        }

        private static async Task GetPostsAsync(ABooru booruInstance, BooruProfile profile, StreamWriter stream, int total, int batch)
        {
            int processed = 0;

            while (processed < total)
            {
                SearchResult[] results;

                if (total - processed < batch)
                    results = await booruInstance.GetRandomPostsAsync(total - processed, profile.Filter);
                else
                    results = await booruInstance.GetRandomPostsAsync(batch, profile.Filter);

                for (int i = 0; i < results.Length; i++)
                {
                    SearchResult result = results[i];
                    string extension = Path.GetExtension(result.FileUrl.GetLeftPart(UriPartial.Path));

                    if (profile.FileFilters.Contains(extension.ToLower()))
                    {
                        string line = $"\"{result.FileUrl}\", \"{result.PreviewUrl}\", \"{result.PostUrl}\", \"{result.SampleUri}\", \"{result.Rating}\", " +
                                    $"\"{string.Join(',', result.Tags)}\", \"{result.ID}\", \"{result.Height}\", \"{result.Width}\", \"{result.PreviewHeight}\", \"{result.PreviewWidth}\", " +
                                    $"\"{result.Creation}\", \"{result.Source}\", \"{result.Score}\", \"{result.MD5}\"";
                        lock (stream)
                        {
                            stream.WriteLine(line);
                        }

                        if (profile.DownloadImages)
                        {
                            using HttpClient httpClient = new();

                            string path = Path.Combine(profile.SaveLocation, $"{result.ID}{extension}");
                            byte[] image = await httpClient.GetByteArrayAsync(result.FileUrl);

                            await File.WriteAllBytesAsync(path, image);
                        }
                    }
                }

                processed += results.Length;
            }
        }
    }
}