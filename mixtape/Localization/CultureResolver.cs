using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;

namespace Mixtape.Localization;

public class CultureResolver : ICultureResolver
{
  /// <inheritdoc />
  public CultureInfo Current { get; protected set; }
  
  /// <inheritdoc />
  public string CurrentLanguageCode { get; protected set; }

  protected ILogger<CultureResolver> Logger { get; }

  protected IMessageAggregator MessageAggregator { get; }

  private string _currentLocaleCode;


  public CultureResolver(ILogger<CultureResolver> logger, IMessageAggregator messageAggregator, IOptionsMonitor<MixtapeOptions> mixtapeOptions)
  {
    Logger = logger;
    MessageAggregator = messageAggregator;

    // update culture in case the language (found in options) changes 
    mixtapeOptions.OnChange(options =>
    {
      if (options.Language != _currentLocaleCode)
      {
        Resolve(options.Language);
      }
    });
  }


  /// <inheritdoc />
  public CultureInfo Resolve(string localeCode)
  {
    _currentLocaleCode = localeCode;
    
    if (!TryConvert(localeCode, out CultureInfo culture))
    {
      culture = CultureInfo.CurrentCulture;
    }

    Set(culture);
    
    Logger.LogTrace("Culture resolved: {culture}", Current);
    
    return culture;
  }


  /// <inheritdoc />
  public bool TryConvert(string isoCode, out CultureInfo culture)
  {
    try
    {
      culture = CultureInfo.CreateSpecificCulture(isoCode.Replace('_', '-'));

      if (culture.ThreeLetterISOLanguageName.IsNullOrEmpty())
      {
        throw new Exception("ThreeLetterISOLanguageName is empty");
      }
      if (culture.ThreeLetterISOLanguageName == "ivl")
      {
        throw new Exception("Invariant language is not allowed");
      }

      return true;
    }
    catch (Exception ex)
    {
      Logger.LogWarning(ex, "Could not create culture from Language code {code}", isoCode);
      culture = null;
      return false;
    }
  }


  /// <inheritdoc />
  public void Set(CultureInfo culture)
  {
    CultureInfo.CurrentCulture = culture;
    CultureInfo.CurrentUICulture = culture;
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
    ValidatorOptions.Global.LanguageManager.Culture = culture;
    
    Current = culture;
    CurrentLanguageCode = culture.Name.Split(['_', '-'])[0];
    
    MessageAggregator.Publish(new CultureChangeMessage()
    {
      Culture = culture,
      LanguageCode = CurrentLanguageCode
    });
  }


  /// <inheritdoc />
  public void Subscribe(Expression<Func<CultureChangeMessage, Task>> handle)
  {
    MessageAggregator.Subscribe(handle);
  }
}


public interface ICultureResolver
{
  /// <summary>
  /// Current culture
  /// </summary>
  CultureInfo Current { get; }
  
  /// <summary>
  /// 2-letter ISO language code
  /// </summary>
  string CurrentLanguageCode { get; }

  /// <summary>
  /// Resolves the current application from either the backoffice user (in case it is backoffice request)
  /// or the domain (in case it is frontend request).
  /// </summary>
  CultureInfo Resolve(string localeCode);

  /// <summary>
  /// Tries to convert an ISO code to a culture
  /// </summary>
  bool TryConvert(string isoCode, out CultureInfo culture);

  /// <summary>
  /// Set a new culture for this request
  /// </summary>
  void Set(CultureInfo culture);

  /// <summary>
  /// Subscribe to culture change
  /// </summary>
  void Subscribe(Expression<Func<CultureChangeMessage, Task>> handle);
}
