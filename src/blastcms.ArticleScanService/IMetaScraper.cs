
using System.Threading.Tasks;

namespace blastcms.ArticleScanService
{
    public interface IMetaScraper
    {
        Task<MetaInformation> GetMetaDataFromUrl(string url);
    }
}
