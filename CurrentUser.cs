using Microsoft.Maui.Storage;

public static class CurrentUser
{
    private const string UserIdKey = "CurrentUserId";
    private const string IsAdminKey = "CurrentUserIsAdmin";

    public static string UserId
    {
        get => Preferences.Get(UserIdKey, string.Empty);
        set => Preferences.Set(UserIdKey, value);
    }

    public static bool IsAdmin
    {
        get => Preferences.Get(IsAdminKey, false);
        set => Preferences.Set(IsAdminKey, value);
    }

    public static bool UserExists()
    {
        return !string.IsNullOrEmpty(UserId);
    }

    public static void SetUser(string userId, bool isAdmin)
    {
        UserId = userId;
        IsAdmin = isAdmin;
    }

    public static void Clear()
    {
        Preferences.Remove(UserIdKey);
        Preferences.Remove(IsAdminKey);
    }
}
