# Google Cloud Storage Implementation Specification

This specification outlines the pattern for implementing Google Cloud Storage (GCS) support in a SaaS application, including multi-tenancy support for file isolation and optional image optimization.

## 1. Overview

The module provides a unified interface for file operations (upload, delete) backed by Google Cloud Storage. It enforces tenant isolation by prefixing object names with the tenant identifier. It also integrates an image optimization step before storage.

## 2. Dependencies

The implementation requires the following external packages:

*   **Google.Cloud.Storage.V1**: The official Google Cloud Storage client library.
*   **Finbuckle.MultiTenant** (or equivalent): For resolving the current tenant context.
*   **Microsoft.Extensions.Configuration**: For retrieving credentials and bucket settings.

## 3. Configuration

The application requires the following configuration keys (in `launchSettings.json` or Environment Variables):

| Key | Type | Description |
| :--- | :--- | :--- |
| `GoogleCloudStorageBucket` | `string` | The name of the GCS bucket to use. |
| `GoogleCredentialFile` | `string` | (Optional) Path to the JSON key file. Use for local development. When deployed to Cloud Run (or other GCP services), omit this to use ambient [Application Default Credentials](https://cloud.google.com/docs/authentication/application-default-credentials). |
| `MaxAllowedFileUploadSize` | `integer` | Maximum allowed file size in bytes. |

## 4. Interfaces

### ICloudStorage

The core interface defining storage operations.

```csharp
using Microsoft.AspNetCore.Components.Forms;
using System.Threading.Tasks;

public interface ICloudStorage
{
    // Uploads a file from a Blazor IBrowserFile stream
    Task<string> UploadFileAsync(IBrowserFile imageFile, string fileNameForStorage);

    // Uploads a file from a URL (e.g., fetching an external image and storing it)
    Task<string> UploadFileAsync(string ImageFileUrl, string fileNameForStorage);

    // Deletes a file from storage
    Task DeleteFileAsync(string fileNameForStorage);
}
```

### ITinifyService (Optional Dependency)

Used for optimizing images before upload to save space and bandwidth.

```csharp
using System.Threading.Tasks;

public interface ITinifyService
{
    Task<byte[]> OptomizeFile(byte[] buffer);
    Task<byte[]> OptomizeFile(string url);
}
```

## 5. Implementation

### GoogleCloudStorage Class

**Responsibilities:**
1.  **Client Initialization**: Initializes `StorageClient` using either a provided credential file or Application Default Credentials.
2.  **Tenant Isolation**: Resolves the current tenant identifier using `IMultiTenantContextAccessor` and prepends it to the file path (`{TenantId}/{FileName}`).
3.  **Optimization**: Passes the file stream to `ITinifyService` for optimization.
4.  **Upload**: Uploads the optimized stream to the configured GCS bucket.
5.  **Clean up**: Disposes streams properly.

**Code Structure:**

```csharp
public class GoogleCloudStorage : ICloudStorage
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    private readonly IMultiTenantContextAccessor<CustomTenantInfo> _httpContextAccessor;
    private readonly ITinifyService _tinifyService; // Optional

    public GoogleCloudStorage(
        IConfiguration configuration, 
        IMultiTenantContextAccessor<CustomTenantInfo> httpContextAccessor, 
        ITinifyService tinifyService)
    {
        // 1. Initialize Credentials
        // 2. Create StorageClient
        // 3. Load config (Bucket Name, Max Size)
    }

    public async Task<string> UploadFileAsync(IBrowserFile imageFile, string fileNameForStorage)
    {
        // 1. Determine Tenant Folder
        // 2. Read Stream
        // 3. Optimize (Tinify)
        // 4. Upload to GCS
        // 5. Return Object Name
    }
    
    // ... Additional methods
}
```

## 6. Service Registration

Register the service in the Dependency Injection container (e.g., `Program.cs` or `Startup.cs`).

```csharp
// Program.cs

// Register dependent services
builder.Services.AddTransient<ITinifyService, TinifyService>(); // If using optimization

// Register Cloud Storage
builder.Services.AddSingleton<ICloudStorage, GoogleCloudStorage>();

// Ensure Config availability
// The implementation uses IConfiguration injected into the constructor.
```

## 7. Usage Example

Inject `ICloudStorage` into your services or controllers.

```csharp
public class MyFileHandler
{
    private readonly ICloudStorage _cloudStorage;

    public MyFileHandler(ICloudStorage cloudStorage)
    {
        _cloudStorage = cloudStorage;
    }

    public async Task HandleUpload(IBrowserFile file)
    {
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.Name);
        var storedPath = await _cloudStorage.UploadFileAsync(file, fileName);
        
        // storedPath will be "tenant-id/guid.ext"
    }
}
```
