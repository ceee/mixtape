using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System.Reflection;

namespace Mixtape.Assemblies;

public class AssemblyDiscovery : IAssemblyDiscovery
{
  public static IAssemblyDiscovery Current;

  AssemblyDiscoveryContext Context;

  IMvcBuilder Mvc;



  public AssemblyDiscovery(IMvcBuilder mvc)
  {
    Mvc = mvc;
    Context = new AssemblyDiscoveryContext();
    Current = this;
  }


  /// <inheritdoc />
  public void Execute(IEnumerable<IAssemblyDiscoveryRule> rules)
  {
    List<Assembly> assemblies = [];
    DependencyContext dependencyContext = DependencyContext.Load(Context.EntryAssembly);

    if (dependencyContext == null)
    {
      return;
    }

    string[] existingLibs = Mvc.PartManager.ApplicationParts.OfType<AssemblyPart>().Select(p => p.Name).ToArray();

    IEnumerable<RuntimeLibrary> libraries = dependencyContext.RuntimeLibraries.Where(lib => !existingLibs.Contains(lib.Name, StringComparer.OrdinalIgnoreCase));

    foreach (RuntimeLibrary library in libraries)
    {
      if (rules.Any(rule => rule.IsValid(library, Context)))
      {
        IEnumerable<Assembly> libraryAssemblies = library.GetDefaultAssemblyNames(dependencyContext).Select(name => Assembly.Load(name));

        foreach (Assembly assembly in libraryAssemblies)
        {
          Mvc.AddApplicationPart(assembly);
        }
      }
    }
  }


  /// <inheritdoc />
  public void AddAssembly(Assembly assembly)
  {
    string[] existingLibs = Mvc.PartManager.ApplicationParts.OfType<AssemblyPart>().Select(p => p.Name).ToArray();

    if (!existingLibs.Contains(assembly.GetName().Name))
    {
      Mvc.AddApplicationPart(assembly);
    }
  }


  /// <inheritdoc />
  public IEnumerable<TypeInfo> GetTypes<TService>(bool allowGenerics = false) => GetTypes(typeof(TService), allowGenerics);

  /// <inheritdoc />
  public IEnumerable<TypeInfo> GetTypes<TService>(IEnumerable<ApplicationPart> parts, bool allowGenerics = false) => GetTypes(typeof(TService), parts, allowGenerics);

  /// <inheritdoc />
  public IEnumerable<TypeInfo> GetTypes(Type serviceType, bool allowGenerics = false) => GetAllClassTypes(allowGenerics).Where(t => serviceType.GetTypeInfo().IsAssignableFrom(t) && t.AsType() != serviceType);

  /// <inheritdoc />
  public IEnumerable<TypeInfo> GetTypes(Type serviceType, IEnumerable<ApplicationPart> parts, bool allowGenerics = false) => GetAllClassTypes(parts, allowGenerics).Where(t => serviceType.GetTypeInfo().IsAssignableFrom(t) && t.AsType() != serviceType);

  /// <inheritdoc />
  public IEnumerable<TypeInfo> GetAllClassTypes(bool allowGenerics = false) => GetAllTypes().Where(t => t.IsClass && !t.IsAbstract && (allowGenerics || !t.ContainsGenericParameters));

  /// <inheritdoc />
  public IEnumerable<TypeInfo> GetAllClassTypes(IEnumerable<ApplicationPart> parts, bool allowGenerics = false) => GetAllTypes(parts).Where(t => t.IsClass && !t.IsAbstract && (allowGenerics || !t.ContainsGenericParameters));

  /// <inheritdoc />
  public IEnumerable<Assembly> GetAssemblies() => Mvc.PartManager.ApplicationParts.OfType<AssemblyPart>().Select(p => p.Assembly);

  /// <inheritdoc />
  public IEnumerable<Assembly> GetAssemblies(IEnumerable<ApplicationPart> parts) => parts.OfType<AssemblyPart>().Select(p => p.Assembly);

  /// <inheritdoc />
  public IEnumerable<TypeInfo> GetAllTypes() => Mvc.PartManager.ApplicationParts.OfType<AssemblyPart>().SelectMany(p => p.Types);

  /// <inheritdoc />
  public IEnumerable<TypeInfo> GetAllTypes(IEnumerable<ApplicationPart> parts) => parts.OfType<AssemblyPart>().SelectMany(p => p.Types);
}


public interface IAssemblyDiscovery
{
  /// <summary>
  /// Discovers runtime assemblies based on the given rules
  /// </summary>
  void Execute(IEnumerable<IAssemblyDiscoveryRule> rules);

  /// <summary>
  /// Manually add an assembly
  /// </summary>
  void AddAssembly(Assembly assembly);

  /// <summary>
  /// Get all discovered types which implement a certain service
  /// </summary>
  IEnumerable<TypeInfo> GetTypes<TService>(bool allowGenerics = false);

  /// <summary>
  /// Get all discovered types which implement a certain service
  /// </summary>
  IEnumerable<TypeInfo> GetTypes<TService>(IEnumerable<ApplicationPart> parts, bool allowGenerics = false);

  /// <summary>
  /// Get all discovered types which implement a certain service
  /// </summary>
  IEnumerable<TypeInfo> GetTypes(Type serviceType, bool allowGenerics = false);

  /// <summary>
  /// Get all discovered types which implement a certain service
  /// </summary>
  IEnumerable<TypeInfo> GetTypes(Type serviceType, IEnumerable<ApplicationPart> parts, bool allowGenerics = false);

  /// <summary>
  /// Get all registered types
  /// </summary>
  IEnumerable<TypeInfo> GetAllTypes();

  /// <summary>
  /// Get all registered types
  /// </summary>
  IEnumerable<TypeInfo> GetAllTypes(IEnumerable<ApplicationPart> parts);

  /// <summary>
  /// Get all registered types
  /// </summary>
  IEnumerable<TypeInfo> GetAllClassTypes(bool allowGenerics = false);

  /// <summary>
  /// Get all registered types
  /// </summary>
  IEnumerable<TypeInfo> GetAllClassTypes(IEnumerable<ApplicationPart> parts, bool allowGenerics = false);

  /// <summary>
  /// Get all registered assemblies
  /// </summary>
  IEnumerable<Assembly> GetAssemblies();

  /// <summary>
  /// Get all registered assemblies
  /// </summary>
  IEnumerable<Assembly> GetAssemblies(IEnumerable<ApplicationPart> parts);
}
