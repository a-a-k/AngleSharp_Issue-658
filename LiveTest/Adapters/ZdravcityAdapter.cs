using System;
using System.Threading.Tasks;
using AngleSharp;
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
            var res = new SearchResult();
            var baseUrl = command.ZoneId == MskZone ? MskUrl : new Uri(string.Format(BaseUrlPattern, command.ZoneId.ToLowerInvariant()));
            var url = $"{baseUrl}search.php?order=Y&what={Uri.EscapeDataString(command.Query)}";
            var context = BrowsingContext.New(new Configuration().WithDefaultLoader().WithCookies());
            var priceHtml = await context.OpenAsync(url);
            res.SourceZoneName = priceHtml
                                     .DocumentElement
                                     .QuerySelector("input#hide-reg-cur-name")
                                     ?.GetAttribute("value")
                                     .Trim()
                                 ?? "unknown";
            return res;
        }
    }
}
