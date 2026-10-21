using System.Text;
using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.UseCases.AdminPortal;

/// <summary>
/// Ca sử dụng: Xuất bảng tính lương hàng loạt ra định dạng Excel (.xls) và CSV ngân hàng (UC18 / UC_HR_11).
/// </summary>
public interface IExportPayrollExcelUseCase
{
    /// <summary>
    /// Xuất bảng thanh toán lương chi tiết ra định dạng Excel (.xls HTML Spreadsheet) với đầy đủ bảng biểu, màu sắc kế toán và định dạng tiền tệ.
    /// </summary>
    byte[] ExportToExcelHtml(PayrollPeriod period, IEnumerable<Payslip> payslips, string companyName = "KIM LONG ECOSTAY - CÔNG TY TNHH MTV");

    /// <summary>
    /// Xuất danh sách bảng lương ra định dạng CSV chuẩn UTF-8 kèm BOM tương thích ngân hàng và Excel.
    /// </summary>
    byte[] ExportToCsv(PayrollPeriod period, IEnumerable<Payslip> payslips);
}

public class ExportPayrollExcelUseCase : IExportPayrollExcelUseCase
{
    public byte[] ExportToExcelHtml(PayrollPeriod period, IEnumerable<Payslip> payslips, string companyName = "KIM LONG ECOSTAY - CÔNG TY TNHH MTV")
    {
        var list = payslips.ToList();
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns=\"http://www.w3.org/TR/REC-html40\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
        sb.AppendLine("    <!--[if gte mso 9]>");
        sb.AppendLine("    <xml>");
        sb.AppendLine("        <x:ExcelWorkbook>");
        sb.AppendLine("            <x:ExcelWorksheets>");
        sb.AppendLine("                <x:ExcelWorksheet>");
        sb.AppendLine($"                    <x:Name>Bang_Luong_{period.PeriodCode}</x:Name>");
        sb.AppendLine("                    <x:WorksheetOptions>");
        sb.AppendLine("                        <x:DisplayGridlines/>");
        sb.AppendLine("                    </x:WorksheetOptions>");
        sb.AppendLine("                </x:ExcelWorksheet>");
        sb.AppendLine("            </x:ExcelWorksheets>");
        sb.AppendLine("        </x:ExcelWorkbook>");
        sb.AppendLine("    </xml>");
        sb.AppendLine("    <![endif]-->");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: 'Times New Roman', 'Arial', sans-serif; font-size: 11pt; color: #333333; }");
        sb.AppendLine("        table { border-collapse: collapse; width: 100%; }");
        sb.AppendLine("        .header-company { font-size: 11pt; font-weight: bold; text-align: left; }");
        sb.AppendLine("        .header-title { font-size: 16pt; font-weight: bold; text-align: center; color: #0d6efd; padding: 10px 0; }");
        sb.AppendLine("        .header-sub { font-size: 11pt; text-align: center; font-style: italic; color: #555555; padding-bottom: 15px; }");
        sb.AppendLine("        th { background-color: #198754; color: #ffffff; font-weight: bold; border: 1px solid #212529; text-align: center; vertical-align: middle; padding: 8px 4px; font-size: 10pt; }");
        sb.AppendLine("        th.group-header { background-color: #0f5132; }");
        sb.AppendLine("        td { border: 1px solid #adb5bd; padding: 6px 4px; vertical-align: middle; font-size: 10pt; }");
        sb.AppendLine("        .text-center { text-align: center; }");
        sb.AppendLine("        .text-start { text-align: left; }");
        sb.AppendLine("        .text-end { text-align: right; }");
        sb.AppendLine("        .text-code { mso-number-format: '\\@'; text-align: center; font-weight: bold; }");
        sb.AppendLine("        .text-bank { mso-number-format: '\\@'; text-align: left; }");
        sb.AppendLine("        .num-vnd { mso-number-format: '\\#\\,\\#\\#0'; text-align: right; }");
        sb.AppendLine("        .num-days { mso-number-format: '0\\.0'; text-align: center; }");
        sb.AppendLine("        .row-total { background-color: #e8f5e9; font-weight: bold; border-top: 2px solid #198754; border-bottom: 2px solid #198754; }");
        sb.AppendLine("        .sign-title { font-weight: bold; text-align: center; font-size: 11pt; padding-top: 20px; }");
        sb.AppendLine("        .sign-note { font-style: italic; text-align: center; font-size: 9.5pt; color: #666666; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // PHẦN HEADER THÔNG TIN DOANH NGHIỆP & KỲ LƯƠNG
        sb.AppendLine("<table>");
        sb.AppendLine($"    <tr><td colspan=\"6\" class=\"header-company\" style=\"border:none;\">{companyName.ToUpper()}</td><td colspan=\"12\" style=\"border:none;\"></td></tr>");
        sb.AppendLine("    <tr><td colspan=\"6\" style=\"border:none; font-style:italic;\">Bộ phận: Phòng Kế Toán - Tiền Lương</td><td colspan=\"12\" style=\"border:none; text-align:right;\">Mẫu số: 02-LĐTL</td></tr>");
        sb.AppendLine($"    <tr><td colspan=\"18\" class=\"header-title\" style=\"border:none;\">BẢNG THANH TOÁN LƯƠNG NHÂN VIÊN DOANH NGHIỆP</td></tr>");
        sb.AppendLine($"    <tr><td colspan=\"18\" class=\"header-sub\" style=\"border:none;\">Kỳ lương: <strong>{period.PeriodCode}</strong> (Tháng {period.PeriodMonth}/{period.PeriodYear}) | Số ngày công chuẩn: <strong>{period.StandardWorkDays}</strong> ngày | Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>");
        sb.AppendLine("</table>");

        // BẢNG CHI TIẾT
        sb.AppendLine("<table>");
        sb.AppendLine("    <thead>");
        sb.AppendLine("        <tr>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 40px;\">STT</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 90px;\">Mã NV</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 180px;\">Họ và Tên</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 140px;\">Phòng Ban</th>");
        sb.AppendLine("            <th colspan=\"3\" class=\"group-header\">NGÀY CÔNG & THỜI GIAN</th>");
        sb.AppendLine("            <th colspan=\"4\" class=\"group-header\">CÁC KHOẢN THU NHẬP (VNĐ)</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 110px; background-color:#0b5ed7;\">TỔNG GROSS</th>");
        sb.AppendLine("            <th colspan=\"4\" class=\"group-header\" style=\"background-color:#bb2d3b;\">CÁC KHOẢN KHẤU TRỪ (VNĐ)</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 110px; background-color:#b02a37;\">TỔNG TRỪ</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 120px; background-color:#157347;\">THỰC LĨNH (NET)</th>");
        sb.AppendLine("            <th colspan=\"2\" class=\"group-header\" style=\"background-color:#495057;\">THÔNG TIN CHUYỂN KHOẢN</th>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("        <tr>");
        sb.AppendLine("            <th style=\"width: 70px;\">Công Đi Làm</th>");
        sb.AppendLine("            <th style=\"width: 70px;\">Nghỉ Phép</th>");
        sb.AppendLine("            <th style=\"width: 60px;\">Tăng Ca (h)</th>");
        sb.AppendLine("            <th style=\"width: 100px;\">Lương Cơ Bản</th>");
        sb.AppendLine("            <th style=\"width: 90px;\">PC Ca Gãy</th>");
        sb.AppendLine("            <th style=\"width: 90px;\">Tiền Làm Thêm</th>");
        sb.AppendLine("            <th style=\"width: 90px;\">Phụ Cấp & Thưởng</th>");
        sb.AppendLine("            <th style=\"width: 80px;\">BHXH (8%)</th>");
        sb.AppendLine("            <th style=\"width: 80px;\">BHYT (1.5%)</th>");
        sb.AppendLine("            <th style=\"width: 80px;\">BHTN (1%)</th>");
        sb.AppendLine("            <th style=\"width: 85px;\">Thuế TNCN</th>");
        sb.AppendLine("            <th style=\"width: 130px;\">Số Tài Khoản</th>");
        sb.AppendLine("            <th style=\"width: 120px;\">Ngân Hàng</th>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("    </thead>");
        sb.AppendLine("    <tbody>");

        int stt = 1;
        foreach (var p in list)
        {
            decimal baseSalary = p.Lines.Where(l => l.LineType == PayslipLineType.BASE_SALARY_PRORATED).Sum(l => l.Amount);
            decimal splitAllowance = p.Lines.Where(l => l.LineType == PayslipLineType.SPLIT_SHIFT_ALLOWANCE).Sum(l => l.Amount);
            decimal overtimePay = p.Lines.Where(l => l.LineType == PayslipLineType.OVERTIME_PAY).Sum(l => l.Amount);
            decimal otherEarnings = p.Lines.Where(l => !l.IsDeduction 
                && l.LineType != PayslipLineType.BASE_SALARY_PRORATED 
                && l.LineType != PayslipLineType.SPLIT_SHIFT_ALLOWANCE 
                && l.LineType != PayslipLineType.OVERTIME_PAY).Sum(l => l.Amount);

            decimal bhxh = p.Lines.Where(l => l.LineType == PayslipLineType.DEDUCTION_BHXH).Sum(l => l.Amount);
            decimal bhyt = p.Lines.Where(l => l.LineType == PayslipLineType.DEDUCTION_BHYT).Sum(l => l.Amount);
            decimal bhtn = p.Lines.Where(l => l.LineType == PayslipLineType.DEDUCTION_BHTN).Sum(l => l.Amount);
            decimal pit = p.Lines.Where(l => l.LineType == PayslipLineType.PERSONAL_INCOME_TAX).Sum(l => l.Amount);

            sb.AppendLine("        <tr>");
            sb.AppendLine($"            <td class=\"text-center\">{stt++}</td>");
            sb.AppendLine($"            <td class=\"text-code\">{p.EmployeeCode}</td>");
            sb.AppendLine($"            <td class=\"text-start\" style=\"font-weight:600;\">{p.EmployeeName}</td>");
            sb.AppendLine($"            <td class=\"text-start\">{p.DepartmentName ?? "Văn phòng"}</td>");
            sb.AppendLine($"            <td class=\"num-days\">{p.ActualWorkDays:0.0}</td>");
            sb.AppendLine($"            <td class=\"num-days\">{p.PaidLeaveDays:0.0}</td>");
            sb.AppendLine($"            <td class=\"num-days\">{p.OvertimeHours:0.0}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\">{baseSalary:N0}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\">{(splitAllowance > 0 ? splitAllowance.ToString("N0") : "-")}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\">{(overtimePay > 0 ? overtimePay.ToString("N0") : "-")}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\">{(otherEarnings > 0 ? otherEarnings.ToString("N0") : "-")}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\" style=\"font-weight:bold; color:#0b5ed7;\">{p.GrossSalary:N0}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\">{bhxh:N0}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\">{bhyt:N0}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\">{bhtn:N0}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\">{(pit > 0 ? pit.ToString("N0") : "-")}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\" style=\"font-weight:bold; color:#b02a37;\">-{p.TotalDeductions:N0}</td>");
            sb.AppendLine($"            <td class=\"num-vnd\" style=\"font-weight:bold; color:#157347; font-size:10.5pt;\">{p.NetSalary:N0}</td>");
            sb.AppendLine($"            <td class=\"text-bank\">{p.BankAccountNumber}</td>");
            sb.AppendLine($"            <td class=\"text-start\">{p.BankName}</td>");
            sb.AppendLine("        </tr>");
        }

        // DÒNG TỔNG CỘNG
        decimal sumGross = list.Sum(p => p.GrossSalary);
        decimal sumDeductions = list.Sum(p => p.TotalDeductions);
        decimal sumNet = list.Sum(p => p.NetSalary);
        decimal sumActualDays = list.Sum(p => p.ActualWorkDays);
        decimal sumLeaveDays = list.Sum(p => p.PaidLeaveDays);
        decimal sumOtHours = list.Sum(p => p.OvertimeHours);

        decimal sumBaseSalary = list.SelectMany(p => p.Lines).Where(l => l.LineType == PayslipLineType.BASE_SALARY_PRORATED).Sum(l => l.Amount);
        decimal sumSplit = list.SelectMany(p => p.Lines).Where(l => l.LineType == PayslipLineType.SPLIT_SHIFT_ALLOWANCE).Sum(l => l.Amount);
        decimal sumOvertime = list.SelectMany(p => p.Lines).Where(l => l.LineType == PayslipLineType.OVERTIME_PAY).Sum(l => l.Amount);
        decimal sumOtherEarnings = list.SelectMany(p => p.Lines).Where(l => !l.IsDeduction 
            && l.LineType != PayslipLineType.BASE_SALARY_PRORATED 
            && l.LineType != PayslipLineType.SPLIT_SHIFT_ALLOWANCE 
            && l.LineType != PayslipLineType.OVERTIME_PAY).Sum(l => l.Amount);

        decimal sumBhxh = list.SelectMany(p => p.Lines).Where(l => l.LineType == PayslipLineType.DEDUCTION_BHXH).Sum(l => l.Amount);
        decimal sumBhyt = list.SelectMany(p => p.Lines).Where(l => l.LineType == PayslipLineType.DEDUCTION_BHYT).Sum(l => l.Amount);
        decimal sumBhtn = list.SelectMany(p => p.Lines).Where(l => l.LineType == PayslipLineType.DEDUCTION_BHTN).Sum(l => l.Amount);
        decimal sumPit = list.SelectMany(p => p.Lines).Where(l => l.LineType == PayslipLineType.PERSONAL_INCOME_TAX).Sum(l => l.Amount);

        sb.AppendLine("        <tr class=\"row-total\">");
        sb.AppendLine("            <td colspan=\"4\" class=\"text-center\">TỔNG CỘNG:</td>");
        sb.AppendLine($"            <td class=\"num-days\">{sumActualDays:0.0}</td>");
        sb.AppendLine($"            <td class=\"num-days\">{sumLeaveDays:0.0}</td>");
        sb.AppendLine($"            <td class=\"num-days\">{sumOtHours:0.0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\">{sumBaseSalary:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\">{sumSplit:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\">{sumOvertime:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\">{sumOtherEarnings:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\" style=\"color:#0b5ed7;\">{sumGross:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\">{sumBhxh:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\">{sumBhyt:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\">{sumBhtn:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\">{sumPit:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\" style=\"color:#b02a37;\">-{sumDeductions:N0}</td>");
        sb.AppendLine($"            <td class=\"num-vnd\" style=\"color:#157347; font-size:11pt;\">{sumNet:N0}</td>");
        sb.AppendLine("            <td colspan=\"2\"></td>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("    </tbody>");
        sb.AppendLine("</table>");

        // CHỮ KÝ PHÊ DUYỆT BẢNG LƯƠNG
        sb.AppendLine("<br/>");
        sb.AppendLine("<table>");
        sb.AppendLine("    <tr>");
        sb.AppendLine("        <td colspan=\"4\" class=\"sign-title\" style=\"border:none;\">NGƯỜI LẬP BIỂU</td>");
        sb.AppendLine("        <td colspan=\"5\" class=\"sign-title\" style=\"border:none;\">KẾ TOÁN TRƯỞNG</td>");
        sb.AppendLine("        <td colspan=\"9\" class=\"sign-title\" style=\"border:none;\">GIÁM ĐỐC DUYỆT CHI</td>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("    <tr>");
        sb.AppendLine("        <td colspan=\"4\" class=\"sign-note\" style=\"border:none;\">(Ký, ghi rõ họ tên)</td>");
        sb.AppendLine("        <td colspan=\"5\" class=\"sign-note\" style=\"border:none;\">(Ký, ghi rõ họ tên)</td>");
        sb.AppendLine("        <td colspan=\"9\" class=\"sign-note\" style=\"border:none;\">(Ký, đóng dấu và ghi rõ họ tên)</td>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("    <tr style=\"height: 70px;\">");
        sb.AppendLine("        <td colspan=\"4\" style=\"border:none;\"></td>");
        sb.AppendLine("        <td colspan=\"5\" style=\"border:none;\"></td>");
        sb.AppendLine("        <td colspan=\"9\" style=\"border:none;\"></td>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("</table>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportToCsv(PayrollPeriod period, IEnumerable<Payslip> payslips)
    {
        var list = payslips.ToList();
        var sb = new StringBuilder();

        // Thêm UTF-8 BOM để Excel hiển thị tiếng Việt có dấu chuẩn 100%
        sb.Append('\uFEFF');

        // Header CSV
        sb.AppendLine("STT,Ma_NV,Ho_Va_Ten,Phong_Ban,Ngay_Cong,Luong_Gross,Khau_Tru,Thuc_Linh,So_Tai_Khoan,Ngan_Hang,Noi_Dung_Chuyen_Khoan");

        int stt = 1;
        foreach (var p in list)
        {
            string employeeName = EscapeCsv(p.EmployeeName ?? string.Empty);
            string department = EscapeCsv(p.DepartmentName ?? "Van phong");
            string bankAccount = EscapeCsv(p.BankAccountNumber ?? string.Empty);
            string bankName = EscapeCsv(p.BankName ?? string.Empty);
            string note = EscapeCsv($"Chi tra luong {period.PeriodCode} - {p.EmployeeCode}");

            sb.AppendLine($"{stt++},{p.EmployeeCode},{employeeName},{department},{p.ActualWorkDays:0.0},{p.GrossSalary:0},{p.TotalDeductions:0},{p.NetSalary:0},\"{bankAccount}\",{bankName},{note}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
