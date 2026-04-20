namespace LupiraWeb.Server.Infrastructure.BlobStorage;

public interface IBlobStorage
{
    Task<string> UploadAsync(Stream content, string contentType, CancellationToken ct);
    Task<BlobContent?> DownloadAsync(string blobRef, CancellationToken ct);
}

public sealed record BlobContent(byte[] Bytes, string ContentType);
