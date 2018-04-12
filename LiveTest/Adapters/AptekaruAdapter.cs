using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using LiveTest.Search;

namespace LiveTest.Adapters
{
    public class AptekaruAdapter : IAdapter
    {
        private static readonly Uri BaseUrl = new Uri("https://apteka.ru");

        public async Task<SearchResult> Execute(SearchCommand command)
        {
            var res = new SearchResult
            {
                Items = new List<SearchResultItem>(),
            };

            var url = $"{BaseUrl}_action/geoip/setBranch/{command.ZoneId}/";
            var context = BrowsingContext.New(new Configuration().WithDefaultLoader().WithCookies());
            await context.OpenAsync(url);
            var page = 1;
            var haveNextPage = true;
            while (haveNextPage)
            {
                url = $"{BaseUrl}search/?PAGEN_products={page}&q={command.Query}";
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

            region = priceDoc.QuerySelector("div.region-select__name")?.TextContent.Trim() ?? "Unknown :(";

            var catalogItems = priceDoc
                .QuerySelector("div.items.catalog-items")
                ?.QuerySelectorAll("article.item.catalog-item");

            if (catalogItems == null)
            {
                items = new List<SearchResultItem>();
                haveNextPage = false;
            }
            else
            {
                items = catalogItems
                    .Select(x => new
                    {
                        Name = x.GetAttribute("data-product-name") ?? "-no name-",
                    })
                    .Select(x => new SearchResultItem
                    {
                        Name = x.Name,
                    })
                    .ToList();

                var nextBtn = priceDoc
                    .QuerySelector("ul.pagin_items li.arrow_next a");
                haveNextPage = nextBtn != null;
            }
        }
    }
}
