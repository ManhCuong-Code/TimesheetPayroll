using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;

namespace TimesheetPayroll.CoreBusiness.Services;

/// <summary>
/// Triển khai dịch vụ xác định ca làm việc tự động (Attendance Shift Resolver).
/// Tuân thủ nghiêm ngặt các quy tắc nghiệp vụ:
/// 1. Tự động xác định ca duy nhất phù hợp mà không cần thao tác thủ công.
/// 2. Bắt lỗi ca mơ hồ (Ambiguous) và không chọn ngẫu nhiên.
/// 3. Không cho phép chấm công nếu chưa có lịch phân công ca hợp lệ.
/// 4. Hỗ trợ đầy đủ ca qua đêm xuyên ngày (Overnight Cross-Midnight Shifts).
/// 5. Tự động phân biệt Check-In / Check-Out theo trạng thái thực tế.
/// </summary>
public class AttendanceShiftResolver : IAttendanceShiftResolver
{
    public AttendanceShiftResolutionResult ResolveShift(
        long employeeId,
        DateTime punchDateTime,
        IEnumerable<ShiftAssignment> assignedShifts,
        IEnumerable<Timesheet> currentTimesheets)
    {
        var punchDate = punchDateTime.Date;
        var punchTime = punchDateTime.TimeOfDay;
        var yesterday = punchDate.AddDays(-1);

        var activeAssignments = assignedShifts
            .Where(s => s.EmployeeId == employeeId && string.Equals(s.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!activeAssignments.Any())
        {
            return AttendanceShiftResolutionResult.NoShift("Bạn chưa được phân công ca làm việc phù hợp cho ngày hôm nay. Vui lòng liên hệ quản lý.");
        }

        var timesheetLookup = currentTimesheets
            .Where(t => t.EmployeeId == employeeId)
            .ToList();

        var candidates = new List<CandidateShiftEvaluation>();

        foreach (var sa in activeAssignments)
        {
            bool isOvernight = sa.IsOvernight || sa.IsNightShift || (sa.EndTime < sa.StartTime);

            // Kiểm tra xem ca này thuộc ngày hiện tại hay là ca qua đêm từ hôm qua
            bool isTodayAssignment = sa.WorkDate.Date == punchDate;
            bool isYesterdayOvernight = sa.WorkDate.Date == yesterday && isOvernight;

            if (!isTodayAssignment && !isYesterdayOvernight)
            {
                continue;
            }

            // Tìm timesheet hiện có của nhân viên cho ca và ngày này
            var existingTs = timesheetLookup.FirstOrDefault(t => 
                t.WorkDate.Date == sa.WorkDate.Date && 
                (t.ShiftId == sa.ShiftId || (t.ShiftId == null && t.ShiftCode == sa.ShiftCode)));

            DateTime shiftStartDT = sa.WorkDate.Date.Add(sa.StartTime);
            DateTime shiftEndDT = isOvernight 
                ? sa.WorkDate.Date.AddDays(1).Add(sa.EndTime) 
                : sa.WorkDate.Date.Add(sa.EndTime);

            // Cửa sổ chấm công: mặc định mở sớm 60 phút và đóng muộn 120 phút
            int earlyMinutes = 60;
            int lateMinutes = 120;
            DateTime windowStart = shiftStartDT.AddMinutes(-earlyMinutes);
            DateTime windowEnd = shiftEndDT.AddMinutes(lateMinutes);

            string action;
            if (existingTs == null || !existingTs.CheckInTime.HasValue)
            {
                action = "CHECK_IN";
            }
            else if (!existingTs.CheckOutTime.HasValue)
            {
                action = "CHECK_OUT";
            }
            else
            {
                action = "COMPLETED";
            }

            int score = 0;
            if (action == "CHECK_OUT" && existingTs?.CheckInTime.HasValue == true)
            {
                // Nếu đang có ca mở, và punchDateTime nằm trong khoảng từ sau khi check-in đến hết windowEnd
                DateTime checkInDT = sa.WorkDate.Date.Add(existingTs.CheckInTime.Value);
                if (punchDateTime >= checkInDT.AddMinutes(-30) && punchDateTime <= windowEnd)
                {
                    score = 100; // Ưu tiên cao nhất cho ca đang mở cần Check-Out
                }
            }
            else if (action == "CHECK_IN")
            {
                // Kiểm tra xem thời điểm quét có nằm trong cửa sổ cho phép Check-In không
                if (punchDateTime >= windowStart && punchDateTime <= shiftEndDT)
                {
                    score = 50;
                }
            }
            else
            {
                score = -100; // Đã hoàn thành cả 2 lượt
            }

            candidates.Add(new CandidateShiftEvaluation
            {
                Assignment = sa,
                ExistingTimesheet = existingTs,
                PunchAction = action,
                ShiftStartDT = shiftStartDT,
                ShiftEndDT = shiftEndDT,
                WindowStart = windowStart,
                WindowEnd = windowEnd,
                IsOvernight = isOvernight,
                Score = score
            });
        }

        // Lọc các ca có điểm ưu tiên hợp lệ
        var eligible = candidates.Where(c => c.Score > 0).ToList();

        if (eligible.Count == 1)
        {
            var match = eligible.First();
            return new AttendanceShiftResolutionResult
            {
                ResolutionStatus = "RESOLVED",
                Message = "Đã xác định ca làm việc thành công.",
                ShiftId = match.Assignment.ShiftId,
                ShiftCode = match.Assignment.ShiftCode,
                ShiftName = match.Assignment.ShiftName,
                WorkDate = match.Assignment.WorkDate,
                PunchAction = match.PunchAction,
                StartTime = match.Assignment.StartTime,
                EndTime = match.Assignment.EndTime,
                IsOvernight = match.IsOvernight,
                CandidateCount = 1
            };
        }

        if (eligible.Count > 1)
        {
            // Kiểm tra nếu có 1 ca có score vượt trội (ví dụ ca CHECK_OUT = 100 điểm)
            int maxScore = eligible.Max(c => c.Score);
            var topScored = eligible.Where(c => c.Score == maxScore).ToList();

            if (topScored.Count == 1)
            {
                var match = topScored.First();
                return new AttendanceShiftResolutionResult
                {
                    ResolutionStatus = "RESOLVED",
                    Message = "Đã tự động chọn ca đang mở có độ ưu tiên cao nhất.",
                    ShiftId = match.Assignment.ShiftId,
                    ShiftCode = match.Assignment.ShiftCode,
                    ShiftName = match.Assignment.ShiftName,
                    WorkDate = match.Assignment.WorkDate,
                    PunchAction = match.PunchAction,
                    StartTime = match.Assignment.StartTime,
                    EndTime = match.Assignment.EndTime,
                    IsOvernight = match.IsOvernight,
                    CandidateCount = 1
                };
            }

            // Trường hợp 2: Có nhiều ca vẫn phù hợp ngang nhau -> Báo mơ hồ, không tự ý chọn bừa bãi
            return AttendanceShiftResolutionResult.Ambiguous(
                "Không xác định được ca làm việc duy nhất. Vui lòng liên hệ quản lý.",
                eligible.Count);
        }

        // Kiểm tra xem tất cả ca đã hoàn tất chưa
        if (candidates.Any() && candidates.All(c => c.PunchAction == "COMPLETED"))
        {
            return AttendanceShiftResolutionResult.ShiftCompleted(
                "Bạn đã hoàn thành đủ lượt vào và ra cho tất cả các ca được phân công.");
        }

        // Nằm ngoài khung giờ
        return AttendanceShiftResolutionResult.OutsideWindow(
            "Thời điểm chấm công hiện tại nằm ngoài khung giờ cho phép của ca được phân công.");
    }

    private class CandidateShiftEvaluation
    {
        public ShiftAssignment Assignment { get; set; } = null!;
        public Timesheet? ExistingTimesheet { get; set; }
        public string PunchAction { get; set; } = "CHECK_IN";
        public DateTime ShiftStartDT { get; set; }
        public DateTime ShiftEndDT { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
        public bool IsOvernight { get; set; }
        public int Score { get; set; }
    }
}
