namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho một Ca làm việc trong doanh nghiệp (Work Shift).
/// Hỗ trợ quản lý ca hành chính, ca sáng, ca chiều, ca tối, ca đêm với các quy định giờ vào/ra,
/// thời gian nghỉ giữa ca, số công tiêu chuẩn và thời gian cho phép trễ.
/// </summary>
public class WorkShift
{
    public long Id { get; set; }

    /// <summary>
    /// Mã định danh ca làm việc (ví dụ: CA_HANH_CHINH, CA_SANG, CA_CHIEU, CA_TOI, CA_DEM).
    /// </summary>
    public string ShiftCode { get; set; } = string.Empty;

    /// <summary>
    /// Tên gọi hiển thị của ca (ví dụ: Ca Hành Chính (08:00 - 17:30)).
    /// </summary>
    public string ShiftName { get; set; } = string.Empty;

    /// <summary>
    /// Giờ bắt đầu ca làm việc (chuẩn).
    /// </summary>
    public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);

    /// <summary>
    /// Giờ kết thúc ca làm việc (chuẩn).
    /// </summary>
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 30, 0);

    /// <summary>
    /// Số giờ nghỉ trưa / nghỉ giữa ca (ví dụ: 1.5 giờ từ 12:00 đến 13:30).
    /// </summary>
    public decimal LunchBreakHours { get; set; } = 1.5m;

    /// <summary>
    /// Số giờ làm việc mục tiêu trong ca (ví dụ: 8.0 giờ cho ca đầy đủ, 4.0 giờ cho ca nửa ngày).
    /// </summary>
    public decimal TargetHours { get; set; } = 8.0m;

    /// <summary>
    /// Đơn vị ngày công chuẩn khi hoàn thành đủ ca (1.0 công hoặc 0.5 công).
    /// </summary>
    public decimal WorkUnits { get; set; } = 1.0m;

    /// <summary>
    /// Số phút cho phép đi muộn / về sớm linh hoạt mà không bị phạt (ví dụ: 15 phút).
    /// </summary>
    public int GracePeriodMinutes { get; set; } = 15;

    /// <summary>
    /// Đánh dấu là ca đêm (từ 22:00 đến 06:00 sáng hôm sau, được hưởng phụ cấp ca đêm 30% theo Bộ luật Lao động).
    /// </summary>
    public bool IsNightShift { get; set; } = false;

    /// <summary>
    /// Trạng thái hoạt động của ca.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Mô tả chi tiết hoặc ghi chú về ca làm việc.
    /// </summary>
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Hiển thị tóm tắt khung giờ ca làm việc.
    /// </summary>
    public string TimeRangeDisplay => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm} ({TargetHours:0.#}h/ca • {WorkUnits:0.#} công)";
}
