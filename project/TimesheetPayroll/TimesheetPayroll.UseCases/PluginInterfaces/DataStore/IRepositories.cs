using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

/// <summary>
/// Cổng giao tiếp dữ liệu hồ sơ nhân sự và hợp đồng.
/// </summary>
public interface IEmployeeRepository
{
    /// <summary>
    /// Lấy danh sách toàn bộ nhân sự trong doanh nghiệp.
    /// </summary>
    Task<IEnumerable<Employee>> GetEmployeesAsync();

    /// <summary>
    /// Tìm hồ sơ nhân viên theo ID nội bộ.
    /// </summary>
    Task<Employee?> GetEmployeeByIdAsync(long id);

    /// <summary>
    /// Tìm hồ sơ nhân viên theo mã định danh (Ví dụ: EMP001).
    /// </summary>
    Task<Employee?> GetEmployeeByCodeAsync(string code);
}

/// <summary>
/// Cổng giao tiếp dữ liệu điểm danh và bảng chấm công.
/// </summary>
public interface ITimesheetRepository
{
    /// <summary>
    /// Lấy toàn bộ dữ liệu chấm công thuộc một kỳ lương xác định.
    /// </summary>
    Task<IEnumerable<Timesheet>> GetTimesheetsByPeriodAsync(long periodId);

    /// <summary>
    /// Lấy dữ liệu chấm công cá nhân theo tháng và năm.
    /// </summary>
    Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAndMonthAsync(long employeeId, int month, int year);

    /// <summary>
    /// Lấy toàn bộ dữ liệu chấm công của tất cả nhân viên theo tháng và năm phục vụ kế toán đối soát tổng hợp.
    /// </summary>
    Task<IEnumerable<Timesheet>> GetAllTimesheetsByMonthAsync(int month, int year);

    /// <summary>
    /// Tổng hợp số ngày công thực tế của nhân viên trong tháng phục vụ tính lương.
    /// </summary>
    Task<decimal> CountWorkDaysByEmployeeAndPeriodAsync(long employeeId, int month, int year);

    /// <summary>
    /// Ghi nhận Check-in/Check-out mới hoặc cập nhật bản ghi chấm công trong ngày.
    /// </summary>
    Task AddOrUpdateTimesheetAsync(Timesheet timesheet);

    /// <summary>
    /// Khóa toàn bộ bản ghi chấm công của kỳ lương sang chế độ Chỉ đọc (Read-Only).
    /// </summary>
    Task LockTimesheetsByPeriodAsync(long periodId);

    /// <summary>
    /// Mở khóa toàn bộ bản ghi chấm công của kỳ lương để cho phép điểm danh và cập nhật lại.
    /// </summary>
    Task UnlockTimesheetsByPeriodAsync(long periodId);
}

/// <summary>
/// Cổng giao tiếp dữ liệu danh mục ca làm việc (Work Shifts).
/// </summary>
public interface IWorkShiftRepository
{
    /// <summary>
    /// Lấy danh sách toàn bộ các ca làm việc đang có trong hệ thống.
    /// </summary>
    Task<IEnumerable<WorkShift>> GetWorkShiftsAsync();

    /// <summary>
    /// Tìm ca làm việc theo ID.
    /// </summary>
    Task<WorkShift?> GetWorkShiftByIdAsync(long id);

    /// <summary>
    /// Tìm ca làm việc theo mã định danh (vd: CA_HANH_CHINH, CA_SANG).
    /// </summary>
    Task<WorkShift?> GetWorkShiftByCodeAsync(string code);

    /// <summary>
    /// Thêm mới một ca làm việc vào hệ thống.
    /// </summary>
    Task<long> AddWorkShiftAsync(WorkShift shift);

    /// <summary>
    /// Cập nhật thông tin ca làm việc.
    /// </summary>
    Task UpdateWorkShiftAsync(WorkShift shift);

    /// <summary>
    /// Xóa ca làm việc khỏi hệ thống.
    /// </summary>
    Task DeleteWorkShiftAsync(long id);
}

/// <summary>
/// Cổng giao tiếp dữ liệu kỳ tính lương hàng tháng.
/// </summary>
public interface IPayrollPeriodRepository
{
    /// <summary>
    /// Lấy danh sách tất cả các kỳ lương trong hệ thống.
    /// </summary>
    Task<IEnumerable<PayrollPeriod>> GetPeriodsAsync();

    /// <summary>
    /// Lấy thông tin kỳ lương theo ID.
    /// </summary>
    Task<PayrollPeriod?> GetPeriodByIdAsync(long id);

    /// <summary>
    /// Tìm kỳ lương theo tháng và năm.
    /// </summary>
    Task<PayrollPeriod?> GetPeriodByMonthYearAsync(int month, int year);

    /// <summary>
    /// Cập nhật trạng thái kỳ lương và tổng quỹ lương (Gross, Net).
    /// </summary>
    Task UpdatePeriodStatusAsync(long periodId, PeriodStatus status, decimal grossTotal, decimal netTotal);
}

