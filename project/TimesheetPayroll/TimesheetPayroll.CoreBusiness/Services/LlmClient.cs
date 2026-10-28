using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;

namespace TimesheetPayroll.CoreBusiness.Services;

/// <summary>
/// Dịch vụ kết nối và giao tiếp trực tiếp với Mô hình Ngôn ngữ Lớn LLM (Google Gemini API / OpenAI API)
/// phục vụ duy nhất cho phân hệ Trợ Lý Lương AI.
/// Tuân thủ bảo mật:
/// - Ưu tiên đọc khóa từ biến môi trường EXTERNAL_API_KEY hoặc User Secrets ở tầng Backend.
/// - Xác thực định dạng khóa với nhà cung cấp trước khi gửi request.
/// - Sử dụng HTTP Header (x-goog-api-key / Authorization: Bearer) thay vì truyền khóa trên URL.
/// - Không bao giờ trả API Key ra HTML/JavaScript frontend, log hoặc thông báo lỗi.
/// </summary>
public class LlmClient : ILlmClient
{
    private const string VerifiedGeminiHost = "generativelanguage.googleapis.com";
    private const string VerifiedOpenAiHost = "api.openai.com";

    private readonly HttpClient _httpClient;
    private AiConfiguration _config;
    private string _lastDiagnosticStatus = "Chưa cấu hình biến môi trường EXTERNAL_API_KEY (Đang sử dụng Động cơ Phân tích Nội bộ).";

    public LlmClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? new HttpClient();

        string resolvedKey = ResolveBackendApiKey(null, out var detectedProvider, out var diagnostic);
        _lastDiagnosticStatus = diagnostic;

