namespace Mixtape.Localization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
public class LocalizeAttribute(string key) : Attribute
{
  public readonly string Key = key;
}
