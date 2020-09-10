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
			});

			AddProcessors(services);
			services.AddTransient<JwtMiddleware>();
			services.AddSingleton(tp => new TaskPoller(tp.GetRequiredService<IOptions<TaskSettings>>()));

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


			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			if (HybridSupport.IsElectronActive)
				SetupElectron(env);

			app.ApplicationServices.GetService<TaskPoller>();
		}

		private void AddProcessors(IServiceCollection services)
		{
			services.AddTransient<ClientProcessor>();
			services.AddTransient<AccountProcessor>();
			services.AddTransient<TransactionProcessor>();
			services.AddTransient<AuthenticationProcessor>();
			services.AddTransient<DatafeedProcessor>();
			services.AddTransient<TaskProcessor>();
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
