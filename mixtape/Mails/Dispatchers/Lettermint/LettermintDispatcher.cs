using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Attachment = System.Net.Mail.Attachment;

namespace Mixtape.Mails.Dispatchers.Lettermint;

public class LettermintDispatcher : IMailDispatcher
{
  protected MailOptions Options { get; set; }

  protected IWebHostEnvironment Env { get; set; }

  protected HttpClient Http { get; }

  protected JsonSerializerOptions JsonSerializerOptions { get; }

  protected ILogger<LettermintDispatcher> Logger { get; }


  public LettermintDispatcher(IOptionsMonitor<MailOptions> monitor, IWebHostEnvironment env, HttpClient http, ILogger<LettermintDispatcher> logger)
  {
    Options = monitor.CurrentValue;
    Env = env;
    Http = http;
    Http.DefaultRequestHeaders.Add("x-lettermint-token", Options.Lettermint.Token);
    JsonSerializerOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
      Converters = { new JsonStringEnumConverter() }
    };
    Logger = logger;

    monitor.OnChange(opts => Options = opts);
  }


  /// <inheritdoc />
  public Task<bool> IsSenderSupported(string email, CancellationToken token = default)
  {
    return Task.FromResult(true);
  }

  
  /// <inheritdoc />
  public async Task<MailSuppressionReason> GetSuppressionReason(string email, CancellationToken token = default)
  {
    if (Options.Lettermint.TeamToken.IsNullOrEmpty())
    {
      Logger.LogWarning("Lettermint team token is not set. Suppression status cannot be checked.");
      return MailSuppressionReason.None;
    }
    
    // build url
    NameValueCollection query = new()
    {
      { "filter[value]", email },
      { "page[size]", "1" }
    };
    string uri = Options.Lettermint.ApiUrl + $"/v1/suppressions?" + query;

    try
    {
      using HttpRequestMessage request = new(HttpMethod.Get, uri);
      request.Headers.Authorization = new("Bearer", Options.Lettermint.TeamToken);

      using HttpResponseMessage responseMessage = await Http.SendAsync(request, token);
      LettermintResponse.ListSuppressions response = await responseMessage.Content.ReadFromJsonAsync<LettermintResponse.ListSuppressions>(JsonSerializerOptions, token);

      if (!responseMessage.IsSuccessStatusCode)
      {
        throw new($"Could not get suppression status via Lettermint API. Status code: {responseMessage.StatusCode} Message: {response.ErrorMessage}");
      }

      LettermintResponse.ListSuppressionsItem suppression = response.Suppressions.FirstOrDefault();

      // the suppression has to be an email address
      // and match the query email because the API endpoint does partial matching for email address,
      // which can lead to false-positives
      if (suppression is not { Type: LettermintResponse.SuppressionType.Email } || !suppression.Value.Equals(email, StringComparison.OrdinalIgnoreCase))
      {
        return MailSuppressionReason.None;
      }

      Logger.LogDebug("Suppression status {status} for email {email} retrieved from Lettermint API",  suppression.Reason, email);
      return suppression.Reason;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Could not send message via Lettermint API");
    }

    return MailSuppressionReason.None;
  }


  /// <inheritdoc />
  public async Task Send(Mail message, CancellationToken token = default)
  {
    string uri = Options.Lettermint.ApiUrl + $"/v1/send";

    // add app name as tag
    if (message.Metadata.TryGetValue("application", out string value) && !message.Tag.HasValue())
    {
      message.Tag = value;
      message.Metadata.Remove("application");
    }

    LettermintRequest.SendEmail data = new()
    {
      // to addresses
      To = Convert(message.To),
      ReplyTo = Convert(message.ReplyToList),
      Cc = Convert(message.CC),
      Bcc = Convert(message.Bcc),

      // from address
      From = message.From!.ToString(),

      // subject
      Subject = message.Subject,

      // tag/group
      Tag = Safenames.Tag(message.Tag),

      // metadata
      Metadata = message.Metadata,
      Route = Options.Lettermint.Route
    };

    // set attachments
    foreach (Attachment attachment in message.Attachments)
    {
      data.Attachments.Add(Convert(attachment));
    }

    // set body
    if (!message.IsBodyHtml)
    {
      data.Text = message.Body;
    }
    else
    {
      data.Html = message.Body;
    }

    try
    {
      using HttpResponseMessage responseMessage = await Http.PostAsJsonAsync(uri, data, JsonSerializerOptions, token);
      LettermintResponse.SendEmail response = await responseMessage.Content.ReadFromJsonAsync<LettermintResponse.SendEmail>(JsonSerializerOptions, token);

      if (!responseMessage.IsSuccessStatusCode)
      {
        throw new Exception($"Could not send message via Lettermint API. Status code: {responseMessage.StatusCode}, Message: {response.ErrorMessage}");
      }

      Logger.LogDebug("Email {id} sent via Lettermint API", response.MessageId);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Could not send message via Lettermint API");
    }
  }


  /// <inheritdoc />
  public void Dispose() { }


  /// <summary>
  /// Convert a collection of addresses to a Lettermint email addresses
  /// </summary>
  protected string[] Convert(MailAddressCollection addresses)
  {
    return addresses
      .Select(address => address.ToString())
      .ToArray();
  }


  /// <summary>
  /// Convert an attachment to a Lettermint email attachment
  /// </summary>
  protected LettermintRequest.EmailAttachment Convert(Attachment attachment)
  {
    byte[] buffer = new byte[8067];
    using MemoryStream memoryStream = new();
    int count;
    while ((count = attachment.ContentStream.Read(buffer, 0, buffer.Length)) > 0)
    {
      memoryStream.Write(buffer, 0, count);
    }
    string base64String = System.Convert.ToBase64String(memoryStream.ToArray());

    return new()
    {
      Filename = attachment.Name,
      ContentType = attachment.ContentType.MediaType,
      Content = base64String,
      ContentId = attachment.ContentId
    };
  }
}
