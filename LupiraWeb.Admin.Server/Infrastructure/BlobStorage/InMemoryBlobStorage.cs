using System.Collections.Concurrent;
using LupiraWeb.Domain.Infrastructure.BlobStorage;

namespace LupiraWeb.Admin.Server.Infrastructure.BlobStorage;

public sealed class InMemoryBlobStorage : IBlobStorage
{
    private readonly ConcurrentDictionary<string, BlobContent> _blobs = new();

    public async Task<string> UploadAsync(Stream content, string contentType, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        var key = $"memory://{Guid.NewGuid():N}";
        _blobs[key] = new BlobContent(ms.ToArray(), contentType);
        return key;
    }

    public Task<BlobContent?> DownloadAsync(string blobRef, CancellationToken ct) =>
        Task.FromResult(_blobs.TryGetValue(blobRef, out var blob) ? blob : null);
}
