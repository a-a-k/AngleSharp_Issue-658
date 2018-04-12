using System;
using System.Collections.Generic;

namespace LiveTest.Search
{
    public class SearchResult
    {
        public string Zone { get; set; }

        public string Query { get; set; }

        public string SourceZoneName { get; set; }

        public DateTimeOffset Time { get; set; }

        public List<SearchResultItem> Items { get; set; }
    }
}
