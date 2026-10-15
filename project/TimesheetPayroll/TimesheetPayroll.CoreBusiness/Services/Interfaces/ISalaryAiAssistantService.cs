using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services.Interfaces;

/// <summary>
/// Cổng dịch vụ Trợ lý Trí tuệ Nhân tạo hỗ trợ tra cứu, giải thích và so sánh lương người lao động (UC19).
/// </summary>
public interface ISalaryAiAssistantService
{
    /// <summary>
    /// Đối soát và phân tích sự chênh lệch chi tiết giữa hai phiếu lương của người lao động.
    /// </summary>
    SalaryComparisonDiff ComparePayslips(Payslip currentPeriod, Payslip previousPeriod, Employee? employee = null);

    /// <summary>
    /// Tiếp nhận câu hỏi tự nhiên của nhân viên, đối soát dữ liệu và sinh lời giải đáp chính xác, tự nhiên.
    /// </summary>
    Task<AiChatMessage> AnswerQuestionAsync(
        string question, 
        Payslip? current, 
        Payslip? previous, 
        Employee? employee, 
        IEnumerable<PayrollPeriod>? allPeriods = null);

    /// <summary>
    /// Mô phỏng dự tính lương thực lĩnh dựa trên các tham số giả định của người dùng (What-If Salary Simulator).
    /// </summary>
    SalarySimulationResult SimulateSalary(SalarySimulationRequest request, IEnumerable<DeductionRate>? rates = null);
}
