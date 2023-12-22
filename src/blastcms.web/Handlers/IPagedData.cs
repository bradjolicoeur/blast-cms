using System.Collections.Generic;

namespace blastcms.web.Handlers
{
    public interface IPagedData<T> 
    {
        long Count { get; }
        IEnumerable<T> Data { get; }
        int Page { get; }
    }
}