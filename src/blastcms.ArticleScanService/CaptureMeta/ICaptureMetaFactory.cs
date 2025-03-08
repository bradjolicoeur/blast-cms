namespace blastcms.ArticleScanService.CaptureMeta
{
    public interface ICaptureMetaFactory
    {
        ICaptureMeta GetCaptureMeta(string url);
    }
}