using System;
using System.IO;
using Azure.Storage.Blobs;

namespace helpsharp.Azure
{
    /// <summary>
    ///     Azure tasks
    /// </summary>
    public class AzureUtils
    {
        #region Public Methods

        /// <summary>
        ///     Uploads to BLOB storage.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="accountKey">The account key.</param>
        /// <param name="container">The container.</param>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destFile">The dest file.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     accountName or accountKey or container or sourceFile
        /// </exception>
        public void UploadToBlobStorage(string connectionString, string container, string sourceFile,
            string destFile)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (sourceFile == null)
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            var blobContainerClient = new BlobContainerClient(connectionString, container);// account.CreateCloudBlobClient();

            blobContainerClient.CreateIfNotExists();

            var blockBlob = blobContainerClient.GetBlobClient(destFile);
            using (var fileStream = File.OpenRead(sourceFile))
            {
                blockBlob.Upload(fileStream);
            }
        }

        #endregion Public Methods
    }
}