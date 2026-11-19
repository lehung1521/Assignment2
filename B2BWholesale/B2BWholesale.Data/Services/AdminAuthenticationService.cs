using B2BWholesale.Data.Models;

namespace B2BWholesale.Data.Services;

public sealed class AdminAuthenticationService : IAdminAuthenticationService
{
    public const int MaximumFailedAttempts = 3;

    private readonly AppDbContext _dbContext;

    public AdminAuthenticationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public AdminLoginResult AuthenticateAdmin(string? usernameOrEmail, string? password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrEmpty(password))
        {
            return new AdminLoginResult(AdminLoginStatus.MissingCredentials);
        }

        var login = usernameOrEmail.Trim();
        var user = _dbContext.Users.FirstOrDefault(x => x.UserName == login || x.Email == login);
        if (user is null)
        {
            return new AdminLoginResult(AdminLoginStatus.InvalidCredentials);
        }

        if (user.Lock)
        {
            return new AdminLoginResult(AdminLoginStatus.AccountLocked);
        }

        if (!PasswordHashing.VerifySha256(password, user.Password))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaximumFailedAttempts)
            {
                user.Lock = true;
                _dbContext.SaveChanges();
                return new AdminLoginResult(AdminLoginStatus.AccountLocked);
            }

            _dbContext.SaveChanges();
            return new AdminLoginResult(
                AdminLoginStatus.InvalidCredentials,
                AttemptsRemaining: MaximumFailedAttempts - user.FailedLoginCount);
        }

        if (user.FailedLoginCount > 0)
        {
            user.FailedLoginCount = 0;
            _dbContext.SaveChanges();
        }

        if (!string.Equals(user.Role, "Admin", StringComparison.Ordinal))
        {
            return new AdminLoginResult(AdminLoginStatus.AgentAccessDenied);
        }

        return new AdminLoginResult(AdminLoginStatus.Success, user);
    }
}
