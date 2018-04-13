using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Network;
using AngleSharp.Network.Default;
using LiveTest.Search;
using Newtonsoft.Json.Linq;

namespace LiveTest.Adapters
{
    public class PiluliAdapter : IAdapter
    {
        private static readonly Uri BaseUrl = new Uri("https://piluli.ru");

        public PiluliAdapter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Take a look at the comment in Programm.cs : line 24
        }

        public async Task<SearchResult> Execute(SearchCommand command)
        {
            var zone = command.ZoneId.ToLowerInvariant();
            var res = new SearchResult
            {
                Items = new List<SearchResultItem>(),
            };

            var query = HttpUtility.UrlEncode(command.Query, Encoding.GetEncoding(1251));
            var baseZone = BaseUrl.AbsoluteUri.Insert(8, $"{zone}.");
            var context = BrowsingContext.New(new Configuration().WithDefaultLoader().WithCookies());
            var next = new AngleSharp.Url($"{baseZone}search_result.html");
            var url = $"{baseZone}search_result.html?searchback=&search={query}";
            var priceHtml = await context.OpenAsync(
                new DocumentRequest(AngleSharp.Url.Create(url))
                {
                    Headers =
                    {
                        { "Host", baseZone },
                        { "Connection", "keep-alive" },
                        { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8" },
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299" },
                        { "DNT", "1" },
                        { "Referer", url },
                        { "Upgrade-Insecure-Requests", "1" },
                        { "Accept-Encoding", "gzip, deflate, br" },
                        { "Accept-Language", "en-US,en;q=0.7,ru;q=0.3" },
                    },
                    Method = HttpMethod.Get
                },
                CancellationToken.None);
            var more = HasManyPages(priceHtml);
            var page = 2;
            while (more)
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        await writer.WriteAsync($"pStart=NaN&pEnd=NaN&search={command.Query}&page={page}");
                        await writer.FlushAsync();
                        stream.Position = 0;
                        var headers = new Dictionary<string, string>
                        {
                            { "Host", baseZone },
                            { "Connection", "keep-alive" },
                            { "Cache-Control", "no-cache" },
                            { "Accept", "*/*" },
                            { "Origin", baseZone },
                            { "X-Requested-With", "XMLHttpRequest" },
                            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299" },
                            { "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8" },
                            { "Content-Length", stream.Length.ToString() },
                            { "DNT", "1" },
                            { "Referer", url },
                            { "Accept-Encoding", "gzip, deflate, br" },
                            { "Accept-Language", "en-US,en;q=0.7,ru;q=0.3" },
                        };
                        var response = await context
                            .Configuration
                            .Services
                            .OfType<HttpRequester>()
                            .First()
                            .RequestAsync(
                                new Request
                                {
                                    Address = next,
                                    Method = HttpMethod.Post,
                                    Content = stream,
                                    Headers = headers
                                },
                                CancellationToken.None);
                        using (var reader = new StreamReader(response.Content))
                        {
                            var json = JObject.Parse(await reader.ReadToEndAsync());
                            var html = json["html"].ToObject<string>();
                            more = !string.IsNullOrWhiteSpace(json["more"].ToObject<string>());
                            priceHtml.Body.QuerySelectorAll("li.product_item-price_buy-b").Last().Insert(AdjacentPosition.AfterEnd, html);
                        }
                    }
                }

                page++;
            }

            ParseSearch(priceHtml, out var region, out var items);
            res.SourceZoneName = region;
            res.Items.AddRange(items);
            return res;
        }

        private void ParseSearch(IDocument html, out string region, out List<SearchResultItem> items)
        {
            var priceDoc = html.DocumentElement;
            var reg = priceDoc
                .QuerySelector("span[itemprop='addressLocality']")
                .TextContent
                .Trim();
            region = ConvertEncoding(reg, Encoding.GetEncoding(1251), Encoding.UTF8);
            var catalogItems = priceDoc
                .QuerySelector("div.search-page--results")
                ?.QuerySelectorAll("li.product_item-price_buy-b");
            if (catalogItems == null)
            {
                items = new List<SearchResultItem>();
            }
            else
            {
                items = catalogItems
                    .Select(x => new
                    {
                        Name = x.QuerySelector("meta[itemprop='name']")?.GetAttribute("content") ?? "-no name-",
                    })
                    .Select(x => new SearchResultItem
                    {
                        Name = ConvertEncoding(x.Name, Encoding.GetEncoding(1251), Encoding.UTF8),
                    })
                    .ToList();
            }
        }

        private static string ConvertEncoding(string source, Encoding sourcEncoding, Encoding targetEncoding) => targetEncoding.GetString(Encoding.Convert(sourcEncoding, targetEncoding, sourcEncoding.GetBytes(source)));

        private static bool HasManyPages(IDocument priceHtml) => priceHtml.All.FirstOrDefault(f => f.Id == "ws_more_search_res" && string.IsNullOrWhiteSpace(f.GetAttribute("style"))) != null;
    }
}
