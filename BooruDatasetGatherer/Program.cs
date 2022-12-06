using BooruDatasetGatherer.Data;
using BooruDatasetGatherer.Factories;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BooruDatasetGatherer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                MainAsync(args).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();
        }

        private static async Task MainAsync(string[] args)
        {
            if (args == null || args.Length == 0)
                return;

            Dictionary<string, string> argMap = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i][0] == '-' && args[i][1] == '-')
                    argMap.Add(args[i][2..], args[i + 1]);
            }

            BooruProfile profile = new BooruProfile();

            if (argMap.ContainsKey("profile"))
            {
                if (!File.Exists(argMap["profile"]) || File.Exists(Path.Join(AppDomain.CurrentDomain.BaseDirectory, $"{argMap["profile"]}.json")))
                    argMap["profile"] = Path.Join(AppDomain.CurrentDomain.BaseDirectory, $"{argMap["profile"]}.json");

                if (File.Exists(argMap["profile"]))
                {
                    using FileStream stream = File.OpenRead(argMap["profile"]);
                    BooruProfile? booruProfile = await JsonSerializer.DeserializeAsync<BooruProfile>(stream);

                    if (booruProfile != null)
                        profile = booruProfile;
                }
            }
            else
            {
                string argJson = JsonSerializer.Serialize(argMap);
                profile = JsonSerializer.Deserialize<BooruProfile>(argJson)!;
            }

            if (string.IsNullOrEmpty(profile.Source))
                return;

            BooruFactory factory = new BooruFactory();
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

            Stopwatch stopWatch = Stopwatch.StartNew();

            string fileLocation = Path.Join(profile.SaveLocation, $"results-{profile.Source}-{DateTime.Now.ToShortDateString()}-{DateTime.Now.ToShortTimeString().Replace(":", "-")}.csv");
            using (StreamWriter stream = new StreamWriter(File.Create(fileLocation)))
            {
                await stream.WriteLineAsync("FILEURL, PREVIEWURL, POSTURL, SAMPLEURI, RATING, TAGS, ID, HEIGHT, WIDTH, PREVIEWHEIGHT, PREVIEWWIDTH, CREATION, SOURCE, SCORE, MD5, LOCATION");

                for (int i = 0; i < threads.Length; i++)
                    threads[i] = GetPostsAsync(factory.GetBooru(profile.Source)!, profile, stream, perThread, profile.BatchSize);

                await Task.WhenAll(threads);
            }

            stopWatch.Stop();

            Console.WriteLine("Finished in " + stopWatch.ElapsedMilliseconds / 1000 + " seconds.");
        }

        private static async Task GetPostsAsync(ABooru booruInstance, BooruProfile profile, StreamWriter stream, int total, int batch)
        {
            int processed = 0;

            while (processed < total)
            {
                SearchResult[] results;

                try
                {
                    if (total - processed < batch)
                        results = await booruInstance.GetRandomPostsAsync(total - processed, profile.Filter);
                    else
                        results = await booruInstance.GetRandomPostsAsync(batch, profile.Filter);
                }
                catch (Exception)
                {
                    Console.WriteLine("Encountered an exception, continuing gathering a new batch.");
                    processed -= batch;
                }
                finally
                {
                    results = Array.Empty<SearchResult>();
                }

                for (int i = 0; i < results.Length; i++)
                {
                    SearchResult result = results[i];
                    if (!profile.MatureContentAllowed && (int)result.Rating > 1)
                        processed--;
                    else
                    {
                        string extension = Path.GetExtension(result.FileUrl.GetLeftPart(UriPartial.Path));

                        if (profile.FileFilters.Contains(extension.ToLower()))
                        {
                            string line = $"\"{result.FileUrl}\", \"{result.PreviewUrl}\", \"{result.PostUrl}\", \"{result.SampleUri}\", \"{result.Rating}\", " +
                                        $"\"{string.Join(',', result.Tags)}\", \"{result.ID}\", \"{result.Height}\", \"{result.Width}\", \"{result.PreviewHeight}\", \"{result.PreviewWidth}\", " +
                                        $"\"{result.Creation}\", \"{result.Source}\", \"{result.Score}\", \"{result.MD5}\", \"{Path.Join(profile.SaveLocation, $"{result.ID}{extension}")}\"";
                            lock (stream)
                            {
                                stream.WriteLine(line);
                            }

                            if (profile.DownloadImages)
                            {
                                using HttpClient httpClient = new HttpClient();

                                string path = Path.Combine(profile.SaveLocation, $"{result.ID}{extension}");
                                byte[] image = await httpClient.GetByteArrayAsync(result.FileUrl);

                                await File.WriteAllBytesAsync(path, image);
                            }
                        }
                    }
                }

                lock (stream)
                {
                    stream.Flush();
                }

                processed += results.Length;
            }
        }
    }
}