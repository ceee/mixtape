using PowCapServer;

namespace Mixtape.Security;

public class CaptchaOptions : PowCapConfig
{
  public string Endpoint { get; set; } = "/api/captcha";

  public bool Enabled { get; set; } = true;

  public CaptchaLocalizationOptions Localization { get; set; } = CaptchaLocalizationOptions.German;
}


public class CaptchaLocalizationOptions : Dictionary<string, string>
{
  public static CaptchaLocalizationOptions English { get; } = new()
  {
    //["initial-state"] = "Verify you're human"
    // ["verifying-label"] = "Verifying...",
    // ["solved-label"] = "You're human",
    // ["error-label"] = "Error",
    // ["troubleshooting-label"] = "Troubleshooting",
    // ["wasm-disabled"] = "Enable WASM for significantly faster solving",
    // ["verify-aria-label"] = "Click to verify you're a human",
    // ["verifying-aria-label"] = "Verifying, please wait",
    // ["verified-aria-label"] = "Verified",
    // ["error-aria-label"] = "An error occurred, please try again",
  };

  public static CaptchaLocalizationOptions German { get; } = new()
  {
    ["initial-state"] = "Bestätigen, dass Sie kein Bot sind"
    // ["verifying-label"] = "Wird überprüft...",
    // ["solved-label"] = "Erfolgreich",
    // ["error-label"] = "Fehler",
    // ["troubleshooting-label"] = "Fehlerbehebung",
    // ["wasm-disabled"] = "Aktivieren Sie WASM für eine deutlich schnellere Lösung",
    // ["verify-aria-label"] = "Klicken, um zu bestätigen, dass Sie ein Mensch bist",
    // ["verifying-aria-label"] = "Wird überprüft, bitte warten",
    // ["verified-aria-label"] = "Bestätigt",
    // ["error-aria-label"] = "Ein Fehler ist aufgetreten, bitte versuchen Sie es erneut",
  };
}