using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.Authentication;

#region 1. ĐĂNG NHẬP HỆ THỐNG & XÁC THỰC BCRYPT (UC01 / UC_AUTH_01)

/// <summary>
/// Ca sử dụng: Xác thực đăng nhập người dùng qua tên đăng nhập và mật khẩu (UC01).
/// Kiểm tra mật khẩu mã hóa BCrypt, trạng thái kích hoạt tài khoản và cập nhật thời điểm đăng nhập.
/// </summary>
public interface ILoginUseCase
{
    Task<UserAccount?> ExecuteAsync(string username, string password);
}

public class LoginUseCase : ILoginUseCase
{
    private readonly IUserAccountRepository _userRepository;

    public LoginUseCase(IUserAccountRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserAccount?> ExecuteAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = await _userRepository.AuthenticateAsync(username.Trim(), password);
        if (user != null && user.IsActive)
        {
            await _userRepository.UpdateLastLoginAsync(user.Id);
        }

        return user;
    }
}

#endregion

#region 2. TRA CỨU DANH SÁCH TÀI KHOẢN HỆ THỐNG (UC07 / UC_ADMIN_01)

/// <summary>
/// Ca sử dụng: Lấy danh sách toàn bộ tài khoản người dùng phục vụ phân quyền RBAC (UC07).
/// </summary>
public interface IGetUserAccountsUseCase
{
    Task<IEnumerable<UserAccount>> ExecuteAsync();
}

public class GetUserAccountsUseCase : IGetUserAccountsUseCase
{
    private readonly IUserAccountRepository _userRepository;

    public GetUserAccountsUseCase(IUserAccountRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserAccount>> ExecuteAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }
}

#endregion