/// <summary>
/// Cổng giao tiếp dữ liệu phiếu lương và các dòng chi tiết cấu thành.
/// </summary>
public interface IPayslipRepository
{
    /// <summary>
    /// Lấy toàn bộ danh sách phiếu lương của một kỳ phục vụ đối soát tổng hợp.
    /// </summary>
    Task<IEnumerable<Payslip>> GetPayslipsByPeriodIdAsync(long periodId);

    /// <summary>
    /// Tra cứu phiếu lương của một nhân viên trong kỳ (Bảo vệ chống lộ IDOR).
    /// </summary>
    Task<Payslip?> GetPayslipByPeriodAndEmployeeAsync(long periodId, long employeeId);

    /// <summary>
    /// Tra cứu danh sách toàn bộ phiếu lương các kỳ của một nhân viên (phục vụ so sánh và tra cứu lịch sử).
    /// </summary>
    Task<IEnumerable<Payslip>> GetPayslipsByEmployeeAsync(long employeeId);

    /// <summary>
    /// Lưu trữ hàng loạt phiếu lương và các dòng chi tiết sau khi chạy tính toán lương tự động.
    /// </summary>
    Task SavePayslipBatchAsync(IEnumerable<Payslip> payslips);
}

/// <summary>
/// Cổng giao tiếp dữ liệu các tỷ lệ trích nộp bảo hiểm (BHXH, BHYT, BHTN).
/// </summary>
public interface IDeductionRateRepository
{
    /// <summary>
    /// Lấy danh sách các tỷ lệ bảo hiểm đang có hiệu lực thi hành.
    /// </summary>
    Task<IEnumerable<DeductionRate>> GetActiveRatesAsync();
}

/// <summary>
/// Cổng giao tiếp dữ liệu đơn xin nghỉ phép của nhân viên.
/// </summary>
public interface ILeaveRequestRepository
{
    /// <summary>
    /// Lấy toàn bộ danh sách đơn nghỉ phép của toàn công ty.
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync();

    /// <summary>
    /// Lấy danh sách các đơn nghỉ phép đang chờ phòng nhân sự xét duyệt.
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync();

    /// <summary>
    /// Lấy lịch sử nộp đơn xin nghỉ phép của một nhân viên.
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeAsync(long employeeId);

    /// <summary>
    /// Tạo mới một đơn xin nghỉ phép trực tuyến.
    /// </summary>
    Task CreateLeaveRequestAsync(LeaveRequest request);

    /// <summary>
    /// Phê duyệt (APPROVED) hoặc từ chối (REJECTED) đơn xin nghỉ phép kèm lý do nếu từ chối.
    /// </summary>
    Task ApproveOrRejectLeaveRequestAsync(long requestId, ApprovalStatus status, string reviewer, string? rejectReason = null);
}

/// <summary>
/// Cổng giao tiếp dữ liệu tài khoản người dùng và xác thực bảo mật.
/// </summary>
public interface IUserAccountRepository
{
    /// <summary>
    /// Tìm thông tin tài khoản theo tên đăng nhập.
    /// </summary>
    Task<UserAccount?> GetByUsernameAsync(string username);

    /// <summary>
    /// Xác thực đăng nhập qua mật khẩu mã hóa BCrypt an toàn.
    /// </summary>
    Task<UserAccount?> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Lấy danh sách tất cả tài khoản trong hệ thống phục vụ quản trị phân quyền.
    /// </summary>
    Task<IEnumerable<UserAccount>> GetAllUsersAsync();

    /// <summary>
    /// Cập nhật thời điểm đăng nhập thành công gần nhất.
    /// </summary>
    Task UpdateLastLoginAsync(long userId);

    /// <summary>
    /// Tìm thông tin tài khoản theo mã ID tài khoản.
    /// </summary>
    Task<UserAccount?> GetByIdAsync(long id);

    /// <summary>
    /// Tìm thông tin tài khoản theo mã nhân viên liên kết.
    /// </summary>
    Task<UserAccount?> GetByEmployeeIdAsync(long employeeId);

    /// <summary>
    /// Kiểm tra tên đăng nhập đã tồn tại trong hệ thống hay chưa (chống trùng lặp).
    /// </summary>
    Task<bool> ExistsUsernameAsync(string username, long? excludeUserId = null);

    /// <summary>
    /// Cấp tài khoản mới cho nhân viên kèm mật khẩu được mã hóa an toàn.
    /// </summary>
    Task<long> CreateUserAccountAsync(UserAccount account, string plainPassword);

    /// <summary>
    /// Cập nhật thông tin tài khoản (vai trò, tên đăng nhập, trạng thái, mật khẩu mới nếu có).
    /// </summary>
    Task UpdateUserAccountAsync(UserAccount account, string? newPlainPassword = null);

