using TimesheetPayroll.CoreBusiness.Exceptions;

namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho bản ghi chấm công hàng ngày của nhân viên.
/// Ghi nhận giờ Check-In, Check-Out, đánh giá trạng thái ca làm việc và tính hệ số ngày công.
/// </summary>
public class Timesheet
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public long? PeriodId { get; set; }
    public long? ShiftId { get; set; }
    public string? ShiftCode { get; set; }
    public string? ShiftName { get; set; }
    public DateTime WorkDate { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public decimal ActualHoursWorked { get; set; } = 8.0m;
    public decimal ActualHours => ActualHoursWorked;
    public decimal OvertimeHours => Math.Max(0m, ActualHoursWorked - 8.0m);
    public WorkShiftStatus WorkShiftStatus { get; set; } = WorkShiftStatus.PRESENT;
    public bool IsLate => WorkShiftStatus == WorkShiftStatus.LATE;
    public bool IsEarlyLeave => WorkShiftStatus == WorkShiftStatus.EARLY_LEAVE;
    public decimal WorkUnits { get; set; } = 1.0m;
    public bool IsLocked { get; set; } = false;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Khóa bản ghi chấm công chống chỉnh sửa.
    /// </summary>
    public void LockTimesheet()
    {
        IsLocked = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mở khóa bản ghi chấm công (chỉ khi kỳ lương chưa bị khóa sổ).
    /// </summary>
    public void UnlockTimesheet()
    {
        IsLocked = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Tính toán số giờ làm việc thực tế dựa trên giờ vào, giờ ra và thời gian nghỉ giữa ca.
    /// Tự động xử lý cả ca làm việc xuyên đêm (qua 24h).
    /// </summary>
    public decimal CalculateHoursWorked(decimal lunchBreakHours = 1.5m, bool isCrossMidnight = false)
    {
        if (CheckInTime.HasValue && CheckOutTime.HasValue)
        {
            decimal rawHours;
            if (CheckOutTime.Value >= CheckInTime.Value)
            {
                rawHours = (decimal)(CheckOutTime.Value - CheckInTime.Value).TotalHours;
            }
            else
            {
                // Ca xuyên đêm qua 24:00 (vd: vào 22:00, ra 06:00 sáng)
                rawHours = (decimal)(24.0 - CheckInTime.Value.TotalHours + CheckOutTime.Value.TotalHours);
            }

            // Nếu thời gian làm việc > 5 tiếng và có thời gian nghỉ thì trừ giờ nghỉ giữa ca
            decimal netHours = (rawHours > 5.0m && lunchBreakHours > 0m) 
                ? Math.Max(0m, rawHours - lunchBreakHours) 
                : rawHours;

            ActualHoursWorked = Math.Round(netHours, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            ActualHoursWorked = 0m;
        }

        return ActualHoursWorked;
    }

    /// <summary>
    /// Điểm danh thông minh (Punch): tự động ghi nhận Check-In nếu chưa có, hoặc Check-Out nếu đã Check-In.
    /// </summary>
    public void RecordPunch(TimeSpan punchTime, WorkShift? shift = null)
    {
        if (IsLocked)
            throw new TimesheetLockedException(Id, WorkDate);

        if (!CheckInTime.HasValue)
        {
            CheckInTime = punchTime;
            EvaluateCheckIn(shift);
        }
        else
        {
            CheckOutTime = punchTime;
            EvaluateShift(shift);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Đánh giá trạng thái khi nhân viên mới Check-In vào ca.
    /// </summary>
    public void EvaluateCheckIn(WorkShift? shift = null)
    {
        if (shift != null)
        {
            ShiftId = shift.Id;
            ShiftCode = shift.ShiftCode;
            ShiftName = shift.ShiftName;
        }

        var startTime = shift?.StartTime ?? new TimeSpan(8, 0, 0);
        int graceMinutes = shift?.GracePeriodMinutes ?? 15;

        if (CheckInTime.HasValue)
        {
            var lateThreshold = startTime.Add(TimeSpan.FromMinutes(graceMinutes));
            if (CheckInTime.Value > lateThreshold)
            {
                int lateMins = (int)(CheckInTime.Value - startTime).TotalMinutes;
                WorkShiftStatus = WorkShiftStatus.LATE;
                Note = $"Đi muộn {lateMins} phút ({ShiftName ?? "Ca làm việc"})";
            }
            else
            {
                WorkShiftStatus = WorkShiftStatus.PRESENT;
                Note = $"Vào ca đúng giờ ({ShiftName ?? "Ca làm việc"})";
            }
            WorkUnits = shift?.WorkUnits ?? 1.0m;
        }
    }

    /// <summary>
    /// Đánh giá ca làm việc theo khung giờ chuẩn truyền vào.
    /// </summary>
    public void EvaluateShift(TimeSpan? standardStartTime, TimeSpan? standardEndTime = null)
    {
        EvaluateShift(null, standardStartTime, standardEndTime);
    }

    /// <summary>
    /// Đánh giá toàn diện trạng thái ca làm việc và hệ số ngày công dựa trên quy định của ca được phân công.
    /// </summary>
    public void EvaluateShift(WorkShift? shift = null, TimeSpan? standardStartTime = null, TimeSpan? standardEndTime = null)
    {
        if (shift != null)
        {
            ShiftId = shift.Id;
            ShiftCode = shift.ShiftCode;
            ShiftName = shift.ShiftName;
        }

        var start = shift?.StartTime ?? standardStartTime ?? new TimeSpan(8, 0, 0);
        var end = shift?.EndTime ?? standardEndTime ?? new TimeSpan(17, 30, 0);
        decimal lunchBreak = shift?.LunchBreakHours ?? 1.5m;
        decimal targetHours = shift?.TargetHours ?? 8.0m;
        decimal baseUnits = shift?.WorkUnits ?? 1.0m;
        int graceMinutes = shift?.GracePeriodMinutes ?? 15;
        bool isCrossMidnight = shift?.IsNightShift ?? (end < start);

        if (!CheckInTime.HasValue && !CheckOutTime.HasValue)
        {
            WorkShiftStatus = WorkShiftStatus.ABSENT_UNAUTHORIZED;
            WorkUnits = 0m;
            ActualHoursWorked = 0m;
            Note = "Vắng mặt không phép";
            return;
        }

        CalculateHoursWorked(lunchBreak, isCrossMidnight);

        // Kiểm tra đi muộn
        bool isLate = false;
        int lateMins = 0;
        if (CheckInTime.HasValue)
        {
            var lateThreshold = start.Add(TimeSpan.FromMinutes(graceMinutes));
            if (CheckInTime.Value > lateThreshold)
            {
                isLate = true;
                lateMins = (int)(CheckInTime.Value - start).TotalMinutes;
            }
        }

        // Kiểm tra về sớm
        bool isEarlyLeave = false;
        int earlyMins = 0;
        if (CheckOutTime.HasValue)
        {
            var earlyThreshold = end.Subtract(TimeSpan.FromMinutes(graceMinutes));
            if (!isCrossMidnight && CheckOutTime.Value < earlyThreshold)
            {
                isEarlyLeave = true;
                earlyMins = (int)(end - CheckOutTime.Value).TotalMinutes;
            }
            else if (isCrossMidnight && CheckOutTime.Value < earlyThreshold && CheckOutTime.Value > start)
            {
                isEarlyLeave = true;
                earlyMins = (int)(end - CheckOutTime.Value).TotalMinutes;
            }
        }

        // Đánh giá tỷ lệ hoàn thành ca
        decimal completionRatio = targetHours > 0m ? (ActualHoursWorked / targetHours) : 1.0m;

        if (completionRatio >= 0.85m)
        {
            WorkUnits = baseUnits;
            if (isLate && isEarlyLeave)
            {
                WorkShiftStatus = WorkShiftStatus.LATE;
                Note = $"Đi muộn {lateMins}p, về sớm {earlyMins}p";
            }
            else if (isLate)
            {
                WorkShiftStatus = WorkShiftStatus.LATE;
                Note = $"Đi muộn {lateMins} phút";
            }
            else if (isEarlyLeave)
            {
                WorkShiftStatus = WorkShiftStatus.EARLY_LEAVE;
                Note = $"Về sớm {earlyMins} phút";
            }
            else
            {
                WorkShiftStatus = WorkShiftStatus.PRESENT;
                Note = "Hoàn thành đủ ca đúng giờ";
            }
        }
        else if (completionRatio >= 0.40m)
        {
            WorkShiftStatus = WorkShiftStatus.HALF_DAY;
            WorkUnits = Math.Round(baseUnits / 2.0m, 2, MidpointRounding.AwayFromZero);
            Note = $"Làm nửa ca ({ActualHoursWorked:0.#}/{targetHours:0.#} giờ)";
        }
        else
        {
            WorkShiftStatus = WorkShiftStatus.ABSENT_UNAUTHORIZED;
            WorkUnits = 0m;
            Note = $"Thời gian làm việc không đủ tối thiểu ({ActualHoursWorked:0.#}h)";
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
