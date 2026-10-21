namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Vai trò người dùng trong hệ thống phân quyền (RBAC).
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Nhân viên thông thường: xem công cá nhân, nộp đơn phép, xem phiếu lương của chính mình.
    /// </summary>
    ROLE_EMPLOYEE,

    /// <summary>
    /// Kế toán tiền lương / Nhân sự: quản lý bảng công, duyệt đơn, chạy tính lương, xem bảng tổng hợp.
    /// </summary>
    ROLE_HR_ACCOUNTANT,

    /// <summary>
    /// Quản lý bộ phận: xem và xác nhận chấm công, xem báo cáo phòng ban, duyệt đơn nghỉ phép.
    /// </summary>
    ROLE_MANAGER,

    /// <summary>
    /// Quản trị viên hệ thống: toàn quyền cấu hình, khóa sổ kỳ lương và quản trị tài khoản.
    /// </summary>
    ROLE_ADMIN
}

/// <summary>
/// Trạng thái ca làm việc hàng ngày của nhân viên.
/// </summary>
public enum WorkShiftStatus
{
    /// <summary>
    /// Đúng giờ (hưởng 1.0 công).
    /// </summary>
    PRESENT,

    /// <summary>
    /// Đến muộn so với quy định.
    /// </summary>
    LATE,

    /// <summary>
    /// Về sớm so với quy định.
    /// </summary>
    EARLY_LEAVE,

    /// <summary>
    /// Làm việc nửa ngày (hưởng 0.5 công).
    /// </summary>
    HALF_DAY,

    /// <summary>
    /// Nghỉ việc có lý do / có phép được duyệt.
    /// </summary>
    ABSENT_WITH_PERMISSION,

    /// <summary>
    /// Nghỉ việc không phép (không tính công, trừ lương).
    /// </summary>
    ABSENT_UNAUTHORIZED,

    /// <summary>
    /// Ngày nghỉ lễ / Tết theo quy định nhà nước (hưởng nguyên lương).
    /// </summary>
    HOLIDAY
}

/// <summary>
/// Phân loại hình thức nghỉ phép của nhân viên.
/// </summary>
public enum LeaveType
{
    /// <summary>
    /// Nghỉ phép năm (hưởng 100% lương cơ bản theo Bộ luật Lao động).
    /// </summary>
    ANNUAL_LEAVE,

    /// <summary>
    /// Nghỉ ốm đau (chế độ bảo hiểm xã hội chi trả).
    /// </summary>
    SICK_LEAVE,

    /// <summary>
    /// Nghỉ thai sản theo quy định luật BHXH.
    /// </summary>
    MATERNITY_LEAVE,

    /// <summary>
    /// Nghỉ không hưởng lương.
    /// </summary>
    UNPAID_LEAVE
}

/// <summary>
/// Trạng thái phê duyệt đơn nghỉ phép của phòng nhân sự.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// Đơn mới tạo, đang chờ quản lý/HR xét duyệt.
    /// </summary>
    PENDING,

    /// <summary>
    /// Đơn đã được chấp thuận.
    /// </summary>
    APPROVED,

    /// <summary>
    /// Đơn bị từ chối kèm lý do phản hồi.
    /// </summary>
    REJECTED
}

/// <summary>
/// Vòng đời trạng thái của một kỳ lương (State Machine).
/// </summary>
public enum PeriodStatus
{
    /// <summary>
    /// Dự thảo: tiếp nhận bảng công, đơn nghỉ phép, cho phép chỉnh sửa thông tin nhân sự.
    /// </summary>
    DRAFT,

    /// <summary>
    /// Đang tính toán: hệ thống đang chạy batch tính toán lương tự động trong Transaction.
    /// </summary>
    CALCULATING,

    /// <summary>
    /// Đã tính: tạo bản nháp phiếu lương để kế toán đối soát tổng quỹ lương (Preview).
    /// </summary>
    CALCULATED,

    /// <summary>
    /// Khóa sổ: dữ liệu chuyển sang trạng thái Chỉ đọc (Read-Only) vĩnh viễn, ngăn chặn sửa đổi.
    /// </summary>
    LOCKED,

    /// <summary>
    /// Đã thanh toán: đã hoàn tất chuyển khoản ủy nhiệm chi ngân hàng tới nhân viên.
    /// </summary>
    PAID
}

/// <summary>
/// Loại khoản mục chi tiết cấu thành trên phiếu lương.
/// </summary>
public enum PayslipLineType
{
    /// <summary>
    /// Lương thời gian theo ngày công thực tế đi làm.
    /// </summary>
    BASE_SALARY_PRORATED,

    /// <summary>
    /// Lương ngày nghỉ phép có hưởng lương (phép năm).
    /// </summary>
    PAID_LEAVE_PAY,

    /// <summary>
    /// Phụ cấp tiền ăn trưa theo ngày/tháng.
    /// </summary>
    MEAL_ALLOWANCE,

    /// <summary>
    /// Phụ cấp tiền xăng xe / đi lại.
    /// </summary>
    FUEL_ALLOWANCE,

    /// <summary>
    /// Tiền làm thêm giờ (Overtime pay: hệ số 1.5x hoặc 2.0x).
    /// </summary>
    OVERTIME_PAY,

    /// <summary>
    /// Thưởng hiệu suất công việc / KPI.
    /// </summary>
    BONUS,

    /// <summary>
    /// Phụ cấp đãi ngộ làm việc ca gãy (Split Shift Allowance) do tính chất ngắt quãng đặc thù.
    /// </summary>
    SPLIT_SHIFT_ALLOWANCE,

    /// <summary>
    /// Trích nộp Bảo hiểm Xã hội (8% của người lao động).
    /// </summary>
    DEDUCTION_BHXH,

    /// <summary>
    /// Trích nộp Bảo hiểm Y tế (1.5% của người lao động).
    /// </summary>
    DEDUCTION_BHYT,

    /// <summary>
    /// Trích nộp Bảo hiểm Thất nghiệp (1% của người lao động).
    /// </summary>
    DEDUCTION_BHTN,

    /// <summary>
    /// Thuế thu nhập cá nhân (TNCN) tạm tính theo biểu lũy tiến.
    /// </summary>
    PERSONAL_INCOME_TAX,

    /// <summary>
    /// Khấu trừ vi phạm nội quy / đi muộn / kỷ luật.
    /// </summary>
    PENALTY
}

/// <summary>
/// Loại hình hợp đồng lao động ký kết.
/// </summary>
public enum ContractType
{
    /// <summary>
    /// Hợp đồng thử việc (thường hưởng 85% lương hoặc thỏa thuận).
    /// </summary>
    PROBATION,

    /// <summary>
    /// Hợp đồng xác định thời hạn (1 - 3 năm).
    /// </summary>
    FULL_TIME,

    /// <summary>
    /// Hợp đồng không xác định thời hạn.
    /// </summary>
    INDEFINITE
}

/// <summary>
/// Trạng thái hoạt động của nhân sự trong doanh nghiệp.
/// </summary>
public enum EmployeeStatus
{
    /// <summary>
    /// Đang làm việc tích cực tại công ty.
    /// </summary>
    ACTIVE,

    /// <summary>
    /// Đang tạm hoãn hợp đồng / nghỉ thai sản / nghỉ dài hạn.
    /// </summary>
    ON_LEAVE,

    /// <summary>
    /// Đã thôi việc / chấm dứt hợp đồng.
    /// </summary>
    RESIGNED
}
