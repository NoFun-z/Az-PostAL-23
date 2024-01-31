using Az_PostAL_23.Data;
using Az_PostAL_23.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata;
using static System.Reflection.Metadata.BlobBuilder;

namespace Az_PostAL_23.Services
{
    public class BlobService : IBlobService
    {

        private readonly BlobServiceClient _blobClient;
        private readonly IServiceProvider _serviceProvider;

        public BlobService(BlobServiceClient blobClient, IServiceProvider serviceProvider)
        {
            _blobClient = blobClient;
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> DeleteBlob(string name, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(name);

            return await blobClient.DeleteIfExistsAsync();
        }

        public async Task<List<string>> GetAllBlobs(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobs = blobContainerClient.GetBlobsAsync();

            var blobString = new List<string>();

            await foreach (var item in blobs)
            {
                blobString.Add(item.Name);
            }

            return blobString;
        }

        public async Task<List<Transaction>> GetAllBlobsWithUri()
        {
            var blobList = new List<Transaction>();
            string sasContainerSignature = "";
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                List<string> Categories = dbContext.Categories.Select(c => c.Title.ToLower()).ToList();
                foreach (var containerName in Categories)
                {
                    BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
                    var blobs = blobContainerClient.GetBlobsAsync();

                    if (blobContainerClient.CanGenerateSasUri)
                    {
                        BlobSasBuilder sasBuilder = new()
                        {
                            BlobContainerName = blobContainerClient.Name,
                            Resource = "c",
                            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                        };

                        sasBuilder.SetPermissions(BlobSasPermissions.Read);

                        sasContainerSignature = blobContainerClient.GenerateSasUri(sasBuilder).AbsoluteUri.Split('?')[1].ToString();
                    }



                    await foreach (var item in blobs)
                    {
                        var blobClient = blobContainerClient.GetBlobClient(item.Name);
                        Transaction blobIndividual = new()
                        {
                            Uri = blobClient.Uri.AbsoluteUri + (!sasContainerSignature.IsNullOrEmpty() ? "?" + sasContainerSignature : "")
                        };

                        blobIndividual.UriName = item.Name;

                        BlobProperties properties = await blobClient.GetPropertiesAsync();

                        if (properties.Metadata.ContainsKey("transactionId"))
                        {
                            blobIndividual.TransactionId = Convert.ToInt16(properties.Metadata["transactionId"]);
                        }
                        if (properties.Metadata.ContainsKey("category"))
                        {
                            using (var scope2 = _serviceProvider.CreateScope())
                            {
                                var dbContext2 = scope2.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                                blobIndividual.CategoryId = dbContext.Categories
                                    .Where(c => c.Title.ToLower() == properties.Metadata["category"])
                                    .FirstOrDefault().CategoryId;

                                blobIndividual.Category = dbContext.Categories.Find(blobIndividual.CategoryId);
                            }
                        }
                        if (properties.Metadata.ContainsKey("amount"))
                        {
                            blobIndividual.Amount = double.Parse(properties.Metadata["amount"]);
                        }
                        if (properties.Metadata.ContainsKey("date"))
                        {
                            blobIndividual.Date = DateTime.Parse(properties.Metadata["date"]);
                        }
                        blobList.Add(blobIndividual);
                    }
                }
            }

            return blobList;
        }

        public async Task<string> GetBlob(string name, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(name);

            return blobClient.Uri.AbsoluteUri;
        }

        public async Task<bool> UploadBlob(string name, IFormFile file, string containerName, Transaction transaction)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(name);

            var httpHeaders = new BlobHttpHeaders()
            {
                ContentType = file.ContentType
            };

            IDictionary<string, string> metadata =
             new Dictionary<string, string>();

            metadata.Add("category", containerName);
            metadata["transactionId"] = transaction.TransactionId.ToString();
            metadata["amount"] = transaction.Amount.ToString();
            metadata["date"] = transaction.Date.ToString("MMM-dd-yy");
            metadata["note"] = transaction.Note;

            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders, metadata);

            //metadata.Remove("title");

            //await blobClient.SetMetadataAsync(metadata);

            if (result != null)
            {
                return true;
            }
            return false;
        }
    }
}
