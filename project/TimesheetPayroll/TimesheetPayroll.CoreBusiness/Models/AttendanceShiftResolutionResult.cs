namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Kết quả tự động phân giải và nhận diện ca làm việc cho nhân viên.
/// </summary>
public class AttendanceShiftResolutionResult
{
    public string ResolutionStatus { get; set; } = "RESOLVED"; // RESOLVED, AMBIGUOUS, NO_ASSIGNED_SHIFT, OUTSIDE_WINDOW, SHIFT_COMPLETED, ERROR
    public bool IsSuccess => ResolutionStatus == "RESOLVED";
    public string Message { get; set; } = string.Empty;

    public long? ShiftId { get; set; }
    public string? ShiftCode { get; set; }
    public string? ShiftName { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Today;
    public string? PunchAction { get; set; } // CHECK_IN, CHECK_OUT
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsOvernight { get; set; }
    public int CandidateCount { get; set; }

    public static AttendanceShiftResolutionResult NoShift(string message = "Bạn chưa được phân công ca làm việc phù hợp.") =>
        new() { ResolutionStatus = "NO_ASSIGNED_SHIFT", Message = message, CandidateCount = 0 };

    public static AttendanceShiftResolutionResult Ambiguous(string message = "Không xác định được ca làm việc duy nhất. Vui lòng liên hệ quản lý.", int count = 2) =>
        new() { ResolutionStatus = "AMBIGUOUS", Message = message, CandidateCount = count };

    public static AttendanceShiftResolutionResult OutsideWindow(string message = "Thời điểm chấm công nằm ngoài khung giờ cho phép.") =>
        new() { ResolutionStatus = "OUTSIDE_WINDOW", Message = message, CandidateCount = 0 };

    public static AttendanceShiftResolutionResult ShiftCompleted(string message = "Bạn đã hoàn thành các lượt chấm công cho ca được phân công.") =>
        new() { ResolutionStatus = "SHIFT_COMPLETED", Message = message, CandidateCount = 0 };
}
