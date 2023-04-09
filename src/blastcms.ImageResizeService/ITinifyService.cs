using System.Threading.Tasks;

namespace blastcms.ImageResizeService
{
    public interface ITinifyService
    {
        Task<byte[]> OptomizeFile(byte[] buffer);
        Task<byte[]> OptomizeFile(string url);
    }
}