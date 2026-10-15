namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho kết quả đối soát và phân tích chênh lệch giữa hai phiếu lương (Salary Comparison Diff).
/// </summary>
public class SalaryComparisonDiff
{
    public Payslip PeriodA { get; set; } = null!;
    public Payslip PeriodB { get; set; } = null!;

    public decimal WorkDaysDiff => PeriodA.ActualWorkDays - PeriodB.ActualWorkDays;
    public decimal GrossDiff => PeriodA.GrossSalary - PeriodB.GrossSalary;
    public decimal DeductionDiff => PeriodA.TotalDeductions - PeriodB.TotalDeductions;
    public decimal NetDiff => PeriodA.NetSalary - PeriodB.NetSalary;

    public List<SalaryLineDiffItem> LineDiffs { get; set; } = new();
    public List<SalaryChangeFactor> KeyFactors { get; set; } = new();
    public string AiSummary { get; set; } = string.Empty;
}

/// <summary>
/// Chi tiết chênh lệch từng khoản mục lương giữa 2 kỳ.
/// </summary>
public class SalaryLineDiffItem
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Thu nhập"; // "Thu nhập" hoặc "Khấu trừ"
    public decimal AmountA { get; set; }
    public decimal AmountB { get; set; }
    public decimal Difference => AmountA - AmountB;
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Nhân tố chính gây nên sự biến động lương giữa hai kỳ.
/// </summary>
public class SalaryChangeFactor
{
    public string Icon { get; set; } = "fa-circle-info";
    public string BadgeClass { get; set; } = "bg-primary";
    public string Title { get; set; } = string.Empty;
    public decimal ImpactAmount { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Yêu cầu mô phỏng tính toán lương dự kiến (What-If Salary Simulation).
/// </summary>
public class SalarySimulationRequest
{
    public decimal BaseSalary { get; set; } = 10000000m;
    public decimal StandardWorkDays { get; set; } = 22m;
    public decimal ActualWorkDays { get; set; } = 22m;
    public decimal PaidLeaveDays { get; set; } = 0m;
    public decimal OvertimeHours { get; set; } = 0m;
    public decimal OvertimeMultiplier { get; set; } = 1.5m; // 1.5 ngày thường, 2.0 cuối tuần, 3.0 ngày lễ
    public decimal MealAllowance { get; set; } = 500000m;
    public decimal FuelAllowance { get; set; } = 0m;
    public decimal Bonus { get; set; } = 0m;
    public int NumberOfDependents { get; set; } = 0;
}

/// <summary>
/// Kết quả chi tiết tính toán mô phỏng lương What-If.
/// </summary>
public class SalarySimulationResult
{
    public decimal BaseSalary { get; set; }
    public decimal StandardWorkDays { get; set; }
    public decimal ActualWorkDays { get; set; }
    public decimal ProratedBaseSalary { get; set; }
    public decimal PaidLeavePay { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal Allowances { get; set; }
    public decimal Bonus { get; set; }
    public decimal GrossSalary { get; set; }

    public decimal Bhxh { get; set; } // 8%
    public decimal Bhyt { get; set; } // 1.5%
    public decimal Bhtn { get; set; } // 1.0%
    public decimal TotalInsurance { get; set; } // 10.5%

    public decimal PersonalDeduction { get; set; } = 15000000m;
    public decimal DependentDeduction { get; set; }
    public decimal TaxableIncome { get; set; } // Gross - phụ cấp ăn trưa không chịu thuế
    public decimal AssessableIncome { get; set; } // Thu nhập tính thuế = Taxable - Insurance - Bản thân - Người phụ thuộc
    public decimal PersonalIncomeTax { get; set; }

    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    public string AiAnalysis { get; set; } = string.Empty;
}

/// <summary>
/// Tin nhắn trao đổi giữa Người dùng và Trợ lý Lương AI.
/// </summary>
public class AiChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Text { get; set; } = string.Empty;
    public SalaryComparisonDiff? ComparisonDiff { get; set; }
    public SalarySimulationResult? SimulationResult { get; set; }
    public List<string> FollowUpSuggestions { get; set; } = new();
    public string? ProviderUsed { get; set; } // "Google Gemini", "OpenAI", "Động cơ Phân tích Nội bộ"
    public bool IsLlmGenerated { get; set; }
}

/// <summary>
/// Nhà cung cấp mô hình trí tuệ nhân tạo (AI Provider).
/// </summary>
public enum AiProviderType
{
    Gemini = 1,
    OpenAi = 2,
    LocalEngine = 3
}

/// <summary>
/// Cấu hình kết nối và nạp tri thức cho AI Assistant.
/// </summary>
public class AiConfiguration
{
    public AiProviderType Provider { get; set; } = AiProviderType.Gemini;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.0-flash";
    public bool IsEnabled { get; set; } = true;
    public string? CustomKnowledgeBase { get; set; } = string.Empty;
    public string? KnowledgeBaseFilePath { get; set; } = "App_Data/knowledge_base_hrm.md";
}

/// <summary>
/// Thông tin tài liệu tri thức nạp cho AI.
/// </summary>
public class KnowledgeDocument
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

