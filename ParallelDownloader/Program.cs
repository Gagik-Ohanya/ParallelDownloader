using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ParallelDownloader
{
    class Program
    {
        static HttpClient _client = new HttpClient();
        static HashSet<string> _links = new HashSet<string>();

        static async Task Main(string[] args)
        {
            try
            {
                await ExtractLinksFromWebPage("https://dataart.com");

                Parallel.ForEach(_links, async (link, state, index) =>
                {
                    using (var result = await _client.GetAsync(link))
                    {
                        if (result.IsSuccessStatusCode)
                        {
                            var bytes = await result.Content.ReadAsByteArrayAsync();
                            await File.WriteAllBytesAsync("..\\..\\..\\WebPages\\file" + index + ".html", bytes);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        static async Task ExtractLinksFromWebPage(string url)
        {
            string result = "";
            try
            {
                using (var response = await _client.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        result = await content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception)
            {
                _links.Remove(url);
                return;
            }
            var document = new HtmlDocument();
            document.LoadHtml(result);
            var nodes = document.DocumentNode.SelectNodes("//a[@href]") ?? new HtmlNodeCollection(null);
            foreach (var node in nodes)
            {
                string link = node.Attributes["href"].Value;
                if (!_links.Contains(link) && link.Contains("dataart.com") && !link.Contains("mailto"))
                {
                    Console.WriteLine(link);
                    _links.Add(link);
                    await ExtractLinksFromWebPage(link);
                }
            }
        }
    }
}