        _config = new AiConfiguration
        {
            Provider = detectedProvider ?? AiProviderType.Gemini,
            Model = (detectedProvider == AiProviderType.OpenAi) ? "gpt-4o-mini" : "gemini-2.5-flash",
            IsEnabled = true,
            ApiKey = resolvedKey
        };
    }

    public bool IsConfigured
    {
        get
        {
            if (!_config.IsEnabled) return false;
            if (_config.Provider == AiProviderType.LocalEngine) return false;
            return IsKeyValidForProvider(_config.ApiKey, _config.Provider, out _);
        }
    }

    public string ProviderName => _config.Provider switch
    {
        AiProviderType.Gemini when IsConfigured => "Google Gemini",
        AiProviderType.OpenAi when IsConfigured => "OpenAI (ChatGPT)",
        _ => "Động cơ Phân tích Nội bộ"
    };

    public string ActiveModel => IsConfigured ? _config.Model : "Local-HRM-2024";

    public string GetKeyDiagnosticStatus() => _lastDiagnosticStatus;

    /// <summary>
    /// Kiểm tra chuỗi có phải là giá trị placeholder mặc định hay không.
    /// </summary>
    public static bool IsPlaceholderKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return true;
        string trimmed = key.Trim();
        return trimmed.Equals("YOUR_GEMINI_API_KEY_HERE", StringComparison.OrdinalIgnoreCase) ||
               trimmed.Equals("YOUR_OPENAI_API_KEY_HERE", StringComparison.OrdinalIgnoreCase) ||
               trimmed.Equals("YOUR_API_KEY_HERE", StringComparison.OrdinalIgnoreCase) ||
               trimmed.Equals("EXTERNAL_API_KEY", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("<", StringComparison.Ordinal);
    }

    /// <summary>
    /// Nhận diện nhà cung cấp dịch vụ dựa trên tiền tố định dạng chuẩn của khóa API.
    /// </summary>
    public static AiProviderType? DetectProviderFromKeyFormat(string? apiKey)
    {
        if (IsPlaceholderKey(apiKey)) return null;
        string trimmed = apiKey!.Trim();

        // Google AI Studio Gemini API Key chuẩn bắt đầu bằng "AIza" (thường 39 ký tự)
        if (trimmed.StartsWith("AIza", StringComparison.Ordinal) && trimmed.Length >= 30)
        {
            return AiProviderType.Gemini;
        }

        // OpenAI API Key chuẩn bắt đầu bằng "sk-" (vd: sk-proj-... hoặc sk-...)
        if (trimmed.StartsWith("sk-", StringComparison.Ordinal) && trimmed.Length >= 20)
        {
            return AiProviderType.OpenAi;
        }

        return null;
    }

    /// <summary>
    /// Kiểm tra khóa API có hợp lệ và tương thích với nhà cung cấp đã chọn hay không.
    /// Tuyệt đối không gửi khóa không hợp lệ hoặc không rõ nhà cung cấp tới endpoint bên ngoài.
    /// </summary>
    public static bool IsKeyValidForProvider(string? apiKey, AiProviderType provider, out string diagnosticMessage)
    {
        if (provider == AiProviderType.LocalEngine)
        {
            diagnosticMessage = "Đang sử dụng Động cơ Phân tích Nội bộ (100% Offline, không dùng API Key).";
            return false;
        }

        if (IsPlaceholderKey(apiKey))
        {
            diagnosticMessage = "Chưa thiết lập biến môi trường EXTERNAL_API_KEY. Hệ thống tự động sử dụng Động cơ Phân tích Nội bộ.";
            return false;
        }

        var detected = DetectProviderFromKeyFormat(apiKey);
        if (detected == null)
        {
            diagnosticMessage = "API Key được cung cấp không khớp định dạng được hỗ trợ của Google Gemini (AIza...) hoặc OpenAI (sk-...). Để bảo mật, hệ thống từ chối gửi khóa tới endpoint bên ngoài và tự động dùng Động cơ Phân tích Nội bộ.";
            return false;
        }

        if (detected != provider)
        {
            string detectedName = detected == AiProviderType.Gemini ? "Google Gemini" : "OpenAI";
            string selectedName = provider == AiProviderType.Gemini ? "Google Gemini" : "OpenAI";
            diagnosticMessage = $"API Key thuộc dịch vụ {detectedName} nhưng cấu hình đang chọn {selectedName}. Hệ thống đã tự động ngăn gửi chéo endpoint.";
            return false;
        }

        diagnosticMessage = $"Đã xác thực định dạng khóa API hợp lệ cho dịch vụ {(provider == AiProviderType.Gemini ? "Google Gemini API" : "OpenAI Chat Completions API")}.";
        return true;
    }

    /// <summary>
    /// Đọc khóa API từ cấu hình Backend / Biến môi trường theo thứ tự ưu tiên chuẩn:
    /// 1. EXTERNAL_API_KEY (Biến môi trường hoặc User Secrets)
    /// 2. Khóa cấu hình trong User Secrets (AiSettings:ApiKey) nếu không phải placeholder
    /// 3. Biến môi trường GEMINI_API_KEY / OPENAI_API_KEY nếu hợp lệ
    /// </summary>
    public static string ResolveBackendApiKey(
        string? configuredKey,
        out AiProviderType? detectedProvider,
        out string diagnosticMessage)
    {
        detectedProvider = null;

        // 1. Ưu tiên cao nhất: Biến môi trường EXTERNAL_API_KEY
        string? externalKey = GetEnvVarAcrossScopes("EXTERNAL_API_KEY");
        if (!IsPlaceholderKey(externalKey))
        {
            detectedProvider = DetectProviderFromKeyFormat(externalKey);
            if (detectedProvider != null)
            {
                diagnosticMessage = $"Đã nạp khóa từ biến môi trường EXTERNAL_API_KEY ({(detectedProvider == AiProviderType.Gemini ? "Google Gemini" : "OpenAI")}).";
                return externalKey!.Trim();
            }

            diagnosticMessage = "Biến môi trường EXTERNAL_API_KEY đã được đặt nhưng không khớp định dạng hỗ trợ (Google Gemini 'AIza...' hoặc OpenAI 'sk-...'). Hệ thống không gửi khóa đi và chuyển sang Động cơ Nội bộ.";
            return string.Empty;
        }

        // 2. Cấu hình từ User Secrets / IConfiguration (nếu không phải placeholder)
        if (!IsPlaceholderKey(configuredKey))
        {
            detectedProvider = DetectProviderFromKeyFormat(configuredKey);
            if (detectedProvider != null)
            {
                diagnosticMessage = $"Đã nạp khóa từ cấu hình bảo mật Backend ({(detectedProvider == AiProviderType.Gemini ? "Google Gemini" : "OpenAI")}).";
                return configuredKey!.Trim();
            }

            diagnosticMessage = "Khóa trong cấu hình Backend không khớp định dạng nhà cung cấp được xác minh (AIza... hoặc sk-...). Hệ thống chuyển sang Động cơ Nội bộ.";
            return string.Empty;
        }

        // 3. Kiểm tra biến môi trường GEMINI_API_KEY / OPENAI_API_KEY trên hệ thống
        string? geminiEnv = GetEnvVarAcrossScopes("GEMINI_API_KEY");
        if (!IsPlaceholderKey(geminiEnv))
        {
            var gDetected = DetectProviderFromKeyFormat(geminiEnv);
            if (gDetected == AiProviderType.Gemini)
            {
                detectedProvider = AiProviderType.Gemini;
                diagnosticMessage = "Đã nạp khóa Google Gemini hợp lệ từ biến môi trường hệ thống.";
                return geminiEnv!.Trim();
            }
        }

        string? openAiEnv = GetEnvVarAcrossScopes("OPENAI_API_KEY");
        if (!IsPlaceholderKey(openAiEnv))
        {
            var oDetected = DetectProviderFromKeyFormat(openAiEnv);
            if (oDetected == AiProviderType.OpenAi)
            {
                detectedProvider = AiProviderType.OpenAi;
                diagnosticMessage = "Đã nạp khóa OpenAI hợp lệ từ biến môi trường hệ thống.";
                return openAiEnv!.Trim();
            }
        }

        if (!IsPlaceholderKey(geminiEnv))
        {
            diagnosticMessage = "Phát hiện biến môi trường GEMINI_API_KEY nhưng khóa không có định dạng Google AI Studio chuẩn (AIza...). Vui lòng cấu hình khóa hợp lệ qua biến môi trường EXTERNAL_API_KEY.";
            return string.Empty;
        }

        diagnosticMessage = "Chưa cấu hình biến môi trường EXTERNAL_API_KEY. Hệ thống đang vận hành bằng Động cơ Phân tích Nội bộ (100% Offline).";
        return string.Empty;
    }

    private static string? GetEnvVarAcrossScopes(string variableName)
    {
        string? val = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);
        if (!string.IsNullOrWhiteSpace(val)) return val;

        try
        {
            val = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
            if (!string.IsNullOrWhiteSpace(val)) return val;

            val = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrWhiteSpace(val)) return val;
        }
        catch
        {
            // Bỏ qua nếu hệ điều hành hạn chế truy cập Registry scope User/Machine
        }

        return null;
    }

    public void UpdateConfiguration(AiConfiguration config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        // Nếu giao diện gửi chuỗi ApiKey rỗng (do Backend không trả khóa bí mật ra UI), giữ nguyên khóa Backend hiện có
        string effectiveKey = !IsPlaceholderKey(config.ApiKey)
            ? config.ApiKey.Trim()
            : _config.ApiKey;

        var provider = config.Provider;
        var detected = DetectProviderFromKeyFormat(effectiveKey);

        // Tự động đồng bộ nhà cung cấp nếu khóa hợp lệ thuộc về nhà cung cấp được hỗ trợ
        if (provider != AiProviderType.LocalEngine && detected != null && detected != provider)
        {
            provider = detected.Value;
        }

        string model = config.Model;
        if (provider == AiProviderType.Gemini && (string.IsNullOrWhiteSpace(model) || model.StartsWith("gpt", StringComparison.OrdinalIgnoreCase)))
        {
            model = "gemini-2.5-flash";
        }
        else if (provider == AiProviderType.OpenAi && (string.IsNullOrWhiteSpace(model) || model.StartsWith("gemini", StringComparison.OrdinalIgnoreCase)))
        {
            model = "gpt-4o-mini";
        }

        _ = IsKeyValidForProvider(effectiveKey, provider, out string diag);
        _lastDiagnosticStatus = diag;

        _config = new AiConfiguration
        {
            Provider = provider,
            ApiKey = detected != null ? effectiveKey : string.Empty,
            Model = model,
            IsEnabled = config.IsEnabled,
            CustomKnowledgeBase = config.CustomKnowledgeBase,
            KnowledgeBaseFilePath = config.KnowledgeBaseFilePath
        };
    }

    public AiConfiguration GetConfiguration()
    {
        // BẢO MẬT: Tuyệt đối không trả chuỗi ApiKey thật ra tầng giao diện HTML / Blazor Component
        return new AiConfiguration
        {
            Provider = _config.Provider,
            ApiKey = string.Empty,
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
        if (!IsKeyValidForProvider(_config.ApiKey, _config.Provider, out string diag))
        {
            _lastDiagnosticStatus = diag;
            return null;
        }

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
            Console.WriteLine($"[LlmClient] Lỗi kết nối dịch vụ {_config.Provider}: {SanitizeSensitiveText(ex.Message)}");
        }

        return null;
    }

    private async Task<string?> CallGeminiAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        string configuredModel = string.IsNullOrWhiteSpace(_config.Model) ? "gemini-2.5-flash" : _config.Model.Trim();
        // Nếu cấu hình cũ đặt gemini-3.8-flash (mã mô hình giả định), ưu tiên fallback sang gemini-2.5-flash / gemini-2.0-flash thực tế
        var candidateModels = new List<string> { configuredModel };
        if (!candidateModels.Contains("gemini-2.5-flash", StringComparer.OrdinalIgnoreCase))
            candidateModels.Add("gemini-2.5-flash");
        if (!candidateModels.Contains("gemini-2.0-flash", StringComparer.OrdinalIgnoreCase))
            candidateModels.Add("gemini-2.0-flash");

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

        foreach (string model in candidateModels)
        {
            var endpointUri = new Uri($"https://{VerifiedGeminiHost}/v1beta/models/{Uri.EscapeDataString(model)}:generateContent");
            if (!string.Equals(endpointUri.Host, VerifiedGeminiHost, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Endpoint không thuộc máy chủ Google Gemini đã xác minh.");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, endpointUri)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };
            // BẢO MẬT: Truyền khóa qua HTTP Header x-goog-api-key thay vì nối vào query string ?key=... trên URL
            request.Headers.Add("x-goog-api-key", _config.ApiKey.Trim());

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 404 && model != candidateModels.Last())
                {
                    continue; // Thử sang model chuẩn tiếp theo nếu tên model tùy chỉnh không tồn tại
                }

                _lastDiagnosticStatus = $"Google Gemini API trả về mã trạng thái {(int)response.StatusCode} ({response.StatusCode}). Vui lòng kiểm tra hiệu lực hoặc hạn mức của EXTERNAL_API_KEY.";
                Console.WriteLine($"[LlmClient] Gemini API trả về mã lỗi {(int)response.StatusCode} ({response.StatusCode}).");
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
                        _config.Model = model;
                        return textElement.GetString();
                    }
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
        string model = string.IsNullOrWhiteSpace(_config.Model) || _config.Model.StartsWith("gemini", StringComparison.OrdinalIgnoreCase)
            ? "gpt-4o-mini"
            : _config.Model.Trim();

        var endpointUri = new Uri($"https://{VerifiedOpenAiHost}/v1/chat/completions");
        if (!string.Equals(endpointUri.Host, VerifiedOpenAiHost, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Endpoint không thuộc máy chủ OpenAI đã xác minh.");
        }

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
        using var request = new HttpRequestMessage(HttpMethod.Post, endpointUri)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey.Trim());

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _lastDiagnosticStatus = $"OpenAI API trả về mã trạng thái {(int)response.StatusCode} ({response.StatusCode}). Vui lòng kiểm tra quyền hạn hoặc hạn mức (Quota/Billing) của EXTERNAL_API_KEY.";
            Console.WriteLine($"[LlmClient] OpenAI API trả về mã lỗi {(int)response.StatusCode} ({response.StatusCode}).");
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

    /// <summary>
    /// Che giấu tuyệt đối mọi chuỗi có dạng API Key trong log hoặc thông báo lỗi.
    /// </summary>
    public string SanitizeSensitiveText(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        string sanitized = input;
        if (!string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            sanitized = sanitized.Replace(_config.ApiKey, "[REDACTED_API_KEY]", StringComparison.OrdinalIgnoreCase);
        }

        sanitized = Regex.Replace(sanitized, @"AIza[0-9A-Za-z\-_]{20,}", "[REDACTED_GEMINI_KEY]");
        sanitized = Regex.Replace(sanitized, @"sk-[0-9A-Za-z\-_]{15,}", "[REDACTED_OPENAI_KEY]");
        return sanitized;
    }
}
