using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.Authentication;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.Web;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IUserSessionService
{
    private readonly IUserAccountRepository _userRepository;
    private ClaimsPrincipal _currentPrincipal = new(new ClaimsIdentity());
    private UserAccount? _currentUser;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public CustomAuthenticationStateProvider(IUserAccountRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public UserAccount? CurrentUser => _currentUser;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_initialized)
        {
            await _initLock.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    await InitializeDefaultUserInternalAsync();
                    _initialized = true;
                }
            }
            finally
            {
                _initLock.Release();
            }
        }

        return new AuthenticationState(_currentPrincipal);
    }

    private async Task InitializeDefaultUserInternalAsync()
    {
        if (_currentUser != null) return;

        UserAccount? defaultUser = null;
        try
        {
            defaultUser = await _userRepository.GetByUsernameAsync("nhanvien.a");
        }
        catch
        {
            // Dự phòng tài khoản mẫu nếu SQL Server chưa sẵn sàng
        }

        defaultUser ??= new UserAccount
        {
            Id = 1,
            EmployeeId = 1,
            Username = "nhanvien.a",
            FullName = "Nguyễn Văn A",
            Role = "ROLE_EMPLOYEE",
            EmployeeCode = "EMP001",
            DepartmentName = "Phòng Công Nghệ Thông Tin",
            IsActive = true
        };

        SetPrincipalInternal(defaultUser);
    }

    private void SetPrincipalInternal(UserAccount user)
    {
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
    }

    public async Task<bool> SignInAsync(UserAccount user)
    {
        if (user == null || !user.IsActive)
            return false;

        _initialized = true;
        SetPrincipalInternal(user);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentPrincipal)));
        return true;
    }

    public void SignOut()
    {
        _initialized = true;
        _currentUser = null;
        _currentPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentPrincipal)));
    }

    public async Task EnsureInitialUserAsync()
    {
        if (_currentUser == null)
        {
            await GetAuthenticationStateAsync();
        }
    }
}
