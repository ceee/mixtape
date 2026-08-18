using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mixtape.Models;


public class ConfigureFlavorJsonOptions(IMixtapeOptions mixtapeOptions) : IConfigureOptions<JsonOptions>
{
  public void Configure(JsonOptions options)
  {
    options.JsonSerializerOptions.Converters.Add(new JsonFlavorVariantConverterFactory(mixtapeOptions));
  }
}