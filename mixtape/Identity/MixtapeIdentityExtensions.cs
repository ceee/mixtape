using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Mixtape.Identity;

public static class MixtapeIdentityExtensions
{
  // <summary>
  /// Adds the default identity system configuration for the specified User and Role types.
  /// </summary>
  /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
  /// <typeparam name="TRole">The type representing a Role in the system.</typeparam>
  /// <param name="services">The services available in the application.</param>
  /// <returns>An <see cref="IdentityBuilder"/> for creating and configuring the identity system.</returns>
  public static IdentityBuilder AddMixtapeIdentity<TUser, TRole>(this IServiceCollection services)
    where TUser : MixtapeIdentityUser, new()
    where TRole : MixtapeIdentityRole, new() =>
    AddMixtapeIdentity<TUser, TRole>(services, null);

  
  /// <summary>
  /// Adds and configures the identity system for the specified User and Role types.
  /// </summary>
  /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
  /// <typeparam name="TRole">The type representing a Role in the system.</typeparam>
  /// <param name="services">The services available in the application.</param>
  /// <param name="setupAction">An action to configure the <see cref="IdentityOptions"/>.</param>
  /// <returns>An <see cref="IdentityBuilder"/> for creating and configuring the identity system.</returns>
  public static IdentityBuilder AddMixtapeIdentity<TUser, TRole>(this IServiceCollection services,
    Action<IdentityOptions> setupAction)
    where TUser : MixtapeIdentityUser, new()
    where TRole : MixtapeIdentityRole, new()
  {
    // Services identity depends on
    services
      .AddOptions()
      .AddLogging();
    
    // Services used by identity
    services.AddAuthentication(options =>
      {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
      })
      .AddCookie(IdentityConstants.ApplicationScheme, o =>
      {
        o.LoginPath = new PathString("/account/login");
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromDays(90);
        
        o.Cookie.Name = MixtapeIdentityConstants.CookieNames.Application;
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.Path = "/";
        
        o.Events = new CookieAuthenticationEvents
        {
          OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
        };
      })
      .AddCookie(IdentityConstants.ExternalScheme, o =>
      {
        o.Cookie.Name = MixtapeIdentityConstants.CookieNames.External;
        o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
      })
      .AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
      {
        o.Cookie.Name = MixtapeIdentityConstants.CookieNames.TwoFactorRememberMe;
        o.Events = new CookieAuthenticationEvents
        {
          OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
        };
      })
      .AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
      {
        o.Cookie.Name = MixtapeIdentityConstants.CookieNames.TwoFactorUserId;
        o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
      });

    // Hosting doesn't add IHttpContextAccessor by default
    services.AddHttpContextAccessor();
    services.AddMetrics();
    
    // Identity services
    services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
    services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
    services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
    services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
    services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
    
    // No interface for the error describer so we can add errors without rev'ing the interface
    services.TryAddScoped<IdentityErrorDescriber>();
    services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
    services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<TUser>>();
    services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
    services.TryAddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();
    services.TryAddScoped<IPasskeyHandler<TUser>, PasskeyHandler<TUser>>();
    services.TryAddScoped<UserManager<TUser>>();
    services.TryAddScoped<SignInManager<TUser>>();
    services.TryAddScoped<RoleManager<TRole>>();

    services.Configure<IdentityOptions>(opts =>
    {
      opts.ClaimsIdentity.UserIdClaimType = MixtapeIdentityConstants.Claims.UserId;
      opts.ClaimsIdentity.UserNameClaimType = MixtapeIdentityConstants.Claims.Username;
      opts.ClaimsIdentity.RoleClaimType = MixtapeIdentityConstants.Claims.Role;
      opts.ClaimsIdentity.SecurityStampClaimType = MixtapeIdentityConstants.Claims.SecurityStamp;
      opts.ClaimsIdentity.EmailClaimType = MixtapeIdentityConstants.Claims.Email;

      opts.Password.RequireDigit = false;
      opts.Password.RequireLowercase = false;
      opts.Password.RequireUppercase = false;
      opts.Password.RequireNonAlphanumeric = false;
      opts.Password.RequiredLength = 8;
      opts.Password.RequiredUniqueChars = 1;

      opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
      opts.Lockout.MaxFailedAccessAttempts = 5;
      opts.Lockout.AllowedForNewUsers = true;

      opts.User.RequireUniqueEmail = true;
    });

    services.Configure<IdentityPasskeyOptions>(opts =>
    {
      // options.ServerDomain = builder.Configuration["Passkeys:ServerDomain"];
      // options.AuthenticatorTimeout = TimeSpan.FromMinutes(3);
      // options.ChallengeSize = 32;
      // options.UserVerificationRequirement = "preferred";
      // options.ResidentKeyRequirement = "required";
    });
    
    services.Configure<SecurityStampValidatorOptions>(opts => { opts.ValidationInterval = TimeSpan.FromMinutes(90); });

    if (setupAction != null)
    {
      services.Configure(setupAction);
    }

    IdentityBuilder builder = new(typeof(TUser), typeof(TRole), services);

    builder.AddDefaultTokenProviders();
    builder.AddMixtapeIdentityStores();

    return builder;
  }
  
  
  /// <summary>
  /// Configures the application cookie.
  /// </summary>
  /// <param name="services">The services available in the application.</param>
  /// <param name="configure">An action to configure the <see cref="CookieAuthenticationOptions"/>.</param>
  /// <returns>The services.</returns>
  public static IServiceCollection ConfigureMixtapeApplicationCookie(this IServiceCollection services, Action<CookieAuthenticationOptions> configure)
    => services.Configure(IdentityConstants.ApplicationScheme, configure);

  /// <summary>
  /// Configure the external cookie.
  /// </summary>
  /// <param name="services">The services available in the application.</param>
  /// <param name="configure">An action to configure the <see cref="CookieAuthenticationOptions"/>.</param>
  /// <returns>The services.</returns>
  public static IServiceCollection ConfigureMixtapeExternalCookie(this IServiceCollection services, Action<CookieAuthenticationOptions> configure)
    => services.Configure(IdentityConstants.ExternalScheme, configure);
}