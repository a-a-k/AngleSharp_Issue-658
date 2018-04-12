using System.Threading.Tasks;
using LiveTest.Search;

namespace LiveTest.Adapters
{
    public interface IAdapter
    {
        Task<SearchResult> Execute(SearchCommand command);
    }
}
