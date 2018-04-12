using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using LiveTest.Search;

namespace LiveTest.Adapters
{
    public class ZdravcityAdapter : IAdapter
    {
        private static readonly string BaseUrlPattern = "https://{0}.zdravcity.ru";
        private static readonly Uri MskUrl = new Uri("https://zdravcity.ru");
        private static readonly string MskZone = "MOSCOW";

        public async Task<SearchResult> Execute(SearchCommand command)
        {
            var res = new SearchResult
            {
                Items = new List<SearchResultItem>(),
            };

            var baseUrl = command.ZoneId == MskZone ? MskUrl : new Uri(string.Format(BaseUrlPattern, command.ZoneId.ToLowerInvariant()));
            var url = $"{baseUrl}search.php?order=Y&what={Uri.EscapeDataString(command.Query)}";
            var context = BrowsingContext.New(new Configuration().WithDefaultLoader().WithCookies());
            var priceHtml = await context.OpenAsync(url);
            ParseSearch(priceHtml, out var region, out var items);
            res.SourceZoneName = region;
            res.Items.AddRange(items);
            return res;
        }

        private void ParseSearch(IDocument html, out string region, out List<SearchResultItem> items)
        {
            var priceDoc = html.DocumentElement;
            region = priceDoc.QuerySelector("input#hide-reg-cur-name")?.GetAttribute("value").Trim() ?? "unknown :(";
            var catalogItems = priceDoc
                .QuerySelector("div#tab-product-list")
                ?.QuerySelectorAll("li.product-item");
            if (catalogItems == null)
            {
                items = new List<SearchResultItem>();
            }
            else
            {
                items = catalogItems
                    .Select(x => new
                    {
                        Name = x.GetAttribute("data-name") ?? "-no name-",
                    })
                    .Select(x => new SearchResultItem
                    {
                        Name = x.Name,
                    })
                    .ToList();
            }
        }
    }
}
