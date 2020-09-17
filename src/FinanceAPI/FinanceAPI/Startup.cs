using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using FinanceAPI.Middleware;
using FinanceAPICore.Tasks;
using FinanceAPIData;
using FinanceAPIData.TaskManagment;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace FinanceAPI
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers().AddNewtonsoftJson(options => options.UseMemberCasing());
			services.Configure<AppSettings>(Configuration);
			services.Configure<TaskSettings>(Configuration.GetSection("TaskSettings"));
			services.Configure<TaskSettings>(settings =>
			{
				settings.TrueLayer_ClientID = Configuration.GetValue<string>(nameof(settings.TrueLayer_ClientID));
				settings.TrueLayer_ClientSecret = Configuration.GetValue<string>(nameof(settings.TrueLayer_ClientSecret));
				settings.TrueLayer_Mode = Configuration.GetValue<string>(nameof(settings.TrueLayer_Mode));
				settings.MongoDB_ConnectionString = Configuration.GetValue<string>(nameof(settings.MongoDB_ConnectionString)) ?? "mongodb://localhost";
			});
			

			SetupLogging(services);
			
			AddProcessors(services);
			services.AddTransient<JwtMiddleware>();
			services.AddSingleton(x => new TransactionLogoCalculator(x.GetRequiredService<IOptions<AppSettings>>().Value.MongoDB_ConnectionString, x.GetRequiredService<IOptions<AppSettings>>().Value.LogoOverrides, true));
			services.AddSingleton(tp => new TaskPoller(tp.GetRequiredService<IOptions<TaskSettings>>(), tp.GetRequiredService<TransactionLogoCalculator>()));


			services.AddCors(options =>
			{
				options.AddDefaultPolicy(builder =>
				{
					builder.AllowAnyOrigin();
					builder.AllowAnyMethod();
					builder.AllowAnyHeader();
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// global cors policy
			app.UseCors();

			app.UseMiddleware<JwtMiddleware>();

			app.UseExceptionHandlingMiddleware();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			if (HybridSupport.IsElectronActive)
				SetupElectron(env);

			app.ApplicationServices.GetService<TaskPoller>();
			if(app.ApplicationServices.GetService<IOptions<AppSettings>>().Value.UseTransactionCalculator)
				app.ApplicationServices.GetService<TransactionLogoCalculator>();
		}

		private void AddProcessors(IServiceCollection services)
		{
			services.AddTransient(x => new ClientProcessor(x.GetRequiredService<IOptions<AppSettings>>().Value.MongoDB_ConnectionString));
			services.AddTransient(x => new AccountProcessor(x.GetRequiredService<IOptions<AppSettings>>().Value.MongoDB_ConnectionString));
			services.AddTransient(x => new TransactionProcessor(x.GetRequiredService<IOptions<AppSettings>>().Value.MongoDB_ConnectionString));
			services.AddTransient(x => new AuthenticationProcessor(x.GetRequiredService<IOptions<AppSettings>>().Value.MongoDB_ConnectionString));
			services.AddTransient(x => new DatafeedProcessor(x.GetRequiredService<IOptions<AppSettings>>().Value.MongoDB_ConnectionString));
			services.AddTransient(x => new TaskProcessor(x.GetRequiredService<IOptions<AppSettings>>().Value.MongoDB_ConnectionString));
		}

		private void SetupLogging(IServiceCollection services)
		{
			var sp = services.BuildServiceProvider();
			var appSettings = sp.GetService<IOptions<AppSettings>>().Value;
			
			var loggerConfiguration = new LoggerConfiguration()
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
				.Enrich.FromLogContext()
				.WriteTo.Logger(lc => lc
					.Filter.ByIncludingOnly(f => f.Level >= LogEventLevel.Error)
					.WriteTo.File("errors.txt"))
				.WriteTo.Logger(lc => lc
					.Filter.ByIncludingOnly(f => f.Level >= LogEventLevel.Information)
					.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss:fff} [{Level}] {Message}{NewLine}{Exception}")
				);

			if (!string.IsNullOrEmpty(appSettings.MongoDB_ConnectionString))
				loggerConfiguration.WriteTo.MongoDB($"{appSettings.MongoDB_ConnectionString}/finance", "logs_financeapi", LogEventLevel.Warning);

			loggerConfiguration.ReadFrom.Configuration(Configuration);

			Log.Logger = loggerConfiguration.CreateLogger();
		}

		private void SetupElectron(IWebHostEnvironment env)
		{
			string icon = Path.Combine(Environment.CurrentDirectory, "./wwwroot/logo_square.png");

			if (Electron.Tray.MenuItems.Count == 0)
			{
				var menu = new MenuItem
				{
					Label = "Exit",
					Click = () => Electron.App.Exit()
				};

				Electron.Tray.Show(icon, menu);
				Electron.Tray.SetToolTip("FinanceAPI");
			}

			var options = new BrowserWindowOptions
			{
				Show = false
			};

			System.Threading.Tasks.Task.Run(async () => await Electron.WindowManager.CreateWindowAsync(options));

			if (env.IsDevelopment())
			{
				var startNotification = new NotificationOptions("Finance API", "FinanceAPI has started")
				{
					Icon = icon
				};

				Electron.Notification.Show(startNotification);
			}
		}
	}
}
