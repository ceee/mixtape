using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Mixtape.Extensions;

public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Adds or overrides an implementation
  /// </summary>
  public static void Replace<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient) 
    where TService : class
    where TImplementation : class, TService
  {
    services.RemoveAll<TService>();
    services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
  }


  /// <summary>
  /// Adds or overrides an implementation
  /// </summary>
  public static void Replace<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory, ServiceLifetime lifetime = ServiceLifetime.Transient)
    where TService : class
    where TImplementation : class, TService
  {
    services.RemoveAll<TService>();
    services.Add(new ServiceDescriptor(typeof(TService), implementationFactory, lifetime));
  }


  /// <summary>
  /// Adds or overrides an implementation
  /// </summary>
  public static void Replace<TService, TOldImplementation, TNewImplementation>(this IServiceCollection services)
    where TService : class
    where TOldImplementation : class, TService
    where TNewImplementation : class, TService
  {
    ServiceDescriptor oldDescriptor = services.FirstOrDefault(x => x.ImplementationType == typeof(TOldImplementation));
    if (oldDescriptor != null)
    {
      services.Remove(oldDescriptor);
      services.Add(new ServiceDescriptor(typeof(TService), typeof(TNewImplementation), oldDescriptor.Lifetime));
    }
  }


  /// <summary>
  /// Removes an implementation
  /// </summary>
  public static void Remove<TService, TImplementation>(this IServiceCollection services)
    where TService : class
    where TImplementation : class, TService
  {
    ServiceDescriptor oldDescriptor = services.FirstOrDefault(x => x.ImplementationType == typeof(TImplementation));
    if (oldDescriptor != null)
    {
      services.Remove(oldDescriptor);
    }
  }
}
