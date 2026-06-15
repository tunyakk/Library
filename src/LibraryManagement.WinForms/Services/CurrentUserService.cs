using LibraryManagement.Application.Abstractions;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.WinForms.Services;

public class CurrentUserService : ICurrentUserService
{
    private int? _userId;
    private string? _username;
    private UserRole? _role;

    public int? UserId => _userId;
    public string? Username => _username;
    public UserRole? Role => _role;
    public bool IsAuthenticated => _userId.HasValue;

    public void SignIn(int userId, string username, UserRole role)
    {
        _userId = userId;
        _username = username;
        _role = role;
    }

    public void AutoSignInAsAdmin(int userId)
    {
        _userId = userId;
        _username = "admin";
        _role = UserRole.Administrator;
    }

    public void SignOut()
    {
        _userId = null;
        _username = null;
        _role = null;
    }
}
