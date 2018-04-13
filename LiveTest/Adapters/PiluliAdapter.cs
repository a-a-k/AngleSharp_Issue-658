using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using LiveTest.Search;

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
            var res = new SearchResult();
            var query = HttpUtility.UrlEncode(command.Query, Encoding.GetEncoding(1251));
            var baseZone = BaseUrl.AbsoluteUri.Insert(8, $"{zone}.");
            var context = BrowsingContext.New(new Configuration().WithDefaultLoader().WithCookies());
            var url = $"{baseZone}search_result.html?searchback=&search={query}";
            var priceHtml = await context.OpenAsync(url);
            res.SourceZoneName = priceHtml
                .DocumentElement
                .QuerySelector("span[itemprop='addressLocality']")
                .TextContent
                .Trim(); ;
            return res;
        }
    }
}
