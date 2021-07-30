
namespace blastcms.ArticleScanService
{
    public interface IMetaScraper
    {
        MetaInformation GetMetaDataFromUrl(string url);
    }
}
