namespace TimesheetPayroll.CoreBusiness.Models;

public class FaceCameraInitResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class FaceCaptureResult
{
    public bool Success { get; set; }
    public float[]? Descriptor { get; set; }
    public double QualityScore { get; set; }
    public bool LivenessPassed { get; set; }
    public string? Error { get; set; }
}

public class FacePunchDbResult
{
    public bool Success { get; set; }
    public string ResultCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? PunchType { get; set; }
    public TimeSpan? PunchTime { get; set; }
    public string? WorkShiftStatus { get; set; }
    public decimal WorkUnits { get; set; }
    public decimal ActualHours { get; set; }
    public string? ShiftName { get; set; }
}

public class AttendanceStatusDto
{
    public long AssignmentId { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime WorkDate { get; set; }
    public long ShiftId { get; set; }
    public string ShiftCode { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsNightShift { get; set; }
    public bool IsOvernight { get; set; }
    public long? TimesheetId { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public decimal ActualHoursWorked { get; set; }
    public string? WorkShiftStatus { get; set; }
    public decimal WorkUnits { get; set; }
    public string? AttendanceSource { get; set; }
    public bool IsLocked { get; set; }
    public string? Note { get; set; }
    public string StatusCategory { get; set; } = "NOT_STARTED";
}

