using System;
using CoreForms.Web.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Remotion.ObjectBinding.Web.Development.WebTesting.TestSite
{
  public class Program
  {
    public static void Main (string[] args)
    {
      var legacyAspNetInitializationOptions = new LegacyAspNetInitializationOptions("/", Environment.CurrentDirectory);
      LegacyAspNetInitialization.Initialize(legacyAspNetInitializationOptions);

      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder (string[] args)
    {
      return Host.CreateDefaultBuilder(args)
          .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
  }
}
