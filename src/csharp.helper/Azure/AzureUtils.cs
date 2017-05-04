using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace csharp.helper.Azure
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
        public void UploadToBlobStorage(string accountName, string accountKey, string container, string sourceFile,
            string destFile)
        {
            if (accountName == null)
            {
                throw new ArgumentNullException(nameof(accountName));
            }

            if (accountKey == null)
            {
                throw new ArgumentNullException(nameof(accountKey));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (sourceFile == null)
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            var account = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), false);
            var cbcClient = account.CreateCloudBlobClient();
            var cbcContainer = cbcClient.GetContainerReference(container);
            cbcContainer.CreateIfNotExists();

            var blockBlob = cbcContainer.GetBlockBlobReference(destFile);
            using (var fileStream = File.OpenRead(sourceFile))
            {
                blockBlob.UploadFromStream(fileStream);
            }
        }

        #endregion Public Methods
    }
}