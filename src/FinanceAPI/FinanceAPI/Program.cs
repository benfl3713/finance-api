using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseWebRoot("wwwroot");
					webBuilder.UseElectron(args);
					webBuilder.UseStartup<Startup>();
				}).
			ConfigureAppConfiguration(configurationBuilder => {
				configurationBuilder.AddEnvironmentVariables();
				if (System.IO.File.Exists("user.appsettings.json"))
					configurationBuilder.AddJsonFile("user.appsettings.json");
			});
	}
}
