using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CrucibleCrawler.Blob
{
    public class BlobStorageWriter
    {
        private string _blobConnectionString;

        public BlobStorageWriter(string blobConnectionString)
        {
            _blobConnectionString = blobConnectionString;
           
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
            BlobServiceClient blobServiceClient = new BlobServiceClient(_blobConnectionString);

            BlobClient blobClient = new BlobClient(_blobConnectionString, containerName, blobName);


            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync();

            //write the string content to the blob using BlobServiceClient
            
            //await blobClient.UploadAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)), true);
            await blobClient.UploadAsync(BinaryData.FromString(content), overwrite: true);

           
            

            
        }
    }
}
