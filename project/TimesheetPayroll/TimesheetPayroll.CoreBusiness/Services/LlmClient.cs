using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;

namespace TimesheetPayroll.CoreBusiness.Services;

/// <summary>
/// Dịch vụ kết nối và giao tiếp trực tiếp với Mô hình Ngôn ngữ Lớn LLM (Google Gemini API / OpenAI API).
/// Sử dụng HttpClient tiêu chuẩn .NET, không phụ thuộc vào thư viện ngoài.
/// </summary>
public class LlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private AiConfiguration _config;

    public LlmClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? new HttpClient();
        _config = new AiConfiguration
        {
            Provider = AiProviderType.Gemini,
            Model = "gemini-2.0-flash",
            IsEnabled = true,
            // Tự động kiểm tra biến môi trường GEMINI_API_KEY nếu có
            ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty
        };
    }

    public bool IsConfigured
    {
        get
        {
            if (!_config.IsEnabled) return false;
            if (_config.Provider == AiProviderType.LocalEngine) return false;
            return !string.IsNullOrWhiteSpace(_config.ApiKey);
        }
    }

    public string ProviderName => _config.Provider switch
    {
        AiProviderType.Gemini => "Google Gemini",
        AiProviderType.OpenAi => "OpenAI (ChatGPT)",
        _ => "Động cơ Phân tích Nội bộ"
    };

    public string ActiveModel => _config.Model;

    public void UpdateConfiguration(AiConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public AiConfiguration GetConfiguration()
    {
        return new AiConfiguration
        {
            Provider = _config.Provider,
            ApiKey = _config.ApiKey,
            Model = _config.Model,
            IsEnabled = _config.IsEnabled,
            CustomKnowledgeBase = _config.CustomKnowledgeBase,
            KnowledgeBaseFilePath = _config.KnowledgeBaseFilePath
        };
    }

    public async Task<string?> GenerateResponseAsync(
        string systemPrompt, 
        string userPrompt, 
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured) return null;

        try
        {
            if (_config.Provider == AiProviderType.Gemini)
            {
                return await CallGeminiAsync(systemPrompt, userPrompt, cancellationToken);
            }
            else if (_config.Provider == AiProviderType.OpenAi)
            {
                return await CallOpenAiAsync(systemPrompt, userPrompt, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LlmClient] Lỗi khi gọi API {_config.Provider}: {ex.Message}");
        }

        return null;
    }

    private async Task<string?> CallGeminiAsync(
        string systemPrompt, 
        string userPrompt, 
        CancellationToken cancellationToken)
    {
        string model = string.IsNullOrWhiteSpace(_config.Model) ? "gemini-2.0-flash" : _config.Model;
        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_config.ApiKey.Trim()}";

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new { text = systemPrompt }
                }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = userPrompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.3,
                maxOutputTokens = 2048
            }
        };

        string jsonPayload = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"[LlmClient] Gemini API trả về lỗi {response.StatusCode}: {errorBody}");
            return null;
        }

        string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);

        if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
            candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0)
            {
                var firstPart = parts[0];
                if (firstPart.TryGetProperty("text", out var textElement))
                {
                    return textElement.GetString();
                }
            }
        }

        return null;
    }

    private async Task<string?> CallOpenAiAsync(
        string systemPrompt, 
        string userPrompt, 
        CancellationToken cancellationToken)
    {
        string model = string.IsNullOrWhiteSpace(_config.Model) ? "gpt-4o-mini" : _config.Model;
        string endpoint = "https://api.openai.com/v1/chat/completions";

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.3
        };

        string jsonPayload = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey.Trim());

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"[LlmClient] OpenAI API trả về lỗi {response.StatusCode}: {errorBody}");
            return null;
        }

        string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);

        if (doc.RootElement.TryGetProperty("choices", out var choices) &&
            choices.GetArrayLength() > 0)
        {
            var firstChoice = choices[0];
            if (firstChoice.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var contentElement))
            {
                return contentElement.GetString();
            }
        }

        return null;
    }
}
