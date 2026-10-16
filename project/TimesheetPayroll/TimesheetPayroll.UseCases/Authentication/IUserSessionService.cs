using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.UseCases.Authentication;

/// <summary>
/// Cổng giao tiếp phiên người dùng và quản lý trạng thái đăng nhập/đăng xuất (User Session Interface).
/// Tuân thủ Clean Architecture: UI chỉ phụ thuộc interface, không phụ thuộc lớp cài đặt cụ thể AuthenticationStateProvider.
/// </summary>
public interface IUserSessionService
{
    UserAccount? CurrentUser { get; }
    Task<bool> SignInAsync(UserAccount user);
    void SignOut();
    Task EnsureInitialUserAsync();
}
