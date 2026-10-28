using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services.Interfaces;

/// <summary>
/// Cổng dịch vụ gọi Mô hình Ngôn ngữ Lớn LLM (Google Gemini API / OpenAI API) phục vụ Trợ lý Lương AI.
/// Lưu ý: Dịch vụ này hoàn toàn độc lập với hệ thống nhận diện khuôn mặt sinh trắc học 128-D (chạy 100% nội bộ).
/// </summary>
public interface ILlmClient
{
    /// <summary>
    /// Cho biết dịch vụ đã có API Key hợp lệ (đúng định dạng nhà cung cấp) và sẵn sàng kết nối trực tuyến hay chưa.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Tên nhà cung cấp mô hình đang kích hoạt (Google Gemini, OpenAI, hoặc Động cơ Phân tích Nội bộ).
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Tên mô hình đang kích hoạt (vd: gemini-2.5-flash, gpt-4o-mini).
    /// </summary>
    string ActiveModel { get; }

    /// <summary>
    /// Thông báo chẩn đoán trạng thái cấu hình API Key an toàn (tuyệt đối không chứa giá trị khóa).
    /// </summary>
    string GetKeyDiagnosticStatus();

    /// <summary>
    /// Gửi câu hỏi và ngữ cảnh dữ liệu tri thức tới LLM đã xác minh để sinh câu trả lời tự nhiên.
    /// </summary>
    Task<string?> GenerateResponseAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật cấu hình Model, Provider hoặc API Key tại runtime.
    /// </summary>
    void UpdateConfiguration(AiConfiguration config);

    /// <summary>
    /// Lấy bản sao cấu hình AI an toàn cho giao diện (không bao giờ trả về chuỗi API Key bí mật ra Frontend/HTML).
    /// </summary>
    AiConfiguration GetConfiguration();
}
