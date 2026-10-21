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
