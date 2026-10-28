namespace TimesheetPayroll.CoreBusiness.Models;

public class FaceCameraInitResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool ReusedStream { get; set; }
    public double CameraInitMs { get; set; }
}

public class FaceCaptureResult
{
    public bool Success { get; set; }
    public float[]? Descriptor { get; set; }
    public double LivenessScore { get; set; } = 0.90;
    public bool QualityOk { get; set; }
    public double QualityScore { get; set; }
    public bool LivenessPassed { get; set; }
    public string? PhotoBase64 { get; set; }
    public string? Error { get; set; }
    public double FaceDetectionMs { get; set; }
    public double FaceAlignmentMs { get; set; }
    public double PreprocessingMs { get; set; }
    public double EmbeddingInferenceMs { get; set; }
    public int ExtractDurationMs { get; set; }
}

public class FaceDbSaveResult
{
    public bool EmployeeExists { get; set; }
    public bool WasAlreadyEnrolled { get; set; }
    public bool SkippedIdenticalWrite { get; set; }
    public double SqlQueryMs { get; set; }
    public double DbWriteMs { get; set; }
}

public class FaceEnrollmentTelemetryDto
{
    public long EmployeeId { get; set; }
    public double CameraInitMs { get; set; }
    public bool ReusedCameraStream { get; set; }
    public double FaceDetectionMs { get; set; }
    public double FaceAlignmentMs { get; set; }
    public double PreprocessingMs { get; set; }
    public double EmbeddingInferenceMs { get; set; }
    public double EmbeddingValidationMs { get; set; }
    public double SqlQueryMs { get; set; }
    public double DbWriteMs { get; set; }
    public double TotalEnrollmentMs { get; set; }
    public bool WasAlreadyEnrolled { get; set; }
    public bool SkippedIdenticalWrite { get; set; }
    public bool ReusedExistingEmbedding { get; set; }
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
