using System;
using ElectronNET.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;


namespace FinanceAPI
{
	public class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				Log.Information("Starting web host");
				CreateHostBuilder(args).Build().Run();
				return 0;
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host terminated unexpectedly");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog()
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseWebRoot("wwwroot");
					webBuilder.UseElectron(args);
					webBuilder.UseStartup<Startup>();
				})
				.ConfigureAppConfiguration(configurationBuilder => {
					if (System.IO.File.Exists("user.appsettings.json"))
						configurationBuilder.AddJsonFile("user.appsettings.json");
					
					configurationBuilder.AddEnvironmentVariables();
				});
	}
}
