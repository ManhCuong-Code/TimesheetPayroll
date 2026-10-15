using System.Globalization;
using System.Text;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;

namespace TimesheetPayroll.CoreBusiness.Services;

/// <summary>
/// Triển khai dịch vụ quản lý và nạp dữ liệu tri thức (Knowledge Base) cho AI Assistant.
/// </summary>
public class AiKnowledgeBaseService : IAiKnowledgeBaseService
{
    private static readonly CultureInfo VnCulture = new("vi-VN");
    private string _customNotes = string.Empty;

    public AiKnowledgeBaseService()
    {
        _customNotes = "Chính sách chuyên cần: Nhân viên đi làm đủ 100% ngày công chuẩn trong tháng được thưởng thêm 300.000 VNĐ tiền chuyên cần.";
    }

    public string GetCustomNotes() => _customNotes;

    public void SetCustomNotes(string notes)
    {
        _customNotes = notes ?? string.Empty;
    }

    public List<KnowledgeDocument> GetAllDocuments()
    {
        return new List<KnowledgeDocument>
        {
            new()
            {
                Title = "Luật Bảo hiểm & Mức trần đóng BHXH, BHYT, BHTN",
                Category = "Pháp luật Lao động & Bảo hiểm",
                Source = "Nghị định 73/2024/NĐ-CP & Nghị định 74/2024/NĐ-CP",
                Content = $"Lương cơ sở mới nhất là {VietnameseLaborRegulations.BaseStatutorySalary:N0} đ/tháng. " +
                          $"Mức trần đóng BHXH và BHYT (tối đa 20 lần lương cơ sở) là {VietnameseLaborRegulations.MaxSalaryCapBhxhBhyt:N0} đ/tháng. " +
                          $"Lương tối thiểu Vùng I là {VietnameseLaborRegulations.RegionalMinimumSalaryRegion1:N0} đ/tháng. " +
                          $"Mức trần đóng BHTN (tối đa 20 lần lương tối thiểu vùng I) là {VietnameseLaborRegulations.MaxSalaryCapBhtn:N0} đ/tháng. " +
                          $"Tỷ lệ người lao động đóng là 10.5% (BHXH 8%, BHYT 1.5%, BHTN 1%). Doanh nghiệp đóng 21.5%."
            },
            new()
            {
                Title = "Thuế Thu Nhập Cá Nhân & Các Khoản Giảm Trừ",
                Category = "Thuế & Tài chính",
                Source = "Nghị quyết 954/2020/UBTVQH14 & Thông tư 111/2013/TT-BTC",
                Content = $"Mức giảm trừ gia cảnh cho bản thân: {VietnameseLaborRegulations.PersonalTaxDeduction:N0} đ/tháng. " +
                          $"Mức giảm trừ cho mỗi người phụ thuộc: {VietnameseLaborRegulations.DependentTaxDeduction:N0} đ/người/tháng. " +
                          $"Mức phụ cấp ăn trưa tối đa được miễn thuế TNCN: {VietnameseLaborRegulations.MaxNonTaxableMealAllowance:N0} đ/tháng. " +
                          $"Phần tiền lương làm thêm giờ (OT) trả cao hơn so với mức làm ngày bình thường được miễn thuế TNCN."
            },
            new()
            {
                Title = "Quy định Làm Thêm Giờ (OT) & Làm Đêm",
                Category = "Bộ luật Lao động",
                Source = "Điều 98 Bộ luật Lao động 2019",
                Content = "Tiền làm thêm giờ: Ngày thường ít nhất 150%; Ngày nghỉ hàng tuần (T7, CN) ít nhất 200%; Ngày lễ tết ít nhất 300%. " +
                          "Làm việc vào ban đêm (22h - 06h) được trả thêm ít nhất 30% tiền lương tính theo đơn giá tiền lương giờ. " +
                          "Làm thêm giờ ban đêm được trả thêm ít nhất 20% tiền lương tính theo công việc làm thêm vào ban ngày."
            },
            new()
            {
                Title = "Chế độ Nghỉ Phép & Nghỉ Lễ Tết Có Lương",
                Category = "Bộ luật Lao động",
                Source = "Điều 112, 113, 115 Bộ luật Lao động 2019",
                Content = $"Người lao động có {VietnameseLaborRegulations.BasicAnnualLeaveDaysPerYear} ngày phép năm có hưởng nguyên lương. " +
                          $"Cứ đủ {VietnameseLaborRegulations.SeniorityYearsPerBonusLeaveDay} năm làm việc được tăng thêm 1 ngày phép. " +
                          $"Tổng số ngày nghỉ Lễ, Tết hưởng nguyên lương trong năm là {VietnameseLaborRegulations.TotalPaidPublicHolidays} ngày. " +
                          $"Nghỉ kết hôn: {VietnameseLaborRegulations.MarriageLeaveDays} ngày; con kết hôn: {VietnameseLaborRegulations.ChildMarriageLeaveDays} ngày; tứ thân phụ mẫu/vợ chồng/con chết: {VietnameseLaborRegulations.BereavementLeaveDays} ngày hưởng nguyên lương."
            },
            new()
            {
                Title = "Quy tắc 14 Ngày Nghỉ Không Hưởng Lương",
                Category = "Bảo hiểm Xã hội",
                Source = "Điều 85 Luật BHXH 2014 & Quyết định 595/QĐ-BHXH",
                Content = "Nếu người lao động không làm việc và không hưởng tiền lương từ 14 ngày làm việc trở lên trong tháng thì tháng đó cả người lao động và người sử dụng lao động đều không phải đóng BHXH, BHYT, BHTN."
            },
            new()
            {
                Title = "Quy chế Nội bộ Doanh nghiệp & Ghi chú Bổ sung",
                Category = "Quy chế Nội bộ",
                Source = "Tài liệu Doanh nghiệp",
                Content = string.IsNullOrWhiteSpace(_customNotes) ? "Chưa có ghi chú bổ sung." : _customNotes
            }
        };
    }

