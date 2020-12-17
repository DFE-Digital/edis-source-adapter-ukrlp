using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.StateManagement;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.AzureStorage
{
    public class BlobStateStore : IStateStore
    {
        private readonly ILogger<BlobStateStore> _logger;
        private readonly BlobContainerClient _containerClient;

        public BlobStateStore(
            StateConfiguration configuration,
            ILogger<BlobStateStore> logger)
        {
            _logger = logger;
            var blobClient = new BlobServiceClient(configuration.BlobConnectionString);
            _containerClient = blobClient.GetBlobContainerClient(configuration.BlobContainerName);
        }

        public async Task<string> GetStateAsync(string key, CancellationToken cancellationToken)
        {
            var blob = _containerClient.GetBlobClient($"{key}.txt");
            var exists = await blob.ExistsAsync(cancellationToken);
            if (!exists)
            {
                return null;
            }

            var download = await blob.DownloadAsync(cancellationToken);
            using (var stream = new MemoryStream())
            {
                await download.Value.Content.CopyToAsync(stream);

                var buffer = stream.ToArray();
                return Encoding.UTF8.GetString(buffer);
            }
        }

        public async Task SetStateAsync(string key, string value, CancellationToken cancellationToken)
        {
            var blob = _containerClient.GetBlobClient($"{key}.txt");

            var buffer = Encoding.UTF8.GetBytes(value);
            using (var stream = new MemoryStream(buffer))
            {
                await blob.UploadAsync(stream, cancellationToken);
            }
        }
    }
}