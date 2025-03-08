using System.Threading.Tasks;

namespace blastcms.ArticleScanService.CaptureMeta
{
    public interface ICaptureMeta
    {
        public Task<CaptureMetaResult> GetMeta(string url);
    }

    public record CaptureMetaResult(string Data)
    {
        
    }
}
