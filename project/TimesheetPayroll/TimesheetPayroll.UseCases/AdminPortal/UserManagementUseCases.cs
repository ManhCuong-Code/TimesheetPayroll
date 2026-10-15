using TimesheetPayroll.CoreBusiness.Exceptions;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.AdminPortal;

#region 1. CẤP TÀI KHOẢN MỚI CHO NHÂN VIÊN (UC_ADMIN_01)

public interface ICreateUserAccountUseCase
{
    Task<long> ExecuteAsync(UserAccount account, string plainPassword);
}

public class CreateUserAccountUseCase : ICreateUserAccountUseCase
{
    private readonly IUserAccountRepository _userRepo;
    private readonly IEmployeeRepository _empRepo;

    public CreateUserAccountUseCase(IUserAccountRepository userRepo, IEmployeeRepository empRepo)
    {
        _userRepo = userRepo;
        _empRepo = empRepo;
    }

    public async Task<long> ExecuteAsync(UserAccount account, string plainPassword)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (account.EmployeeId <= 0)
            throw new BusinessRuleValidationException("Vui lòng chọn nhân viên được cấp tài khoản.", nameof(CreateUserAccountUseCase));

        if (string.IsNullOrWhiteSpace(account.Username))
            throw new BusinessRuleValidationException("Tên đăng nhập không được để trống.", nameof(CreateUserAccountUseCase));

        account.Username = account.Username.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6)
            throw new BusinessRuleValidationException("Mật khẩu phải có ít nhất 6 ký tự.", nameof(CreateUserAccountUseCase));

        // Kiểm tra nhân viên có tồn tại hay không
        var employee = await _empRepo.GetEmployeeByIdAsync(account.EmployeeId);
        if (employee == null)
            throw new BusinessRuleValidationException($"Không tìm thấy hồ sơ nhân viên ID={account.EmployeeId}.", nameof(CreateUserAccountUseCase));

        // Kiểm tra nhân viên đã có tài khoản hay chưa
        var existingForEmp = await _userRepo.GetByEmployeeIdAsync(account.EmployeeId);
        if (existingForEmp != null)
            throw new BusinessRuleValidationException($"Nhân viên '{employee.FullName}' ({employee.EmployeeCode}) đã có tài khoản '{existingForEmp.Username}'. Mỗi nhân viên chỉ được cấp 1 tài khoản duy nhất.", nameof(CreateUserAccountUseCase));

        // Kiểm tra trùng username
        bool usernameTaken = await _userRepo.ExistsUsernameAsync(account.Username);
        if (usernameTaken)
            throw new BusinessRuleValidationException($"Tên đăng nhập '{account.Username}' đã tồn tại trong hệ thống. Vui lòng chọn tên khác.", nameof(CreateUserAccountUseCase));

        return await _userRepo.CreateUserAccountAsync(account, plainPassword);
    }
}

#endregion

#region 2. CHỈNH SỬA TÀI KHOẢN & PHÂN QUYỀN (UC_ADMIN_02)

public interface IUpdateUserAccountUseCase
{
    Task ExecuteAsync(UserAccount account, string? newPlainPassword = null);
}

public class UpdateUserAccountUseCase : IUpdateUserAccountUseCase
{
    private readonly IUserAccountRepository _userRepo;

    public UpdateUserAccountUseCase(IUserAccountRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task ExecuteAsync(UserAccount account, string? newPlainPassword = null)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (account.Id <= 0)
            throw new BusinessRuleValidationException("Mã tài khoản không hợp lệ.", nameof(UpdateUserAccountUseCase));

        if (string.IsNullOrWhiteSpace(account.Username))
            throw new BusinessRuleValidationException("Tên đăng nhập không được để trống.", nameof(UpdateUserAccountUseCase));

        account.Username = account.Username.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(newPlainPassword) && newPlainPassword.Length < 6)
            throw new BusinessRuleValidationException("Mật khẩu mới phải có ít nhất 6 ký tự.", nameof(UpdateUserAccountUseCase));

        // Kiểm tra trùng username với tài khoản khác
        bool usernameTaken = await _userRepo.ExistsUsernameAsync(account.Username, account.Id);
        if (usernameTaken)
            throw new BusinessRuleValidationException($"Tên đăng nhập '{account.Username}' đã được tài khoản khác sử dụng.", nameof(UpdateUserAccountUseCase));

        await _userRepo.UpdateUserAccountAsync(account, newPlainPassword);
    }
}

#endregion

#region 3. DANH SÁCH NHÂN VIÊN CHƯA CÓ TÀI KHOẢN

public interface IGetEmployeesWithoutAccountUseCase
{
    Task<IEnumerable<Employee>> ExecuteAsync();
}

public class GetEmployeesWithoutAccountUseCase : IGetEmployeesWithoutAccountUseCase
{
    private readonly IUserAccountRepository _userRepo;

    public GetEmployeesWithoutAccountUseCase(IUserAccountRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IEnumerable<Employee>> ExecuteAsync()
    {
        return await _userRepo.GetEmployeesWithoutAccountAsync();
    }
}

#endregion

#region 4. KHÓA / MỞ KHÓA TÀI KHOẢN NHANH (TOGGLE STATUS)

public interface IToggleUserStatusUseCase
{
    Task ExecuteAsync(long userId);
}

public class ToggleUserStatusUseCase : IToggleUserStatusUseCase
{
    private readonly IUserAccountRepository _userRepo;

    public ToggleUserStatusUseCase(IUserAccountRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task ExecuteAsync(long userId)
    {
        if (userId <= 0) return;
        await _userRepo.ToggleUserStatusAsync(userId);
    }
}

#endregion
