using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShareFlow.Application.Revenue.Interfaces;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.Services;

/// <summary>
/// 调用 Claude API 从截图中提取平台收益数据。
/// 配置项：Ai:ClaudeApiKey
/// </summary>
public class ClaudeAiRevenueAgent(
    IHttpClientFactory clientFactory,
    IConfiguration configuration,
    IRegionContext regionContext) : IAiRevenueSkillAgent
{
    private const string ClaudeApiUrl = "https://api.anthropic.com/v1/messages";
    private const string ClaudeModel = "claude-opus-4-5";

    public async Task<AiRevenueExtractResult> ExtractAsync(
        string imageBase64, string mimeType, string platform, CancellationToken ct = default)
    {
        var apiKey = configuration["Ai:ClaudeApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // 无 API Key 时返回低置信度占位结果，触发人工核实
            return new AiRevenueExtractResult
            {
                Amount = 0,
                Currency = regionContext.DefaultCurrency,
                Date = DateTime.UtcNow.Date,
                Confidence = 0,
                RawJson = "{\"error\":\"Claude API key not configured\"}",
            };
        }

        var prompt = BuildPrompt(platform, regionContext.DefaultCurrency);

        var requestBody = new
        {
            model = ClaudeModel,
            max_tokens = 512,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = mimeType,
                                data = imageBase64,
                            },
                        },
                        new { type = "text", text = prompt },
                    },
                },
            },
        };

        var client = clientFactory.CreateClient("Claude");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(ClaudeApiUrl, content, ct);
        var responseJson = await response.Content.ReadAsStringAsync(ct);

        return ParseClaudeResponse(responseJson);
    }

    private static string BuildPrompt(string platform, string currency)
    {
        return $"You are a revenue data extraction assistant for {platform} platform.\n" +
               "Extract the TOTAL revenue amount, currency, and date from this screenshot.\n\n" +
               "Respond ONLY with a JSON object in this exact format (no markdown, no explanation):\n" +
               $"{{\"amount\": 123.45, \"currency\": \"{currency}\", \"date\": \"2024-01-15\", \"confidence\": 0.95}}\n\n" +
               "Rules:\n" +
               "- amount: numeric value only, no currency symbols\n" +
               "- currency: ISO 4217 code (USD, CNY, etc.)\n" +
               "- date: ISO 8601 format (YYYY-MM-DD)\n" +
               "- confidence: 0.0 to 1.0 (how confident you are in the extraction)\n" +
               "- If you cannot extract data reliably, set confidence below 0.7";
    }

    private static AiRevenueExtractResult ParseClaudeResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var textContent = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            using var resultDoc = JsonDocument.Parse(textContent.Trim());
            var root = resultDoc.RootElement;

            return new AiRevenueExtractResult
            {
                Amount = root.TryGetProperty("amount", out var amt) ? amt.GetDecimal() : 0,
                Currency = root.TryGetProperty("currency", out var cur) ? cur.GetString() ?? "USD" : "USD",
                Date = root.TryGetProperty("date", out var dt)
                    ? DateTime.Parse(dt.GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd"))
                    : DateTime.UtcNow.Date,
                Confidence = root.TryGetProperty("confidence", out var conf) ? conf.GetDecimal() : 0.5m,
                RawJson = responseJson,
            };
        }
        catch
        {
            return new AiRevenueExtractResult
            {
                Amount = 0,
                Currency = "USD",
                Date = DateTime.UtcNow.Date,
                Confidence = 0,
                RawJson = responseJson,
            };
        }
    }
}
