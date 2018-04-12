using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using LiveTest.Search;

namespace LiveTest.Adapters
{
    public class EaptekaAdapter : IAdapter
    {
        private static readonly Regex Whitespaces = new Regex(@"\s+");
        private static readonly Uri BaseUrl = new Uri("https://www.eapteka.ru");

        public async Task<SearchResult> Execute(SearchCommand command)
        {
            var zoneId = command.ZoneId.ToLowerInvariant();
            var res = new SearchResult
            {
                Items = new List<SearchResultItem>(),
            };
            var context = BrowsingContext.New(new Configuration().WithDefaultLoader());
            var page = 1;
            var haveNextPage = true;
            while (haveNextPage)
            {
                var url = $"{BaseUrl}{(zoneId == "mow" ? $"search/?q={command.Query}&PAGEN_238={page}" : $"{zoneId}/search/?q={command.Query}&PAGEN_238={page}")}";
                var priceHtml = await context.OpenAsync(url);
                ParseSearch(priceHtml, out var region, out haveNextPage, out var items);
                res.SourceZoneName = region;
                res.Items.AddRange(items);
                page++;
            }

            return res;
        }

        private void ParseSearch(IDocument html, out string region, out bool haveNextPage, out List<SearchResultItem> items)
        {
            var priceDoc = html.DocumentElement;
            region = priceDoc
                .QuerySelector("span.select_title")
                ?.TextContent
                .Trim();
            var catalogItems = priceDoc
                .QuerySelectorAll("div[itemtype='http://schema.org/Offer']");
            if (catalogItems == null)
            {
                items = new List<SearchResultItem>();
                haveNextPage = false;
            }
            else
            {
                items = catalogItems
                    .Select(x =>
                    {
                        var name = x.QuerySelector("*[itemprop=name]");
                        return new
                        {
                            Name = NormalizeWhitespace(name.GetAttribute("content")) ?? "-no name-",
                        };
                    })
                    .Select(x => new SearchResultItem
                    {
                        Name = x.Name,
                    })
                    .ToList();

                var nextBtn = priceDoc.QuerySelector("ul.clearfix li a[rel='next']");
                haveNextPage = nextBtn != null;
            }
        }

        private static string NormalizeWhitespace(string input) => input == null ? null : Whitespaces.Replace(input, " ").Trim();
    }
}
