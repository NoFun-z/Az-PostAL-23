using Az_PostAL_23.Models;
using System.Reflection.Metadata;

namespace Az_PostAL_23.Services
{
    public interface IBlobService
    {
        Task<string> GetBlob(string name, string containerName);
        Task<List<string>> GetAllBlobs(string containerName);
        Task<List<Transaction>> GetAllBlobsWithUri();
        Task<bool> UploadBlob(string name, IFormFile file, string containerName, Transaction transaction);
        Task<bool> DeleteBlob(string name, string containerName);
    }
}
