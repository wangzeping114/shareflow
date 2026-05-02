using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using ShareFlow.Application.Common.Interfaces;

namespace ShareFlow.Infrastructure.Services;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucketName;

    public MinioStorageService(IConfiguration config)
    {
        var endpoint  = config["MinIO:Endpoint"]   ?? "localhost:9000";
        var accessKey = config["MinIO:AccessKey"]  ?? "shareflow";
        var secretKey = config["MinIO:SecretKey"]  ?? "shareflow123";
        var useSSL    = bool.TryParse(config["MinIO:UseSSL"], out var ssl) && ssl;
        _bucketName   = config["MinIO:BucketName"] ?? "shareflow-contracts";

        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSSL)
            .Build();
    }

    public async Task EnsureBucketExistsAsync(CancellationToken ct = default)
    {
        var exists = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_bucketName), ct);

        if (!exists)
        {
            await _client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_bucketName), ct);
        }
    }

    public async Task UploadAsync(string objectName, byte[] data, string contentType, CancellationToken ct = default)
    {
        await EnsureBucketExistsAsync(ct);

        using var stream = new MemoryStream(data);
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(data.Length)
            .WithContentType(contentType), ct);
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, int expirySeconds = 3600, CancellationToken ct = default)
    {
        return await _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithExpiry(expirySeconds));
    }
}
