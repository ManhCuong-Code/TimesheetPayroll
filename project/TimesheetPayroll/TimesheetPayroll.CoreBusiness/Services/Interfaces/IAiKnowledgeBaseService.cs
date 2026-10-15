using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services.Interfaces;

/// <summary>
/// Dịch vụ quản lý và nạp dữ liệu tri thức (Knowledge Base) cho Trợ lý AI.
/// Cung cấp tri thức pháp luật lao động, chính sách công ty và ngữ cảnh đối soát thực tế.
/// </summary>
public interface IAiKnowledgeBaseService
{
    /// <summary>
    /// Tạo System Instruction / Prompt chứa toàn bộ tri thức pháp luật, quy chế công ty và ngữ cảnh phiếu lương thực tế.
    /// </summary>
    string BuildFullSystemPrompt(
        Employee? employee, 
        Payslip? current, 
        Payslip? previous, 
        SalaryComparisonDiff? diff = null);

    /// <summary>
    /// Danh sách các tài liệu tri thức đang được nạp vào AI.
    /// </summary>
    List<KnowledgeDocument> GetAllDocuments();

    /// <summary>
    /// Lấy ghi chú / quy chế bổ sung riêng của doanh nghiệp.
    /// </summary>
    string GetCustomNotes();

    /// <summary>
    /// Cập nhật ghi chú / quy chế bổ sung riêng của doanh nghiệp.
    /// </summary>
    void SetCustomNotes(string notes);
}
