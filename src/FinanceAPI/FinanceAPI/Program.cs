using System;
using System.IO;
using ElectronNET.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;


namespace FinanceAPI
{
	public class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				Log.Information("Starting Up");
				CreateHostBuilder(args).Build().Run();
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
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
					Console.WriteLine("Loading Config...");
					if (System.IO.File.Exists("user.appsettings.json"))
						configurationBuilder.AddJsonFile("user.appsettings.json");

					if (Directory.Exists("config"))
					{
						Log.Information("Found config folder. Loading all config files matching pattern: *appsettings.json");
						foreach (var file in Directory.GetFiles("config", "*appsettings.json"))
						{
							configurationBuilder.AddJsonFile(file);
							Log.Information($"Loaded config from {file}");
						}
					}
					
					configurationBuilder.AddEnvironmentVariables();
				});
	}
}
