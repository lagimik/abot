using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CrucibleCrawler.Blob
{
    public class BlobStorageWriter
    {
        private string _blobUrl;
        private string _sasToken;
        private string _storageAccountName;

        public BlobStorageWriter(string blobUrl, string blobStorageAccountName, string sasToken)
        {
            _blobUrl = blobUrl;
            _storageAccountName = blobStorageAccountName;
            _sasToken = sasToken;
        }

        
        public async Task WriteStringToBlobAsync(string containerName, string blobName, string content)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.", nameof(containerName));
            }

            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException($"'{nameof(blobName)}' cannot be null or empty.", nameof(blobName));
            }

            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException($"'{nameof(content)}' cannot be null or empty.", nameof(content));
            }
            
            // Create a BlobServiceClient using the service URL and SAS token
            BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(_blobUrl), new StorageSharedKeyCredential(_storageAccountName, _sasToken));


            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync();

            // Get a reference to the blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Convert the string content to a byte array
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(content);

            // Upload the byte array to the blob
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                await blobClient.UploadAsync(stream, true);
            }
        }
    }
}