    public string BuildFullSystemPrompt(
        Employee? employee, 
        Payslip? current, 
        Payslip? previous, 
        SalaryComparisonDiff? diff = null)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== VAI TRÒ VÀ NGUYÊN TẮC HOẠT ĐỘNG ===");
        sb.AppendLine("Bạn là Trợ lý Lương AI thông minh (HRM Salary AI Assistant) thuộc Hệ thống Chấm công & Tính lương doanh nghiệp Timesheet & Payroll.");
        sb.AppendLine("Nhiệm vụ của bạn là hỗ trợ người lao động giải đáp thắc mắc về tiền lương, phiếu lương, lý do biến động lương, quy chế làm thêm giờ (OT), bảo hiểm xã hội và thuế thu nhập cá nhân (TNCN).");
        sb.AppendLine();
        sb.AppendLine("NGUYÊN TẮC BẮT BUỘC (CRITICAL RULES):");
        sb.AppendLine("1. CHỐNG ẢO GIÁC (ZERO HALLUCINATION): Toàn bộ số liệu về ngày công, lương cơ bản, Gross, bảo hiểm, thuế, Net và các khoản chênh lệch PHẢI lấy chính xác 100% từ dữ liệu thực tế bên dưới. Không được tự bịa số liệu tài chính.");
        sb.AppendLine("2. PHONG CÁCH GIAO TIẾP: Lịch sự, thân thiện, rõ ràng, đồng cảm và chuyên nghiệp. Xưng 'Em' và gọi người lao động bằng 'Anh/Chị' kèm theo tên của họ.");
        sb.AppendLine("3. ĐỊNH DẠNG: Sử dụng Markdown (in đậm số tiền có đơn vị 'đ' hoặc 'VNĐ', dùng danh sách gạch đầu dòng rõ ràng để dễ đọc).");
        sb.AppendLine("4. CĂN CỨ PHÁP LÝ: Khi giải thích về bảo hiểm, thuế, ngày phép, làm thêm giờ, hãy viện dẫn điều luật hoặc nghị định tương ứng có trong phần tri thức.");
        sb.AppendLine();

        sb.AppendLine("=== TÀI LIỆU TRI THỨC PHÁP LUẬT & QUY CHẾ ĐÃ NẠP (KNOWLEDGE BASE) ===");
        foreach (var doc in GetAllDocuments())
        {
            sb.AppendLine($"[TÀI LIỆU: {doc.Title} | Nguồn: {doc.Source}]");
            sb.AppendLine(doc.Content);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(_customNotes))
        {
            sb.AppendLine("[QUY CHẾ NỘI BỘ BỔ SUNG CỦA DOANH NGHIỆP]");
            sb.AppendLine(_customNotes);
            sb.AppendLine();
        }

