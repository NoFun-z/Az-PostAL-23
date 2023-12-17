using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Az_PostAL_23.Services
{
    public class ContainerService : IContainerService
    {

        private readonly BlobServiceClient _blobClient;

        public ContainerService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }

        public async Task CreateContainer(string containerName, string containerType)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);

            PublicAccessType publicAccessType = PublicAccessType.BlobContainer;

            if (containerType == "Private") publicAccessType = PublicAccessType.None;
            else publicAccessType = PublicAccessType.BlobContainer;

            await blobContainerClient.CreateIfNotExistsAsync(publicAccessType);
        }

        public async Task DeleteContainer(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            await blobContainerClient.DeleteIfExistsAsync();
        }

        public async Task<List<string>> GetAllContainer()
        {
            List<string> containerName = new();

            await foreach (BlobContainerItem blobkContainerItem in _blobClient.GetBlobContainersAsync())
            {
                containerName.Add(blobkContainerItem.Name);
            }

            return containerName;
        }
    }
}
