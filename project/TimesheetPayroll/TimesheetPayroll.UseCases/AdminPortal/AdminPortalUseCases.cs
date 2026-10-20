using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.AdminPortal;

#region 1. TÍNH TOÁN BẢNG LƯƠNG TỰ ĐỘNG HÀNG LOẠT (UC14 / UC_HR_07)

/// <summary>
/// Ca sử dụng: Chạy tính toán bảng lương tự động hàng loạt 1-chạm (UC14).
/// Duyệt qua toàn bộ nhân viên hoạt động, tổng hợp ngày công và áp dụng công thức lương chuẩn.
/// </summary>
public interface ICalculatePayrollBatchUseCase
{
    Task<IEnumerable<Payslip>> ExecuteAsync(long periodId);
}

public class CalculatePayrollBatchUseCase : ICalculatePayrollBatchUseCase
{
    private readonly IEmployeeRepository _empRepo;
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly IPayrollPeriodRepository _periodRepo;
    private readonly IDeductionRateRepository _rateRepo;
    private readonly IPayslipRepository _payslipRepo;
    private readonly IPayrollCalculationService _calcService;

    public CalculatePayrollBatchUseCase(
        IEmployeeRepository empRepo,
        ITimesheetRepository timesheetRepo,
        IPayrollPeriodRepository periodRepo,
        IDeductionRateRepository rateRepo,
        IPayslipRepository payslipRepo,
        IPayrollCalculationService calcService)
    {
        _empRepo = empRepo;
        _timesheetRepo = timesheetRepo;
        _periodRepo = periodRepo;
        _rateRepo = rateRepo;
        _payslipRepo = payslipRepo;
        _calcService = calcService;
    }

    public async Task<IEnumerable<Payslip>> ExecuteAsync(long periodId)
    {
        var period = await _periodRepo.GetPeriodByIdAsync(periodId);
        if (period == null)
            throw new InvalidOperationException($"Không tìm thấy kỳ lương với ID = {periodId}.");

        // Quy tắc nghiệp vụ: Kỳ lương đã khóa sổ (Read-Only) không được phép tính lại
        if (!period.CanCalculate())
            throw new InvalidOperationException($"Kỳ lương {period.PeriodCode} đã ở trạng thái {period.Status} (Khóa sổ) và không được phép tính lại.");

        var employees = await _empRepo.GetEmployeesAsync();
        var rates = (await _rateRepo.GetActiveRatesAsync()).ToList();

        var payslips = new List<Payslip>();
        foreach (var emp in employees.Where(e => e.IsActiveEmployee()))
        {
            var empTimesheets = (await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(
                emp.Id, period.PeriodMonth, period.PeriodYear)).ToList();

            decimal standard8hDays = 0m;
            decimal splitShiftDays = 0m;
            decimal otherShiftDays = 0m;

            if (empTimesheets.Any())
            {
                // Phân loại chi tiết từng loại ca:
                splitShiftDays = empTimesheets
                    .Where(t => t.ShiftCode == "CA_GAY" || (t.ShiftName != null && t.ShiftName.Contains("Ca Gãy", StringComparison.OrdinalIgnoreCase)))
                    .Sum(t => t.WorkUnits);

                standard8hDays = empTimesheets
                    .Where(t => (t.ShiftCode != "CA_GAY" && !(t.ShiftName != null && t.ShiftName.Contains("Ca Gãy", StringComparison.OrdinalIgnoreCase))) && t.WorkUnits >= 1.0m)
                    .Sum(t => t.WorkUnits);

                otherShiftDays = empTimesheets
                    .Where(t => (t.ShiftCode != "CA_GAY" && !(t.ShiftName != null && t.ShiftName.Contains("Ca Gãy", StringComparison.OrdinalIgnoreCase))) && t.WorkUnits < 1.0m && t.WorkUnits > 0m)
                    .Sum(t => t.WorkUnits);
            }
            else
            {
                // Đối với nhân viên chính thức nếu chưa phát sinh chấm công riêng lẻ trong kỳ,
                // mặc định tính đủ ngày công chuẩn của ca 8 tiếng hành chính
                standard8hDays = period.StandardWorkDays;
            }

            var payslip = _calcService.CalculatePayslip(
                emp, periodId,
                standard8hDays: standard8hDays,
                splitShiftDays: splitShiftDays,
                otherShiftDays: otherShiftDays,
                standardWorkDays: period.StandardWorkDays,
                rates: rates,
                paidLeaveDays: 0m, overtimeHours: 0m, bonusAmount: 0m, penaltyAmount: 0m,
                splitShiftAllowancePerDay: 50000m,
                calculateTax: true);

            payslips.Add(payslip);
        }

        // Lưu trữ toàn bộ kết quả phiếu lương theo lô
        await _payslipRepo.SavePayslipBatchAsync(payslips);

        // Cập nhật trạng thái kỳ tính lương sang CALCULATED và cập nhật tổng quỹ lương
        decimal totalGross = payslips.Sum(p => p.GrossSalary);
        decimal totalNet = payslips.Sum(p => p.NetSalary);
        await _periodRepo.UpdatePeriodStatusAsync(periodId, PeriodStatus.CALCULATED, totalGross, totalNet);

        return payslips;
    }
}

