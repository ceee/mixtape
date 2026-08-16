using Microsoft.AspNetCore.Http;

namespace Mixtape.Context
{
  public class MixtapeContextMiddleware(RequestDelegate next)
  {
    public async Task Invoke(HttpContext httpContext, IMixtapeContext mixtapeContext, ICultureResolver cultureResolver)
    {
      // resolve mixtape context
      await mixtapeContext.Resolve(httpContext);
      
      // reset current culture on execution thread
      cultureResolver.Set(cultureResolver.Current);
      
      await next(httpContext);
    }
  }
}