    /// <summary>
    /// Lấy danh sách nhân viên chưa được cấp tài khoản để quản trị viên cấp mới.
    /// </summary>
    Task<IEnumerable<Employee>> GetEmployeesWithoutAccountAsync();

    /// <summary>
    /// Chuyển đổi trạng thái hoạt động của tài khoản (Kích hoạt / Khóa tài khoản).
    /// </summary>
    Task ToggleUserStatusAsync(long userId);

    /// <summary>
    /// Cập nhật mật khẩu mới cho tài khoản người dùng và mã hóa BCrypt an toàn.
    /// </summary>
    Task ChangePasswordAsync(long userId, string newPlainPassword);
}

/// <summary>
/// Cổng giao tiếp dữ liệu hồ sơ đặc trưng khuôn mặt nhân viên (128-d Biometric Vector).
/// </summary>
public interface IFaceProfileRepository
{
    Task<EmployeeFaceProfile?> GetByEmployeeIdAsync(long employeeId);
    Task<IEnumerable<EmployeeFaceProfile>> GetAllActiveProfilesAsync();
    Task<IEnumerable<EmployeeFaceProfile>> GetAllProfilesWithEmployeeDetailsAsync();
    Task SaveProfileAsync(EmployeeFaceProfile profile);
    Task DeleteProfileAsync(long employeeId);
    Task ToggleStatusAsync(long employeeId, bool isActive);
    Task UpdateProfileMetadataAsync(long employeeId, string? notes, decimal livenessThreshold);
}

/// <summary>
/// Cổng giao tiếp dữ liệu nhật ký kiểm toán khuôn mặt và chấm công camera.
/// </summary>
public interface IFaceAuditLogRepository
{
    Task LogActionAsync(FaceAuditLog log);
    Task<IEnumerable<FaceAuditLog>> GetLogsAsync(int limit = 100);
}

/// <summary>
/// Cổng giao tiếp dữ liệu nhật ký chỉnh sửa / điều chỉnh bảng chấm công.
/// </summary>
public interface ITimesheetAuditLogRepository
{
    Task LogAuditAsync(TimesheetAuditLog log);
    Task<IEnumerable<TimesheetAuditLog>> GetLogsByEmployeeAsync(long employeeId, int limit = 50);
    Task<IEnumerable<TimesheetAuditLog>> GetRecentLogsAsync(int limit = 100);
}

/// <summary>
/// Cổng giao tiếp dữ liệu yêu cầu chấm công thủ công (bổ sung giờ vào/ra, điều chỉnh ca làm việc).
/// </summary>
public interface IAttendanceRequestRepository
{
    Task<long> CreateAttendanceRequestAsync(AttendanceRequest request);
    Task<AttendanceRequest?> GetAttendanceRequestByIdAsync(long id);
    Task<IEnumerable<AttendanceRequest>> GetAllAttendanceRequestsAsync();
    Task<IEnumerable<AttendanceRequest>> GetPendingAttendanceRequestsAsync();
    Task<IEnumerable<AttendanceRequest>> GetAttendanceRequestsByEmployeeAsync(long employeeId);
    Task<bool> ApproveAttendanceRequestAsync(long requestId, string approvedBy);
    Task<bool> RejectAttendanceRequestAsync(long requestId, string approvedBy, string rejectionReason);
    Task<int> CountPendingAttendanceRequestsAsync();
}

/// <summary>
/// Cổng giao tiếp dữ liệu phân công ca làm việc và các thủ tục xử lý chấm công tự động trong SQL Server.
/// </summary>
public interface IShiftAssignmentRepository
{
    Task<IEnumerable<ShiftAssignment>> GetAssignmentsByEmployeeAsync(long employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ShiftAssignment>> GetAssignmentsByDateAsync(DateTime date);
    Task<IEnumerable<ShiftAssignment>> GetAssignmentsByMonthAsync(int month, int year);
    Task<long> AssignShiftAsync(ShiftAssignment assignment);
    Task AssignShiftBatchAsync(IEnumerable<ShiftAssignment> assignments);
    Task DeleteAssignmentAsync(long id);
    Task<AttendanceShiftResolutionResult> ResolveAttendanceShiftViaSpAsync(long employeeId, DateTime punchDateTime);
    Task<FacePunchDbResult> ProcessFaceAttendanceViaSpAsync(long employeeId, long shiftId, DateTime workDate, DateTime punchDateTime, string punchAction, string? deviceInfo, string performedBy, decimal confidence, double distance, string? photoUrl = null);
    Task<IEnumerable<AttendanceStatusDto>> GetAttendanceStatusViaSpAsync(long employeeId, DateTime targetDate);
    Task<IEnumerable<dynamic>> GetMonthlyAttendanceSummaryViaSpAsync(int month, int year, long? employeeId = null);
    Task<bool> ResetTodayPunchForTestAsync(long employeeId, DateTime workDate);
}



