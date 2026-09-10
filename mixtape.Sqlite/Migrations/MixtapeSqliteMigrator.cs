using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.Logging;
using ServiceStack.OrmLite;

namespace Mixtape.Sqlite.Migrations;

/// <summary>
/// Extend built-in migrator so we can:
/// 1. override the console-logger (which is hard-coded into the Migrator)
/// 2. be able to sort assembly types
/// </summary>
public class MixtapeSqliteMigrator : Migrator
{
  public MixtapeSqliteMigrator(IDbConnectionFactory dbFactory, ILogFactory logFactory, params Assembly[] migrationAssemblies) 
    : this(dbFactory, logFactory, [.. ScanAssemblies(migrationAssemblies)]) { }

  public MixtapeSqliteMigrator(IDbConnectionFactory dbFactory, ILogFactory logFactory, params Type[] migrationTypes) : base(dbFactory, migrationTypes)
  {
    Log = logFactory.GetLogger(typeof(MixtapeSqliteMigrator));
    Timeout = TimeSpan.FromSeconds(10);
  }
  
  
  public static List<Type> ScanAssemblies(params Assembly[] migrationAssemblies)
  {
    return [
      .. migrationAssemblies
      .SelectMany(x => x.GetTypes().Where(type => type.IsInstanceOf(typeof(MigrationBase)) && !type.IsAbstract))
      .OrderBy(x => x.Name)
    ];
  }
}