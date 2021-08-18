using Finbuckle.MultiTenant;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace blastcms.web.CloudStorage
{
    public class GoogleCloudStorage : ICloudStorage
    {
        private readonly GoogleCredential _googleCredential;
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly int _maxAllowedUploadSize;
        private readonly IMultiTenantContextAccessor<TenantInfo> _httpContextAccessor;

        public GoogleCloudStorage(IConfiguration configuration, IMultiTenantContextAccessor<TenantInfo> httpContextAccessor)
        {
            if (string.IsNullOrEmpty(configuration.GetValue<string>("GoogleCredentialFile")))
            {
                _googleCredential = GoogleCredential.GetApplicationDefault();
            }
            else
            {
                _googleCredential = GoogleCredential.FromFile(configuration.GetValue<string>("GoogleCredentialFile"));
            }

            _storageClient = StorageClient.Create(_googleCredential);

            _bucketName = configuration.GetValue<string>("GoogleCloudStorageBucket");
            _maxAllowedUploadSize = configuration.GetValue<int>("MaxAllowedFileUploadSize");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadFileAsync(IBrowserFile imageFile, string fileNameForStorage)
        {
            var folder = _httpContextAccessor.MultiTenantContext?.TenantInfo?.Identifier ?? "unknown";
            var objectName = $"{folder}/{fileNameForStorage}";
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.OpenReadStream(maxAllowedSize: _maxAllowedUploadSize).CopyToAsync(memoryStream);
                var dataObject = await _storageClient.UploadObjectAsync(_bucketName, objectName , imageFile.ContentType, memoryStream);
                return dataObject.Name;
            }
        }

        public async Task DeleteFileAsync(string fileNameForStorage)
        {
            await _storageClient.DeleteObjectAsync(_bucketName, fileNameForStorage);
        }
    }
}