#endregion

#region 2. ĐỐI SOÁT QUỸ LƯƠNG TỔNG HỢP (UC15 / UC_HR_08)

/// <summary>
/// Ca sử dụng: Xem bảng lương tổng hợp và đối soát quỹ lương trước khi chốt sổ (UC15).
/// </summary>
public interface IViewPayrollSummaryUseCase
{
    Task<(PayrollPeriod? Period, IEnumerable<Payslip> Payslips)> ExecuteAsync(long periodId);
}

public class ViewPayrollSummaryUseCase : IViewPayrollSummaryUseCase
{
    private readonly IPayrollPeriodRepository _periodRepo;
    private readonly IPayslipRepository _payslipRepo;

    public ViewPayrollSummaryUseCase(IPayrollPeriodRepository periodRepo, IPayslipRepository payslipRepo)
    {
        _periodRepo = periodRepo;
        _payslipRepo = payslipRepo;
    }

    public async Task<(PayrollPeriod? Period, IEnumerable<Payslip> Payslips)> ExecuteAsync(long periodId)
    {
        var period = await _periodRepo.GetPeriodByIdAsync(periodId);
        var payslips = await _payslipRepo.GetPayslipsByPeriodIdAsync(periodId);
        return (period, payslips);
    }
}

#endregion

#region 3. CHỐT KỲ LƯƠNG KHÓA SỔ READ-ONLY (UC16 / UC_HR_09)

/// <summary>
/// Ca sử dụng: Chốt kỳ lương khóa sổ (Read-Only Lock), bảo vệ tính bất biến của dữ liệu (UC16).
/// </summary>
public interface ILockPayrollPeriodUseCase
{
    Task<bool> ExecuteAsync(long periodId, string lockedBy);
}

public class LockPayrollPeriodUseCase : ILockPayrollPeriodUseCase
{
    private readonly IPayrollPeriodRepository _periodRepo;
    private readonly ITimesheetRepository _timesheetRepo;

    public LockPayrollPeriodUseCase(IPayrollPeriodRepository periodRepo, ITimesheetRepository timesheetRepo)
    {
        _periodRepo = periodRepo;
        _timesheetRepo = timesheetRepo;
    }

    public async Task<bool> ExecuteAsync(long periodId, string lockedBy)
    {
        var period = await _periodRepo.GetPeriodByIdAsync(periodId);
        if (period == null || period.IsLocked())
            return false;

        // 1. Khóa toàn bộ các bản ghi chấm công của kỳ lương
        await _timesheetRepo.LockTimesheetsByPeriodAsync(periodId);

        // 2. Chuyển trạng thái kỳ sang LOCKED (Khóa sổ chống sửa đổi)
        await _periodRepo.UpdatePeriodStatusAsync(
            periodId, PeriodStatus.LOCKED, period.TotalGrossAmount, period.TotalNetAmount);

        return true;
    }
}

/// <summary>
/// Ca sử dụng: Mở khóa kỳ lương (Unlock Payroll Period), cho phép kế toán tính lại lương và mở khóa điểm danh.
/// </summary>
public interface IUnlockPayrollPeriodUseCase
{
    Task<bool> ExecuteAsync(long periodId);
}

public class UnlockPayrollPeriodUseCase : IUnlockPayrollPeriodUseCase
{
    private readonly IPayrollPeriodRepository _periodRepo;
    private readonly ITimesheetRepository _timesheetRepo;

    public UnlockPayrollPeriodUseCase(IPayrollPeriodRepository periodRepo, ITimesheetRepository timesheetRepo)
    {
        _periodRepo = periodRepo;
        _timesheetRepo = timesheetRepo;
    }

    public async Task<bool> ExecuteAsync(long periodId)
    {
        var period = await _periodRepo.GetPeriodByIdAsync(periodId);
        if (period == null)
            return false;

        // 1. Mở khóa toàn bộ các bản ghi chấm công của kỳ lương
        await _timesheetRepo.UnlockTimesheetsByPeriodAsync(periodId);

        // 2. Chuyển trạng thái kỳ sang CALCULATED (cho phép tính lại và sửa đổi)
        await _periodRepo.UpdatePeriodStatusAsync(
            periodId, PeriodStatus.CALCULATED, period.TotalGrossAmount, period.TotalNetAmount);

        return true;
    }
}

