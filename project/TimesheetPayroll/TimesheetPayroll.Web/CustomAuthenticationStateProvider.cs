using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.Web;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IUserAccountRepository _userRepository;
    private ClaimsPrincipal _currentPrincipal = new(new ClaimsIdentity());
    private UserAccount? _currentUser;

    public CustomAuthenticationStateProvider(IUserAccountRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public UserAccount? CurrentUser => _currentUser;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentPrincipal));
    }

    public async Task<bool> SignInAsync(UserAccount user)
    {
        if (user == null || !user.IsActive)
            return false;

        _currentUser = user;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new(ClaimTypes.GivenName, user.FullName ?? user.Username),
            new("EmployeeId", user.EmployeeId.ToString()),
            new("EmployeeCode", user.EmployeeCode ?? string.Empty),
            new("DepartmentName", user.DepartmentName ?? string.Empty),
            new("Email", user.Email ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, "CustomAuthType");
        _currentPrincipal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return true;
    }

    public void SignOut()
    {
        _currentUser = null;
        _currentPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// Đăng nhập mặc định nếu phiên ban đầu chưa có để người dùng trải nghiệm ngay lập tức.
    /// </summary>
    public async Task EnsureInitialUserAsync()
    {
        if (_currentUser == null)
        {
            var defaultUser = await _userRepository.GetByUsernameAsync("nhanvien.a");
            if (defaultUser != null)
            {
                await SignInAsync(defaultUser);
            }
        }
    }
}
