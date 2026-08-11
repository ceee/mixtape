using Microsoft.AspNetCore.Identity;

namespace Mixtape.Identity;

public partial class MixtapeUserStore<TUser, TRole>(IMixtapeIdentityStoreDbProvider db) : MixtapeUserStore<TUser>(db),
  IUserRoleStore<TUser>
  where TUser : MixtapeIdentityUser, new()
  where TRole : MixtapeIdentityRole, new()
{
  /// <inheritdoc />
  public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
  {
    user.RoleIds.Add(roleName);
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
  {
    return Task.FromResult((IList<string>)user.RoleIds.ToList());
  }


  /// <inheritdoc />
  public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
  {
    return await Db.FindAll<TUser>(x => !x.IsDeleted && x.RoleIds.Contains(roleName), cancellationToken);
  }


  /// <inheritdoc />
  public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.RoleIds.Contains(roleName, StringComparer.InvariantCultureIgnoreCase));
  }


  /// <inheritdoc />
  public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
  {
    user.RoleIds.Remove(roleName);
    return Task.CompletedTask;
  }
}
