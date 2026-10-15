using TimesheetPayroll.CoreBusiness.Exceptions;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.Authentication;

/// <summary>
/// Ca sử dụng: Nhân viên tự đổi mật khẩu tài khoản cá nhân.
/// Yêu cầu xác thực mật khẩu hiện tại (mật khẩu cũ) và nhập mật khẩu mới 2 lần khớp nhau.
/// </summary>
public interface IChangePasswordUseCase
{
    Task<bool> ExecuteAsync(long userId, string currentPassword, string newPassword, string confirmNewPassword);
    Task<bool> ExecuteByUsernameAsync(string username, string currentPassword, string newPassword, string confirmNewPassword);
}

public class ChangePasswordUseCase : IChangePasswordUseCase
{
    private readonly IUserAccountRepository _userRepository;

    public ChangePasswordUseCase(IUserAccountRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> ExecuteAsync(long userId, string currentPassword, string newPassword, string confirmNewPassword)
    {
        if (userId <= 0)
            throw new ArgumentException("Mã tài khoản người dùng không hợp lệ.", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleValidationException("Không tìm thấy thông tin tài khoản người dùng trong hệ thống.", nameof(ChangePasswordUseCase));

        return await ProcessChangePasswordAsync(user, currentPassword, newPassword, confirmNewPassword);
    }

    public async Task<bool> ExecuteByUsernameAsync(string username, string currentPassword, string newPassword, string confirmNewPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Tên đăng nhập không được để trống.", nameof(username));

        var user = await _userRepository.GetByUsernameAsync(username.Trim());
        if (user == null)
            throw new BusinessRuleValidationException($"Không tìm thấy tài khoản người dùng '{username}'.", nameof(ChangePasswordUseCase));

        return await ProcessChangePasswordAsync(user, currentPassword, newPassword, confirmNewPassword);
    }

    private async Task<bool> ProcessChangePasswordAsync(UserAccount user, string currentPassword, string newPassword, string confirmNewPassword)
    {
        if (!user.IsActive)
            throw new BusinessRuleValidationException("Tài khoản của bạn đã bị vô hiệu hóa hoặc tạm khóa. Vui lòng liên hệ Quản trị viên.", nameof(ChangePasswordUseCase));

        // 1. Kiểm tra mật khẩu hiện tại (mật khẩu cũ)
        if (string.IsNullOrWhiteSpace(currentPassword))
            throw new BusinessRuleValidationException("Vui lòng nhập mật khẩu hiện tại (mật khẩu cũ).", nameof(currentPassword));

        // 2. Kiểm tra mật khẩu mới
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new BusinessRuleValidationException("Vui lòng nhập mật khẩu mới.", nameof(newPassword));

        if (newPassword.Length < 6)
            throw new BusinessRuleValidationException("Mật khẩu mới phải có độ dài tối thiểu 6 ký tự để đảm bảo an toàn.", nameof(newPassword));

        // 3. Kiểm tra xác nhận mật khẩu mới (nhập mật khẩu mới 2 lần)
        if (string.IsNullOrWhiteSpace(confirmNewPassword))
            throw new BusinessRuleValidationException("Vui lòng nhập lại mật khẩu mới vào ô xác nhận.", nameof(confirmNewPassword));

        if (!string.Equals(newPassword, confirmNewPassword, StringComparison.Ordinal))
            throw new BusinessRuleValidationException("Mật khẩu mới và mật khẩu xác nhận không trùng khớp. Vui lòng nhập lại chính xác.", nameof(confirmNewPassword));

        // 4. Mật khẩu mới không được trùng với mật khẩu cũ
        if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
            throw new BusinessRuleValidationException("Mật khẩu mới không được trùng với mật khẩu hiện tại.", nameof(newPassword));

        // 5. Xác thực mật khẩu cũ thông qua AuthenticateAsync
        var authUser = await _userRepository.AuthenticateAsync(user.Username, currentPassword);
        if (authUser == null)
            throw new BusinessRuleValidationException("Mật khẩu hiện tại (mật khẩu cũ) không chính xác. Vui lòng kiểm tra lại.", nameof(currentPassword));

        // 6. Cập nhật mật khẩu mới và mã hóa BCrypt an toàn
        await _userRepository.ChangePasswordAsync(user.Id, newPassword);
        return true;
    }
}
