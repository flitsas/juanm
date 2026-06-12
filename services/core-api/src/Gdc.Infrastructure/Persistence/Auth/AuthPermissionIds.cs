namespace Gdc.Infrastructure.Persistence.Auth;

public static class AuthPermissionIds
{
    public static readonly Guid UsersRead = Guid.Parse("44444444-4444-4444-8444-444444444401");
    public static readonly Guid UsersWrite = Guid.Parse("44444444-4444-4444-8444-444444444402");
    public static readonly Guid RbacManage = Guid.Parse("44444444-4444-4444-8444-444444444403");
    public static readonly Guid ReglasRead = Guid.Parse("44444444-4444-4444-8444-444444444404");
    public static readonly Guid ReglasWrite = Guid.Parse("44444444-4444-4444-8444-444444444405");
    public static readonly Guid ReglasExecute = Guid.Parse("44444444-4444-4444-8444-444444444406");

    public static readonly IReadOnlyCollection<Guid> All =
        [UsersRead, UsersWrite, RbacManage, ReglasRead, ReglasWrite, ReglasExecute];
}
