using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Mixtape.Context;

public sealed class MixtapeContext(IOptionsSnapshot<MixtapeOptions> options, ICultureResolver cultureResolver, IServiceProvider services) : IMixtapeContext
{
  /// <inheritdoc />
  public IMixtapeOptions Options { get; } = options.Value;

  /// <inheritdoc />
  public IServiceProvider Services { get; } = services;
  
  bool _resolved = false;


  /// <inheritdoc />
  public Task Resolve(HttpContext context)
  {
    if (_resolved)
    {
      return Task.CompletedTask;
    }

    // set current culture
    cultureResolver.Resolve(Options.Language);
    
    _resolved = true;
    return Task.CompletedTask;
  }
}



public interface IMixtapeContext
{
  /// <summary>
  /// Global mixtape options
  /// </summary>
  IMixtapeOptions Options { get; }

  /// <summary>
  /// Service container
  /// </summary>
  IServiceProvider Services { get; }

  /// <summary>
  /// Resolves the current application (for backoffice + frontend requests) and
  /// the currently active backoffice user, as users are not signed in with the default scheme and do therefore not populate HttpContext.User
  /// </summary>
  Task Resolve(HttpContext context);
}