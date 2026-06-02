namespace ShareFlow.Application.Common.Interfaces;

/// <summary>对象存储服务（MinIO / S3 兼容）</summary>
public interface IStorageService
{
    /// <summary>上传字节数组到存储桶</summary>
    Task UploadAsync(string objectName, byte[] data, string contentType, CancellationToken ct = default);

    /// <summary>生成预签名下载 URL（默认有效期 1 小时）</summary>
    Task<string> GetPresignedUrlAsync(string objectName, int expirySeconds = 3600, CancellationToken ct = default);

    /// <summary>下载对象存储文件内容</summary>
    Task<byte[]> DownloadAsync(string objectName, CancellationToken ct = default);

    /// <summary>确保 Bucket 存在（首次启动调用）</summary>
    Task EnsureBucketExistsAsync(CancellationToken ct = default);
}
