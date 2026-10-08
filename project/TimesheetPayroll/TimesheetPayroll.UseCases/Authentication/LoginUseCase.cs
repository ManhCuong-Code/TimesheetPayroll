using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.Authentication;

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
        if (user != null)
        {
            await _userRepository.UpdateLastLoginAsync(user.Id);
        }

        return user;
    }
}

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
