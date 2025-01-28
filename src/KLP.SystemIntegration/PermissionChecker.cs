public static class PermissionChecker
{
    public static bool IsSuperAdmin()
    {
        return Environnement.UserName == "root";
    }
}