using System.Text;
using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.UseCases.AdminPortal;

public interface IExportTimesheetExcelUseCase
{
    byte[] ExportTimesheetToExcelHtml(int month, int year, IEnumerable<EmployeeTimesheetSummaryItem> items, string companyName = "CÔNG TY TNHH HỆ THỐNG QUẢN TRỊ DOANH NGHIỆP");
    byte[] ExportDailyTimesheetsToExcelHtml(int month, int year, IEnumerable<Timesheet> timesheets, string companyName = "CÔNG TY TNHH HỆ THỐNG QUẢN TRỊ DOANH NGHIỆP");
}

public class ExportTimesheetExcelUseCase : IExportTimesheetExcelUseCase
{
    public byte[] ExportTimesheetToExcelHtml(int month, int year, IEnumerable<EmployeeTimesheetSummaryItem> items, string companyName = "CÔNG TY TNHH HỆ THỐNG QUẢN TRỊ DOANH NGHIỆP")
    {
        var list = items.ToList();
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
        sb.AppendLine($"                    <x:Name>Bang_Cong_{month:D2}_{year}</x:Name>");
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
        sb.AppendLine("        .header-company { font-size: 11pt; font-weight: bold; text-align: left; color: #1e3a8a; }");
        sb.AppendLine("        .header-title { font-size: 16pt; font-weight: bold; text-align: center; color: #1e3a8a; padding: 10px 0; }");
        sb.AppendLine("        .header-sub { font-size: 11pt; text-align: center; font-style: italic; color: #555555; padding-bottom: 15px; }");
        sb.AppendLine("        th { background-color: #1e3a8a; color: #ffffff; font-weight: bold; border: 1px solid #212529; text-align: center; vertical-align: middle; padding: 8px 4px; font-size: 10pt; }");
        sb.AppendLine("        th.group-header { background-color: #1e40af; }");
        sb.AppendLine("        td { border: 1px solid #adb5bd; padding: 6px 4px; vertical-align: middle; font-size: 10pt; }");
        sb.AppendLine("        .text-center { text-align: center; }");
        sb.AppendLine("        .text-start { text-align: left; }");
        sb.AppendLine("        .text-end { text-align: right; }");
        sb.AppendLine("        .text-code { mso-number-format: '\\@'; text-align: center; font-weight: bold; }");
        sb.AppendLine("        .num-val { mso-number-format: '0\\.0'; text-align: center; }");
        sb.AppendLine("        .num-int { mso-number-format: '0'; text-align: center; }");
        sb.AppendLine("        .row-total { background-color: #eff6ff; font-weight: bold; border-top: 2px solid #1e3a8a; border-bottom: 2px solid #1e3a8a; }");
        sb.AppendLine("        .sign-title { font-weight: bold; text-align: center; font-size: 11pt; padding-top: 20px; }");
        sb.AppendLine("        .sign-note { font-style: italic; text-align: center; font-size: 9.5pt; color: #666666; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // HEADER
        sb.AppendLine("<table>");
        sb.AppendLine($"    <tr><td colspan=\"5\" class=\"header-company\" style=\"border:none;\">{companyName.ToUpper()}</td><td colspan=\"10\" style=\"border:none; text-align:right;\">Mẫu số: 01a-LĐTL</td></tr>");
        sb.AppendLine("    <tr><td colspan=\"5\" style=\"border:none; font-style:italic;\">Bộ phận: Phòng Hành Chính Nhân Sự & Quản Lý Ca</td><td colspan=\"10\" style=\"border:none;\"></td></tr>");
        sb.AppendLine($"    <tr><td colspan=\"15\" class=\"header-title\" style=\"border:none;\">BẢNG TỔNG HỢP CHẤM CÔNG VÀ THEO DÕI NGÀY CÔNG LAO ĐỘNG</td></tr>");
        sb.AppendLine($"    <tr><td colspan=\"15\" class=\"header-sub\" style=\"border:none;\">Kỳ chấm công: <strong>Tháng {month:D2}/{year}</strong> | Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm} | Tổng số nhân sự: {list.Count}</td></tr>");
        sb.AppendLine("</table>");

        // BẢNG
        sb.AppendLine("<table>");
        sb.AppendLine("    <thead>");
        sb.AppendLine("        <tr>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 40px;\">STT</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 90px;\">Mã NV</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 180px;\">Họ và Tên</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 150px;\">Phòng Ban / Bộ Phận</th>");
        sb.AppendLine("            <th colspan=\"4\" class=\"group-header\">CHI TIẾT CA LÀM VIỆC</th>");
        sb.AppendLine("            <th colspan=\"3\" class=\"group-header\" style=\"background-color:#d97706;\">KỶ LUẬT & NGHỈ PHÉP</th>");
        sb.AppendLine("            <th colspan=\"2\" class=\"group-header\" style=\"background-color:#059669;\">THỜI GIAN LÀM VIỆC</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 100px; background-color:#16a34a;\">TỔNG CÔNG (HƯỞNG LƯƠNG)</th>");
        sb.AppendLine("            <th rowspan=\"2\" style=\"width: 120px;\">GHI CHÚ / TRẠNG THÁI</th>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("        <tr>");
        sb.AppendLine("            <th style=\"width: 65px;\">Ca Chuẩn 8h</th>");
        sb.AppendLine("            <th style=\"width: 65px;\">Ca Gãy</th>");
        sb.AppendLine("            <th style=\"width: 65px;\">Nửa Ca</th>");
        sb.AppendLine("            <th style=\"width: 65px;\">Tổng Lượt</th>");
        sb.AppendLine("            <th style=\"width: 75px;\">Muộn/Sớm</th>");
        sb.AppendLine("            <th style=\"width: 75px;\">Nghỉ Phép</th>");
        sb.AppendLine("            <th style=\"width: 75px;\">K.Phép</th>");
        sb.AppendLine("            <th style=\"width: 80px;\">Tổng Giờ Làm</th>");
        sb.AppendLine("            <th style=\"width: 70px;\">Tăng Ca</th>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("    </thead>");
        sb.AppendLine("    <tbody>");

        int stt = 1;
        foreach (var item in list)
        {
            sb.AppendLine("        <tr>");
            sb.AppendLine($"            <td class=\"text-center\">{stt++}</td>");
            sb.AppendLine($"            <td class=\"text-code\">{item.EmployeeCode}</td>");
            sb.AppendLine($"            <td class=\"text-start\" style=\"font-weight:600;\">{item.FullName}</td>");
            sb.AppendLine($"            <td class=\"text-start\">{item.DepartmentName ?? "Chưa phân phòng"}</td>");
            sb.AppendLine($"            <td class=\"num-int\">{item.Standard8hCount}</td>");
            sb.AppendLine($"            <td class=\"num-int\">{item.SplitShiftCount}</td>");
            sb.AppendLine($"            <td class=\"num-int\">{item.HalfShiftCount}</td>");
            sb.AppendLine($"            <td class=\"num-int\">{item.FullShiftCount}</td>");
            sb.AppendLine($"            <td class=\"num-int\" style=\"{(item.LateOrEarlyCount > 0 ? "color:#b45309; font-weight:bold;" : "")}\">{item.LateOrEarlyCount}</td>");
            sb.AppendLine($"            <td class=\"num-val\">{item.PaidLeaveCount:0.0}</td>");
            sb.AppendLine($"            <td class=\"num-val\" style=\"{(item.AbsentCount > 0 ? "color:#dc2626; font-weight:bold;" : "")}\">{item.AbsentCount:0.0}</td>");
            sb.AppendLine($"            <td class=\"num-val\">{item.TotalHours:0.0} h</td>");
            sb.AppendLine($"            <td class=\"num-val\">{item.OvertimeHours:0.0} h</td>");
            sb.AppendLine($"            <td class=\"num-val\" style=\"font-weight:bold; color:#15803d; font-size:11pt; background-color:#f0fdf4;\">{item.TotalWorkUnits:0.0}</td>");
            sb.AppendLine($"            <td class=\"text-start\">{item.Status}</td>");
            sb.AppendLine("        </tr>");
        }

        // TỔNG CỘNG
        decimal sumStandard8h = list.Sum(i => i.Standard8hCount);
        decimal sumSplit = list.Sum(i => i.SplitShiftCount);
        decimal sumHalf = list.Sum(i => i.HalfShiftCount);
        decimal sumFull = list.Sum(i => i.FullShiftCount);
        decimal sumLate = list.Sum(i => i.LateOrEarlyCount);
        decimal sumLeave = list.Sum(i => i.PaidLeaveCount);
        decimal sumAbsent = list.Sum(i => i.AbsentCount);
        decimal sumHours = list.Sum(i => i.TotalHours);
        decimal sumOt = list.Sum(i => i.OvertimeHours);
        decimal sumUnits = list.Sum(i => i.TotalWorkUnits);

        sb.AppendLine("        <tr class=\"row-total\">");
        sb.AppendLine("            <td colspan=\"4\" class=\"text-center\">TỔNG CỘNG TOÀN ĐƠN VỊ:</td>");
        sb.AppendLine($"            <td class=\"num-int\">{sumStandard8h}</td>");
        sb.AppendLine($"            <td class=\"num-int\">{sumSplit}</td>");
        sb.AppendLine($"            <td class=\"num-int\">{sumHalf}</td>");
        sb.AppendLine($"            <td class=\"num-int\">{sumFull}</td>");
        sb.AppendLine($"            <td class=\"num-int\">{sumLate}</td>");
        sb.AppendLine($"            <td class=\"num-val\">{sumLeave:0.0}</td>");
        sb.AppendLine($"            <td class=\"num-val\">{sumAbsent:0.0}</td>");
        sb.AppendLine($"            <td class=\"num-val\">{sumHours:0.0} h</td>");
        sb.AppendLine($"            <td class=\"num-val\">{sumOt:0.0} h</td>");
        sb.AppendLine($"            <td class=\"num-val\" style=\"color:#15803d; font-size:11.5pt;\">{sumUnits:0.0} công</td>");
        sb.AppendLine("            <td></td>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("    </tbody>");
        sb.AppendLine("</table>");

        // CHỮ KÝ
        sb.AppendLine("<br/>");
        sb.AppendLine("<table>");
        sb.AppendLine("    <tr>");
        sb.AppendLine("        <td colspan=\"5\" class=\"sign-title\" style=\"border:none;\">NGƯỜI LẬP BIỂU</td>");
        sb.AppendLine("        <td colspan=\"5\" class=\"sign-title\" style=\"border:none;\">QUẢN LÝ BỘ PHẬN</td>");
        sb.AppendLine("        <td colspan=\"5\" class=\"sign-title\" style=\"border:none;\">BAN GIÁM ĐỐC DUYỆT</td>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("    <tr>");
        sb.AppendLine("        <td colspan=\"5\" class=\"sign-note\" style=\"border:none;\">(Ký, ghi rõ họ tên)</td>");
        sb.AppendLine("        <td colspan=\"5\" class=\"sign-note\" style=\"border:none;\">(Ký, ghi rõ họ tên)</td>");
        sb.AppendLine("        <td colspan=\"5\" class=\"sign-note\" style=\"border:none;\">(Ký, đóng dấu và ghi rõ họ tên)</td>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("    <tr style=\"height: 70px;\">");
        sb.AppendLine("        <td colspan=\"5\" style=\"border:none;\"></td>");
        sb.AppendLine("        <td colspan=\"5\" style=\"border:none;\"></td>");
        sb.AppendLine("        <td colspan=\"5\" style=\"border:none;\"></td>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("</table>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportDailyTimesheetsToExcelHtml(int month, int year, IEnumerable<Timesheet> timesheets, string companyName = "CÔNG TY TNHH HỆ THỐNG QUẢN TRỊ DOANH NGHIỆP")
    {
        var list = timesheets.OrderBy(t => t.WorkDate).ThenBy(t => t.EmployeeCode).ToList();
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns=\"http://www.w3.org/TR/REC-html40\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: 'Times New Roman', 'Arial', sans-serif; font-size: 10pt; }");
        sb.AppendLine("        table { border-collapse: collapse; width: 100%; }");
        sb.AppendLine("        th { background-color: #0f172a; color: #ffffff; border: 1px solid #333; padding: 6px; text-align: center; }");
        sb.AppendLine("        td { border: 1px solid #ccc; padding: 5px; text-align: center; }");
        sb.AppendLine("        .text-start { text-align: left; }");
        sb.AppendLine("        .text-code { mso-number-format: '\\@'; font-weight: bold; }");
        sb.AppendLine("        .time-val { mso-number-format: 'hh\\:mm\\:ss'; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h2>{companyName.ToUpper()} - SỔ NHẬT KÝ ĐIỂM DANH CHI TIẾT (THÁNG {month:D2}/{year})</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<thead><tr><th>STT</th><th>Mã NV</th><th>Họ và Tên</th><th>Ngày</th><th>Thứ</th><th>Ca Làm Việc</th><th>Giờ Vào</th><th>Giờ Ra</th><th>Số Giờ Làm</th><th>Ngày Công</th><th>Trạng Thái</th><th>Ghi Chú</th></tr></thead>");
        sb.AppendLine("<tbody>");
        int stt = 1;
        foreach (var t in list)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{stt++}</td>");
            sb.AppendLine($"<td class=\"text-code\">{t.EmployeeCode}</td>");
            sb.AppendLine($"<td class=\"text-start\">{t.EmployeeName}</td>");
            sb.AppendLine($"<td>{t.WorkDate:dd/MM/yyyy}</td>");
            sb.AppendLine($"<td>{t.WorkDate.ToString("dddd", new System.Globalization.CultureInfo("vi-VN"))}</td>");
            sb.AppendLine($"<td class=\"text-start\">{t.ShiftName ?? "Ca Hành Chính"}</td>");
            sb.AppendLine($"<td class=\"time-val\">{(t.CheckInTime.HasValue ? t.CheckInTime.Value.ToString(@"hh\:mm\:ss") : "-")}</td>");
            sb.AppendLine($"<td class=\"time-val\">{(t.CheckOutTime.HasValue ? t.CheckOutTime.Value.ToString(@"hh\:mm\:ss") : "-")}</td>");
            sb.AppendLine($"<td>{t.ActualHoursWorked:0.0}</td>");
            sb.AppendLine($"<td style=\"font-weight:bold;\">{t.WorkUnits:0.0}</td>");
            sb.AppendLine($"<td>{t.WorkShiftStatus}</td>");
            sb.AppendLine($"<td class=\"text-start\">{t.Note}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table>");
        sb.AppendLine("</body></html>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
