namespace Mixtape.Models;

public class UrlResult
{
  public string Url { get; set; }

  public string Domain { get; set; }

  public UrlResult(string domain, string url)
  {
    Domain = domain;
    Url = url;
  }

  public UrlResult(Uri domain, string url)
  {
    if (domain != null)
    {
      Domain = domain.Scheme + "://" + domain.Authority.TrimEnd("/");
    }
    Url = url;
  }
}

public class UrlsResult
{
  public string[] Urls { get; set; } = [];

  public string Domain { get; set; }

  public UrlsResult(string domain, params string[] urls)
  {
    Domain = domain;
    Urls = urls.Where(x => x.HasValue()).ToArray();
  }

  public UrlsResult(Uri domain, params string[] urls)
  {
    if (domain != null)
    {
      Domain = domain.Scheme + "://" + domain.Authority.TrimEnd("/");
    }
    Urls = urls.Where(x => x.HasValue()).ToArray();
  }
}


public class PreviewUrlResult
{
  public string Url { get; set; }

  public PreviewUrlResult(string url)
  {
    Url = url;
  }
}