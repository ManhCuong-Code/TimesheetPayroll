using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services.Interfaces;

/// <summary>
/// Cổng dịch vụ gọi Mô hình Ngôn ngữ Lớn LLM (Google Gemini API / OpenAI API).
/// </summary>
public interface ILlmClient
{
    /// <summary>
    /// Cho biết dịch vụ đã có API Key hợp lệ và sẵn sàng kết nối trực tuyến hay chưa.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Tên nhà cung cấp mô hình (vd: Google Gemini, OpenAI, Động cơ Nội bộ).
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Tên mô hình đang kích hoạt (vd: gemini-2.0-flash, gpt-4o-mini).
    /// </summary>
    string ActiveModel { get; }

    /// <summary>
    /// Gửi câu hỏi và ngữ cảnh dữ liệu tri thức tới LLM để sinh câu trả lời tự nhiên.
    /// </summary>
    Task<string?> GenerateResponseAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật cấu hình API Key, Model hoặc Provider tại runtime.
    /// </summary>
    void UpdateConfiguration(AiConfiguration config);

    /// <summary>
    /// Lấy cấu hình AI hiện tại.
    /// </summary>
    AiConfiguration GetConfiguration();
}
