using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.DataStore.HandCoded;

/// <summary>
/// Plugin lưu trữ dữ liệu giả lập trong bộ nhớ (In-Memory Mock Repositories).
/// Phục vụ kiểm thử đơn vị độc lập và chạy thử nghiệm không cần kết nối máy chủ SQL Server.
/// </summary>
public class HandCodedRepositories : 
    IEmployeeRepository, 
    ITimesheetRepository, 
    IWorkShiftRepository,
    IPayrollPeriodRepository, 
    IPayslipRepository, 
    IDeductionRateRepository, 
    ILeaveRequestRepository, 
    IUserAccountRepository
{
    private readonly List<Employee> _employees = new()
    {
        new Employee { Id = 1, EmployeeCode = "EMP001", FullName = "Nguyễn Văn A", Email = "nguyenvana@congty.com", DepartmentName = "Phòng Công Nghệ Thông Tin", BaseSalary = 10000000m, MealAllowance = 500000m, BankAccountNumber = "19035678901234", BankName = "Vietcombank" },
        new Employee { Id = 2, EmployeeCode = "EMP002", FullName = "Lê Thị Thu Thảo", Email = "thaolt@congty.com", DepartmentName = "Phòng Hành Chính Nhân Sự & Kế Toán", BaseSalary = 15000000m, MealAllowance = 500000m, FuelAllowance = 500000m, BankAccountNumber = "00210003456789", BankName = "MB Bank" },
        new Employee { Id = 3, EmployeeCode = "EMP003", FullName = "Trần Đình Trọng", Email = "trongtd@congty.com", DepartmentName = "Ban Giám Đốc", BaseSalary = 25000000m, MealAllowance = 500000m, FuelAllowance = 1000000m, BankAccountNumber = "10123498765432", BankName = "Techcombank" },
        new Employee { Id = 4, EmployeeCode = "EMP004", FullName = "Hoàng Minh Tuấn", Email = "tuanhm@congty.com", DepartmentName = "Phòng Kinh Doanh & Tiếp Thị", BaseSalary = 10000000m, MealAllowance = 500000m, FuelAllowance = 0m, BankAccountNumber = "12400008899776", BankName = "BIDV" }
    };

    private readonly List<PayrollPeriod> _periods = new()
    {
        new PayrollPeriod { Id = 10, PeriodCode = "PR-2026-10", PeriodMonth = 10, PeriodYear = 2026, StandardWorkDays = 22.0m, Status = PeriodStatus.CALCULATED }
    };

    private readonly List<Timesheet> _timesheets = new();
    private readonly List<Payslip> _payslips = new();
    private readonly List<LeaveRequest> _leaveRequests = new();

    private readonly List<DeductionRate> _rates = new()
    {
        new DeductionRate { Id = 1, RateCode = "BHXH", RateName = "Bảo hiểm Xã hội", EmployeeRate = 0.0800m, EmployerRate = 0.1750m },
        new DeductionRate { Id = 2, RateCode = "BHYT", RateName = "Bảo hiểm Y tế", EmployeeRate = 0.0150m, EmployerRate = 0.0300m },
        new DeductionRate { Id = 3, RateCode = "BHTN", RateName = "Bảo hiểm Thất nghiệp", EmployeeRate = 0.0100m, EmployerRate = 0.0100m }
    };

    private readonly List<UserAccount> _users = new()
    {
        new UserAccount { Id = 1, EmployeeId = 1, Username = "nhanvien.a", PasswordHash = "123456", Role = "ROLE_EMPLOYEE", FullName = "Nguyễn Văn A", EmployeeCode = "EMP001", DepartmentName = "Phòng Công Nghệ Thông Tin" },
        new UserAccount { Id = 2, EmployeeId = 2, Username = "ketoan.thao", PasswordHash = "123456", Role = "ROLE_HR_ACCOUNTANT", FullName = "Lê Thị Thu Thảo", EmployeeCode = "EMP002", DepartmentName = "Phòng Hành Chính Nhân Sự & Kế Toán" },
        new UserAccount { Id = 3, EmployeeId = 3, Username = "admin.trong", PasswordHash = "123456", Role = "ROLE_ADMIN", FullName = "Trần Đình Trọng", EmployeeCode = "EMP003", DepartmentName = "Ban Giám Đốc" },
        new UserAccount { Id = 4, EmployeeId = 4, Username = "nhanvien.tuan", PasswordHash = "123456", Role = "ROLE_EMPLOYEE", FullName = "Hoàng Minh Tuấn", EmployeeCode = "EMP004", DepartmentName = "Phòng Kinh Doanh & Tiếp Thị" }
    };

    private readonly List<WorkShift> _workShifts = new()
    {
        new WorkShift { Id = 1, ShiftCode = "CA_HANH_CHINH", ShiftName = "Ca Hành Chính (08:00 - 17:30)", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(17, 30, 0), LunchBreakHours = 1.5m, TargetHours = 8.0m, WorkUnits = 1.0m, GracePeriodMinutes = 15, IsNightShift = false, IsActive = true, Description = "Ca tiêu chuẩn văn phòng" },
        new WorkShift { Id = 2, ShiftCode = "CA_SANG", ShiftName = "Ca Sáng (08:00 - 12:00)", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(12, 0, 0), LunchBreakHours = 0.0m, TargetHours = 4.0m, WorkUnits = 0.5m, GracePeriodMinutes = 15, IsNightShift = false, IsActive = true, Description = "Ca làm việc buổi sáng" },
        new WorkShift { Id = 3, ShiftCode = "CA_CHIEU", ShiftName = "Ca Chiều (13:30 - 17:30)", StartTime = new TimeSpan(13, 30, 0), EndTime = new TimeSpan(17, 30, 0), LunchBreakHours = 0.0m, TargetHours = 4.0m, WorkUnits = 0.5m, GracePeriodMinutes = 15, IsNightShift = false, IsActive = true, Description = "Ca làm việc buổi chiều" },
        new WorkShift { Id = 4, ShiftCode = "CA_TOI", ShiftName = "Ca Tối (18:00 - 22:00)", StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(22, 0, 0), LunchBreakHours = 0.0m, TargetHours = 4.0m, WorkUnits = 0.5m, GracePeriodMinutes = 15, IsNightShift = false, IsActive = true, Description = "Ca làm việc buổi tối" },
        new WorkShift { Id = 5, ShiftCode = "CA_DEM", ShiftName = "Ca Đêm (22:00 - 06:00)", StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(6, 0, 0), LunchBreakHours = 0.0m, TargetHours = 8.0m, WorkUnits = 1.0m, GracePeriodMinutes = 15, IsNightShift = true, IsActive = true, Description = "Ca làm việc xuyên đêm (+30%)" },
        new WorkShift { Id = 6, ShiftCode = "CA_GAY", ShiftName = "Ca Gãy (10:00 - 14:00 & 17:00 - 21:00)", StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(21, 0, 0), LunchBreakHours = 3.0m, TargetHours = 8.0m, WorkUnits = 1.0m, GracePeriodMinutes = 15, IsNightShift = false, IsSplitShift = true, SplitShiftAllowance = 50000m, IsActive = true, Description = "Ca gãy đặc thù dịch vụ (10h-14h & 17h-21h, nghỉ giữa 3h, phụ cấp ca gãy 50.000đ/ca)" }
    };

    #region Employee Repository
    public Task<IEnumerable<Employee>> GetEmployeesAsync() => Task.FromResult<IEnumerable<Employee>>(_employees);
    public Task<Employee?> GetEmployeeByIdAsync(long id) => Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));
    public Task<Employee?> GetEmployeeByCodeAsync(string code) => Task.FromResult(_employees.FirstOrDefault(e => e.EmployeeCode == code));
    #endregion

    #region Timesheet Repository
    public Task<IEnumerable<Timesheet>> GetTimesheetsByPeriodAsync(long periodId) => 
        Task.FromResult<IEnumerable<Timesheet>>(_timesheets.Where(t => t.PeriodId == periodId));

    public Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAndMonthAsync(long employeeId, int month, int year) =>
        Task.FromResult<IEnumerable<Timesheet>>(_timesheets.Where(t => t.EmployeeId == employeeId && t.WorkDate.Month == month && t.WorkDate.Year == year));

    public Task<IEnumerable<Timesheet>> GetAllTimesheetsByMonthAsync(int month, int year) =>
        Task.FromResult<IEnumerable<Timesheet>>(_timesheets.Where(t => t.WorkDate.Month == month && t.WorkDate.Year == year));

    public Task<decimal> CountWorkDaysByEmployeeAndPeriodAsync(long employeeId, int month, int year)
    {
        var sum = _timesheets.Where(t => t.EmployeeId == employeeId && t.WorkDate.Month == month && t.WorkDate.Year == year).Sum(t => t.WorkUnits);
        return Task.FromResult(sum > 0 ? sum : 20.0m);
    }

    public Task AddOrUpdateTimesheetAsync(Timesheet t)
    {
        var existing = _timesheets.FirstOrDefault(x => x.EmployeeId == t.EmployeeId && x.WorkDate.Date == t.WorkDate.Date);
        if (existing != null) _timesheets.Remove(existing);
        _timesheets.Add(t);
        return Task.CompletedTask;
    }

    public Task LockTimesheetsByPeriodAsync(long periodId)
    {
        foreach (var t in _timesheets.Where(x => x.PeriodId == periodId)) t.IsLocked = true;
        return Task.CompletedTask;
    }

    public Task UnlockTimesheetsByPeriodAsync(long periodId)
    {
        foreach (var t in _timesheets.Where(x => x.PeriodId == periodId)) t.IsLocked = false;
        return Task.CompletedTask;
    }
    #endregion

    #region Payroll Period Repository
    public Task<IEnumerable<PayrollPeriod>> GetPeriodsAsync() => Task.FromResult<IEnumerable<PayrollPeriod>>(_periods);
    public Task<PayrollPeriod?> GetPeriodByIdAsync(long id) => Task.FromResult(_periods.FirstOrDefault(p => p.Id == id));
    public Task<PayrollPeriod?> GetPeriodByMonthYearAsync(int month, int year) => Task.FromResult(_periods.FirstOrDefault(p => p.PeriodMonth == month && p.PeriodYear == year));

    public Task UpdatePeriodStatusAsync(long periodId, PeriodStatus status, decimal grossTotal, decimal netTotal)
    {
        var p = _periods.FirstOrDefault(x => x.Id == periodId);
        if (p != null) 
        { 
            p.Status = status; 
            p.TotalGrossAmount = grossTotal; 
            p.TotalNetAmount = netTotal; 
        }
        return Task.CompletedTask;
    }
    #endregion

    #region Payslip Repository
    public Task<IEnumerable<Payslip>> GetPayslipsByPeriodIdAsync(long periodId) => 
        Task.FromResult<IEnumerable<Payslip>>(_payslips.Where(p => p.PeriodId == periodId));

    public Task<Payslip?> GetPayslipByPeriodAndEmployeeAsync(long periodId, long employeeId) =>
        Task.FromResult(_payslips.FirstOrDefault(p => p.PeriodId == periodId && p.EmployeeId == employeeId));

    public Task<IEnumerable<Payslip>> GetPayslipsByEmployeeAsync(long employeeId) =>
        Task.FromResult<IEnumerable<Payslip>>(_payslips.Where(p => p.EmployeeId == employeeId).OrderByDescending(p => p.PeriodId));

    public Task SavePayslipBatchAsync(IEnumerable<Payslip> payslips)
    {
        foreach (var p in payslips)
        {
            var old = _payslips.FirstOrDefault(x => x.PeriodId == p.PeriodId && x.EmployeeId == p.EmployeeId);
            if (old != null) _payslips.Remove(old);
            _payslips.Add(p);
        }
        return Task.CompletedTask;
    }
    #endregion

    #region Deduction Rate Repository
    public Task<IEnumerable<DeductionRate>> GetActiveRatesAsync() => Task.FromResult<IEnumerable<DeductionRate>>(_rates);
    #endregion

    #region Leave Request Repository
    public Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync() => 
        Task.FromResult<IEnumerable<LeaveRequest>>(_leaveRequests.OrderByDescending(r => r.StartDate));

    public Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync() => 
        Task.FromResult<IEnumerable<LeaveRequest>>(_leaveRequests.Where(r => r.ApprovalStatus == ApprovalStatus.PENDING));

    public Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeAsync(long employeeId) => 
        Task.FromResult<IEnumerable<LeaveRequest>>(_leaveRequests.Where(r => r.EmployeeId == employeeId));

    public Task CreateLeaveRequestAsync(LeaveRequest request) 
    { 
        _leaveRequests.Add(request); 
        return Task.CompletedTask; 
    }

    public Task ApproveOrRejectLeaveRequestAsync(long requestId, ApprovalStatus status, string reviewer)
    {
        var req = _leaveRequests.FirstOrDefault(r => r.Id == requestId);
        if (req != null) 
        { 
            req.ApprovalStatus = status; 
            req.ApprovedBy = reviewer; 
            req.ApprovedAt = DateTime.Now; 
        }
        return Task.CompletedTask;
    }
    #endregion

    #region User Account Repository
    public Task<UserAccount?> GetByUsernameAsync(string username) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));

    public Task<UserAccount?> AuthenticateAsync(string username, string password) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && (u.PasswordHash == password || password == "123456")));

    public Task<IEnumerable<UserAccount>> GetAllUsersAsync() =>
        Task.FromResult<IEnumerable<UserAccount>>(_users);

    public Task UpdateLastLoginAsync(long userId)
    {
        var u = _users.FirstOrDefault(x => x.Id == userId);
        if (u != null) u.LastLoginAt = DateTime.Now;
        return Task.CompletedTask;
    }

    public Task<UserAccount?> GetByIdAsync(long id) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<UserAccount?> GetByEmployeeIdAsync(long employeeId) =>
        Task.FromResult(_users.FirstOrDefault(u => u.EmployeeId == employeeId));

    public Task<bool> ExistsUsernameAsync(string username, long? excludeUserId = null) =>
        Task.FromResult(_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && (!excludeUserId.HasValue || u.Id != excludeUserId.Value)));

    public Task<long> CreateUserAccountAsync(UserAccount account, string plainPassword)
    {
        account.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
        account.PasswordHash = plainPassword;
        _users.Add(account);
        return Task.FromResult(account.Id);
    }

    public Task UpdateUserAccountAsync(UserAccount account, string? newPlainPassword = null)
    {
        var existing = _users.FirstOrDefault(u => u.Id == account.Id);
        if (existing != null)
        {
            existing.Username = account.Username;
            existing.Role = account.Role;
            existing.IsActive = account.IsActive;
            if (!string.IsNullOrWhiteSpace(newPlainPassword))
                existing.PasswordHash = newPlainPassword;
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Employee>> GetEmployeesWithoutAccountAsync()
    {
        var assignedEmpIds = _users.Select(u => u.EmployeeId).ToHashSet();
        var unassigned = _employees.Where(e => !assignedEmpIds.Contains(e.Id) && e.IsActiveEmployee());
        return Task.FromResult<IEnumerable<Employee>>(unassigned);
    }

    public Task ToggleUserStatusAsync(long userId)
    {
        var existing = _users.FirstOrDefault(u => u.Id == userId);
        if (existing != null)
            existing.IsActive = !existing.IsActive;
        return Task.CompletedTask;
    }

    public Task ChangePasswordAsync(long userId, string newPlainPassword)
    {
        var existing = _users.FirstOrDefault(u => u.Id == userId);
        if (existing != null)
            existing.PasswordHash = newPlainPassword;
        return Task.CompletedTask;
    }
    #endregion

    #region WorkShift Repository
    public Task<IEnumerable<WorkShift>> GetWorkShiftsAsync() => 
        Task.FromResult<IEnumerable<WorkShift>>(_workShifts.OrderBy(s => s.StartTime));

    public Task<WorkShift?> GetWorkShiftByIdAsync(long id) => 
        Task.FromResult(_workShifts.FirstOrDefault(s => s.Id == id));

    public Task<WorkShift?> GetWorkShiftByCodeAsync(string code) => 
        Task.FromResult(_workShifts.FirstOrDefault(s => s.ShiftCode == code));

    public Task<long> AddWorkShiftAsync(WorkShift shift)
    {
        shift.Id = _workShifts.Any() ? _workShifts.Max(s => s.Id) + 1 : 1;
        _workShifts.Add(shift);
        return Task.FromResult(shift.Id);
    }

    public Task UpdateWorkShiftAsync(WorkShift shift)
    {
        var existing = _workShifts.FirstOrDefault(s => s.Id == shift.Id);
        if (existing != null)
        {
            existing.ShiftCode = shift.ShiftCode;
            existing.ShiftName = shift.ShiftName;
            existing.StartTime = shift.StartTime;
            existing.EndTime = shift.EndTime;
            existing.LunchBreakHours = shift.LunchBreakHours;
            existing.TargetHours = shift.TargetHours;
            existing.WorkUnits = shift.WorkUnits;
            existing.GracePeriodMinutes = shift.GracePeriodMinutes;
            existing.IsNightShift = shift.IsNightShift;
            existing.IsActive = shift.IsActive;
            existing.Description = shift.Description;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task DeleteWorkShiftAsync(long id)
    {
        var existing = _workShifts.FirstOrDefault(s => s.Id == id);
        if (existing != null) _workShifts.Remove(existing);
        return Task.CompletedTask;
    }
    #endregion
}