        sb.AppendLine("=== DỮ LIỆU NHÂN SỰ CỦA NGƯỜI DÙNG ĐANG TRÒ CHUYỆN (USER CONTEXT) ===");
        if (employee != null)
        {
            sb.AppendLine($"• Họ và tên: {employee.FullName}");
            sb.AppendLine($"• Mã nhân viên: {employee.EmployeeCode}");
            sb.AppendLine($"• Phòng ban: {employee.DepartmentName ?? "Nhân sự"}");
            sb.AppendLine($"• Lương cơ bản theo hợp đồng: {employee.BaseSalary.ToString("N0", VnCulture)} VNĐ/tháng");
            sb.AppendLine($"• Phụ cấp ăn trưa: {employee.MealAllowance.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine($"• Phụ cấp xăng xe/khác: {employee.FuelAllowance.ToString("N0", VnCulture)} VNĐ");
        }
        else
        {
            sb.AppendLine("• Thông tin nhân sự: Chưa liên kết hồ sơ.");
        }
        sb.AppendLine();

        if (current != null)
        {
            sb.AppendLine($"=== CHI TIẾT PHIẾU LƯƠNG KỲ HIỆN TẠI (Tháng {current.PeriodMonth}/{current.PeriodYear}) ===");
            sb.AppendLine($"• Số ngày công thực tế: {current.ActualWorkDays} công");
            sb.AppendLine($"• Tổng thu nhập gộp (Gross): {current.GrossSalary.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine($"• Tổng khấu trừ: {current.TotalDeductions.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine($"• THỰC LĨNH CHUYỂN KHOẢN (NET): {current.NetSalary.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine($"• Số tài khoản ngân hàng: {current.BankAccountNumber} ({current.BankName})");
            sb.AppendLine("• Các dòng chi tiết cấu thành:");
            if (current.Lines != null && current.Lines.Any())
            {
                foreach (var line in current.Lines)
                {
                    string sign = line.IsDeduction ? "[-] Khấu trừ" : "[+] Thu nhập";
                    sb.AppendLine($"   - {sign} {line.LineDescription}: {line.Amount.ToString("N0", VnCulture)} VNĐ");
                }
            }
            sb.AppendLine();
        }

        if (previous != null)
        {
            sb.AppendLine($"=== CHI TIẾT PHIẾU LƯƠNG KỲ TRƯỚC (Tháng {previous.PeriodMonth}/{previous.PeriodYear}) ===");
            sb.AppendLine($"• Số ngày công thực tế: {previous.ActualWorkDays} công");
            sb.AppendLine($"• Tổng thu nhập gộp (Gross): {previous.GrossSalary.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine($"• Tổng khấu trừ: {previous.TotalDeductions.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine($"• THỰC LĨNH CHUYỂN KHOẢN (NET): {previous.NetSalary.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine();
        }

        if (diff != null && previous != null && current != null)
        {
            sb.AppendLine("=== KẾT QUẢ ĐỐI SOÁT CHÊNH LỆCH HAI KỲ LƯƠNG (SYSTEM DETERMINISTIC DIFF) ===");
            sb.AppendLine($"• Chênh lệch ngày công: {(diff.WorkDaysDiff >= 0 ? "+" : "")}{diff.WorkDaysDiff} công");
            sb.AppendLine($"• Chênh lệch Gross: {(diff.GrossDiff >= 0 ? "+" : "")}{diff.GrossDiff.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine($"• Chênh lệch khấu trừ: {(diff.DeductionDiff >= 0 ? "+" : "")}{diff.DeductionDiff.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine($"• Chênh lệch THỰC LĨNH NET: {(diff.NetDiff >= 0 ? "+" : "")}{diff.NetDiff.ToString("N0", VnCulture)} VNĐ");
            sb.AppendLine("• Các nguyên nhân chính:");
            foreach (var f in diff.KeyFactors)
            {
                sb.AppendLine($"   - {f.Title}: {f.Description}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
