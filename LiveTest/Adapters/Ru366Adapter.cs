using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using LiveTest.Search;

namespace LiveTest.Adapters
{
    public class Ru366Adapter : IAdapter
    {
        private static readonly Uri BaseUrl = new Uri("https://366.ru");

        public async Task<SearchResult> Execute(SearchCommand command)
        {
            var res = new SearchResult
            {
                Items = new List<SearchResultItem>(),
            };
            var url = $"{BaseUrl}_s/baseStore/?code={command.ZoneId}";
            var context = BrowsingContext.New(new Configuration().WithDefaultLoader().WithCookies());
            await context.OpenAsync(url);
            var page = 0;
            var haveNextPage = true;
            while (haveNextPage)
            {
                url = $"{BaseUrl}search/?page={page}&q={command.Query}";
                var priceHtml = await context.OpenAsync(url);
                this.ParseSearch(priceHtml, out var region, out haveNextPage, out var items);
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
                .QuerySelector("span.b-login-link.i-fw-b")
                .TextContent
                .Trim();
            var catalogItems = priceDoc
                .QuerySelector("div.c-prod-item-list")
                ?.QuerySelectorAll("div.c-prod-item.c-prod-item--grid");
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
                        var node = x.QuerySelector("a");
                        return new
                        {
                            Name = node.GetAttribute("data-gtm-name") ?? "-no name-",
                        };
                    })
                    .Select(x => new SearchResultItem
                    {
                        Name = x.Name,
                    })
                    .ToList();

                var nextBtn = priceDoc
                    .QuerySelector("div.b-pagination a i.b-icn--next");
                haveNextPage = nextBtn != null;
            }
        }
    }
}
