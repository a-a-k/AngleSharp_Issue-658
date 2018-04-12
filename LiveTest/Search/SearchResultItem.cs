using System;

namespace LiveTest.Search
{
    public class SearchResultItem
    {
        public int Position { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        public int? Amount { get; set; }

        public Uri Link { get; set; }
    }
}
