using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace blastcms.web.CloudStorage
{
    public interface ICloudStorage
    {
        Task<string> UploadFileAsync(IBrowserFile imageFile, string fileNameForStorage);
        Task<string> UploadFileAsync(string ImageFileUrl, string fileNameForStorage);
        Task DeleteFileAsync(string fileNameForStorage);
    }
}