#endregion

#region 4. GIÁM SÁT BẢNG CÔNG THÁNG (UC13 / UC_HR_06)

/// <summary>
/// Ca sử dụng: Xem tổng hợp bảng công của tất cả nhân viên trong kỳ (UC13).
/// </summary>
public interface IViewTimesheetSummaryUseCase
{
    Task<IEnumerable<Timesheet>> ExecuteAsync(long periodId);
    Task<IEnumerable<Timesheet>> ExecuteByMonthAsync(int month, int year);
}

public class ViewTimesheetSummaryUseCase : IViewTimesheetSummaryUseCase
{
    private readonly ITimesheetRepository _timesheetRepo;

    public ViewTimesheetSummaryUseCase(ITimesheetRepository timesheetRepo)
    {
        _timesheetRepo = timesheetRepo;
    }

    public async Task<IEnumerable<Timesheet>> ExecuteAsync(long periodId)
    {
        return await _timesheetRepo.GetTimesheetsByPeriodAsync(periodId);
    }

    public async Task<IEnumerable<Timesheet>> ExecuteByMonthAsync(int month, int year)
    {
        return await _timesheetRepo.GetAllTimesheetsByMonthAsync(month, year);
    }
}

#endregion

#region 5. XÉT DUYỆT ĐƠN NGHỈ PHÉP (UC12 / UC_HR_06)

/// <summary>
/// Ca sử dụng: Xét duyệt hoặc từ chối đơn xin nghỉ phép của nhân viên (UC12).
/// </summary>
public interface IProcessLeaveRequestUseCase
{
    Task<IEnumerable<LeaveRequest>> GetAllRequestsAsync();
    Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync();
    Task ApproveAsync(long requestId, string reviewer);
    Task RejectAsync(long requestId, string reviewer);
}

public class ProcessLeaveRequestUseCase : IProcessLeaveRequestUseCase
{
    private readonly ILeaveRequestRepository _leaveRepo;

    public ProcessLeaveRequestUseCase(ILeaveRequestRepository leaveRepo)
    {
        _leaveRepo = leaveRepo;
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllRequestsAsync()
    {
        return await _leaveRepo.GetAllLeaveRequestsAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync()
    {
        return await _leaveRepo.GetPendingLeaveRequestsAsync();
    }

    public async Task ApproveAsync(long requestId, string reviewer)
    {
        await _leaveRepo.ApproveOrRejectLeaveRequestAsync(requestId, ApprovalStatus.APPROVED, reviewer);
    }

    public async Task RejectAsync(long requestId, string reviewer)
    {
        await _leaveRepo.ApproveOrRejectLeaveRequestAsync(requestId, ApprovalStatus.REJECTED, reviewer);
    }
}

#endregion

#region 6. XUẤT DANH SÁCH CHUYỂN KHOẢN NGÂN HÀNG (UC17 / UC_HR_10)

/// <summary>
/// DTO chứa thông tin chi trả lương chuyển khoản ngân hàng.
/// </summary>
public record BankTransferRecord(
    long EmployeeId,
    string EmployeeCode,
    string FullName,
    string BankAccountNumber,
    string BankName,
    decimal Amount,
    string Description);

/// <summary>
/// Ca sử dụng: Xuất danh sách chuyển khoản lương ngân hàng (UC17).
/// </summary>
public interface IExportBankTransferUseCase
{
    Task<IEnumerable<BankTransferRecord>> ExecuteAsync(long periodId);
}

public class ExportBankTransferUseCase : IExportBankTransferUseCase
{
    private readonly IPayrollPeriodRepository _periodRepo;
    private readonly IPayslipRepository _payslipRepo;

    public ExportBankTransferUseCase(IPayrollPeriodRepository periodRepo, IPayslipRepository payslipRepo)
    {
        _periodRepo = periodRepo;
        _payslipRepo = payslipRepo;
    }

    public async Task<IEnumerable<BankTransferRecord>> ExecuteAsync(long periodId)
    {
        var period = await _periodRepo.GetPeriodByIdAsync(periodId);
        if (period == null) return Enumerable.Empty<BankTransferRecord>();

        var payslips = await _payslipRepo.GetPayslipsByPeriodIdAsync(periodId);
        return payslips.Select(p => new BankTransferRecord(
            p.EmployeeId,
            p.EmployeeCode ?? string.Empty,
            p.EmployeeName ?? string.Empty,
            p.BankAccountNumber ?? string.Empty,
            p.BankName ?? string.Empty,
            p.NetSalary,
            $"Chi tra luong thang {period.PeriodMonth:D2}/{period.PeriodYear} - {p.EmployeeCode}"
        ));
    }
}

#endregion
