using Mixtape.Mails.Dispatchers;
using Mixtape.Mails.Dispatchers.Postmark;
using Mixtape.Mails.Dispatchers.Scaleway;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mixtape.Mails.Dispatchers.Lettermint;

namespace Mixtape.Mails;

internal class MixtapeMailModule : MixtapeModule
{
  public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddHttpClient<ScalewayDispatcher>().RemoveAllLoggers();
    services.AddHttpClient<LettermintDispatcher>().RemoveAllLoggers();

    services.AddScoped<IMailProvider, MailProvider>();

    // use logger mail dispatcher as default implementation
    // to use other dispatchers .AddMailDispatcher() should be used
    services.AddScoped<IMailDispatcher, LoggerMailDispatcher>();

    services.AddOptions<MailOptions>().Bind(configuration.GetSection("Mixtape:Mails")).Configure(opts =>
    {
      opts.BuildViewPath = mail => $"~/Mails/{mail.ViewKey.Replace('.', '/')}.cshtml";
    });
  }
}