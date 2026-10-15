using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.EmployeePortal;

#region 1. LẤY LỊCH SỬ PHIẾU LƯƠNG CỦA NHÂN VIÊN (Anti-IDOR)

public interface IGetEmployeePayslipHistoryUseCase
{
    Task<IEnumerable<Payslip>> ExecuteAsync(long employeeId);
}

public class GetEmployeePayslipHistoryUseCase : IGetEmployeePayslipHistoryUseCase
{
    private readonly IPayslipRepository _payslipRepo;

    public GetEmployeePayslipHistoryUseCase(IPayslipRepository payslipRepo)
    {
        _payslipRepo = payslipRepo;
    }

    public async Task<IEnumerable<Payslip>> ExecuteAsync(long employeeId)
    {
        if (employeeId <= 0) return Enumerable.Empty<Payslip>();
        return await _payslipRepo.GetPayslipsByEmployeeAsync(employeeId);
    }
}

#endregion

#region 2. ĐỐI SOÁT & SO SÁNH HAI KỲ LƯƠNG (UC19 / UC_EMP_05)

public interface ICompareSalaryUseCase
{
    Task<SalaryComparisonDiff?> ExecuteAsync(long employeeId, long periodIdA, long periodIdB);
}

public class CompareSalaryUseCase : ICompareSalaryUseCase
{
    private readonly IPayslipRepository _payslipRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ISalaryAiAssistantService _aiService;

    public CompareSalaryUseCase(
        IPayslipRepository payslipRepo, 
        IEmployeeRepository employeeRepo, 
        ISalaryAiAssistantService aiService)
    {
        _payslipRepo = payslipRepo;
        _employeeRepo = employeeRepo;
        _aiService = aiService;
    }

    public async Task<SalaryComparisonDiff?> ExecuteAsync(long employeeId, long periodIdA, long periodIdB)
    {
        if (employeeId <= 0 || periodIdA <= 0 || periodIdB <= 0) return null;

        // Cơ chế bảo mật Ownership-based Authorization (Chống lộ lọt IDOR)
        var payslipA = await _payslipRepo.GetPayslipByPeriodAndEmployeeAsync(periodIdA, employeeId);
        var payslipB = await _payslipRepo.GetPayslipByPeriodAndEmployeeAsync(periodIdB, employeeId);

        if (payslipA == null || payslipB == null) return null;

        var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
        return _aiService.ComparePayslips(payslipA, payslipB, employee);
    }
}

#endregion

#region 3. HỎI ĐÁP VỚI TRỢ LÝ LƯƠNG AI (Conversational AI Assistant)

public interface IAskSalaryAiUseCase
{
    Task<AiChatMessage> ExecuteAsync(long employeeId, string question, long currentPeriodId, long? comparePeriodId = null);
}

public class AskSalaryAiUseCase : IAskSalaryAiUseCase
{
    private readonly IPayslipRepository _payslipRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IPayrollPeriodRepository _periodRepo;
    private readonly ISalaryAiAssistantService _aiService;

    public AskSalaryAiUseCase(
        IPayslipRepository payslipRepo, 
        IEmployeeRepository employeeRepo, 
        IPayrollPeriodRepository periodRepo, 
        ISalaryAiAssistantService aiService)
    {
        _payslipRepo = payslipRepo;
        _employeeRepo = employeeRepo;
        _periodRepo = periodRepo;
        _aiService = aiService;
    }

    public async Task<AiChatMessage> ExecuteAsync(
        long employeeId, 
        string question, 
        long currentPeriodId, 
        long? comparePeriodId = null)
    {
        if (employeeId <= 0)
        {
            return new AiChatMessage
            {
                IsUser = false,
                Text = "Yêu cầu không hợp lệ: Không tìm thấy thông tin định danh nhân sự của bạn."
            };
        }

        // 1. Tải thông tin nhân viên
        var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

        // 2. Tải phiếu lương hiện tại (Ownership Check)
        Payslip? currentPayslip = null;
        if (currentPeriodId > 0)
        {
            currentPayslip = await _payslipRepo.GetPayslipByPeriodAndEmployeeAsync(currentPeriodId, employeeId);
        }

        // 3. Tải phiếu lương kỳ so sánh (hoặc tự động tìm kỳ trước đó nếu không chỉ định)
        Payslip? comparePayslip = null;
        if (comparePeriodId.HasValue && comparePeriodId.Value > 0)
        {
            comparePayslip = await _payslipRepo.GetPayslipByPeriodAndEmployeeAsync(comparePeriodId.Value, employeeId);
        }
        else
        {
            // Tự động tìm kỳ liền trước
            var allPayslips = await _payslipRepo.GetPayslipsByEmployeeAsync(employeeId);
            var sorted = allPayslips.OrderByDescending(p => p.PeriodId).ToList();
            if (sorted.Count > 1)
            {
                comparePayslip = sorted.FirstOrDefault(p => p.PeriodId != currentPeriodId);
            }
        }

        // 4. Tải danh sách kỳ lương
        var allPeriods = await _periodRepo.GetPeriodsAsync();

        // 5. Gửi sang Trợ lý AI xử lý
        return await _aiService.AnswerQuestionAsync(question, currentPayslip, comparePayslip, employee, allPeriods);
    }
}

#endregion

#region 4. MÔ PHỎNG DỰ TÍNH LƯƠNG WHAT-IF (Salary Simulator)

public interface ISimulateSalaryUseCase
{
    Task<SalarySimulationResult> ExecuteAsync(long employeeId, SalarySimulationRequest request);
}

public class SimulateSalaryUseCase : ISimulateSalaryUseCase
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IDeductionRateRepository _rateRepo;
    private readonly ISalaryAiAssistantService _aiService;

    public SimulateSalaryUseCase(
        IEmployeeRepository employeeRepo, 
        IDeductionRateRepository rateRepo, 
        ISalaryAiAssistantService aiService)
    {
        _employeeRepo = employeeRepo;
        _rateRepo = rateRepo;
        _aiService = aiService;
    }

    public async Task<SalarySimulationResult> ExecuteAsync(long employeeId, SalarySimulationRequest request)
    {
        // Nếu người dùng không nhập lương cơ bản, lấy từ hồ sơ thực tế của nhân viên
        if (request.BaseSalary <= 0 && employeeId > 0)
        {
            var emp = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
            if (emp != null && emp.BaseSalary > 0)
            {
                request.BaseSalary = emp.BaseSalary;
                if (request.MealAllowance <= 0) request.MealAllowance = emp.MealAllowance;
                if (request.FuelAllowance <= 0) request.FuelAllowance = emp.FuelAllowance;
            }
        }

        var rates = await _rateRepo.GetActiveRatesAsync();
        return _aiService.SimulateSalary(request, rates);
    }
}

#endregion
