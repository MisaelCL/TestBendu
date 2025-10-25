using System;
using C_C.App.Model;

namespace C_C.App.Services;

public class UserSession
{
    public UserModel? CurrentUser { get; private set; }

    public event EventHandler<UserModel?>? CurrentUserChanged;

    public void SetCurrentUser(UserModel? user)
    {
        CurrentUser = user;
        CurrentUserChanged?.Invoke(this, user);
    }
}
