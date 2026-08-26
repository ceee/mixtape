using System;
using Microsoft.Extensions.Logging;

namespace Mixtape.Sqlite;

/// <summary>
/// This attribute will specify how to log updates to this table
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
public class SqliteLoggingAttribute(LogLevel logLevel) : Attribute
{
  public LogLevel LogLevel { get; set; } = logLevel;
}
