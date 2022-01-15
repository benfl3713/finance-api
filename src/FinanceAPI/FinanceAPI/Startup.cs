using System;
using System.IO;
using ElectronNET.API;
using ElectronNET.API.Entities;
using FinanceAPI.Middleware;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.DataService.Wealth;
using FinanceAPICore.Tasks;
using FinanceAPIData;
using FinanceAPIData.TaskManagment;
using FinanceAPIData.Tasks;
using FinanceAPIData.Wealth;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using MenuItem = ElectronNET.API.Entities.MenuItem;
using Notification = FinanceAPICore.Notification;

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
			// services.Configure<TaskSettings>(settings =>
			// {
			// 	settings.TrueLayer_ClientID = Configuration.GetValue<string>(nameof(settings.TrueLayer_ClientID));
			// 	settings.TrueLayer_ClientSecret = Configuration.GetValue<string>(nameof(settings.TrueLayer_ClientSecret));
			// 	settings.TrueLayer_Mode = Configuration.GetValue<string>(nameof(settings.TrueLayer_Mode));
			// 	settings.MongoDB_ConnectionString = Configuration.GetValue<string>(nameof(settings.MongoDB_ConnectionString)) ?? "mongodb://localhost";
			// 	settings.LogoOverrides = Configuration.GetValue<Dictionary<string, Logo>>(nameof(AppSettings.LogoOverrides));
			// });
			services.Configure<TaskSettings>(Configuration);
			

			SetupLogging(services);
			SetupHangfire(services);

			services.AddTransient<JwtMiddleware>();
			services.AddSingleton(x => new TransactionLogoCalculator(x.GetRequiredService<ITransactionsDataService>(), x.GetRequiredService<IClientDataService>(), x.GetRequiredService<IOptions<AppSettings>>().Value.LogoOverrides));
			services.AddSingleton<TaskPoller>();

			AddDataServices(services);
			AddProcessors(services);

			services.AddCors(options =>
			{
				options.AddDefaultPolicy(builder =>
				{
					builder.AllowAnyOrigin();
					builder.AllowAnyMethod();
					builder.AllowAnyHeader();
				});
			});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo {Title = "Finance API", Description = "💲💰 Personal Finance Manager API"});
				c.AddSecurityDefinition("JWT Authentication", new OpenApiSecurityScheme
				{
					Flows = new OpenApiOAuthFlows
					{
						Password = new OpenApiOAuthFlow
						{
							AuthorizationUrl = new Uri("/api/Auth/authenticate", UriKind.Relative)
						}
					},
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http
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

			app.UseSerilogRequestLogging();

			Notification.NotificationDataService = app.ApplicationServices.GetService<INotificationDataService>();

			// new TransactionMigrater().Run(app.ApplicationServices.GetService<IOptions<AppSettings>>().Value.MongoDB_ConnectionString);
			if (app.ApplicationServices.GetService<IOptions<AppSettings>>().Value.IsDemo)
			{
				Serilog.Log.Logger?.Write(LogEventLevel.Information, "FinanceAPI running in Demo Mode");
				RecurringJob.AddOrUpdate<DemoClearDownTask>("Demo-DemoClearDownTask", r => r.Execute(new Task("DemoClearDownTask", null, TaskType.DemoClearDownTask, DateTime.Now)), "59 23 * * Sun");
			}
			else
			{
				RecurringJob.RemoveIfExists("Demo-DemoClearDownTask");
			}

			// global cors policy
			app.UseCors();

			app.UseMiddleware<JwtMiddleware>();

			app.UseExceptionHandlingMiddleware();

			app.UseRouting();

			app.UseSwagger();

			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance API");
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			if (HybridSupport.IsElectronActive)
				SetupElectron(env);

			app.ApplicationServices.GetService<TaskPoller>();
			if(app.ApplicationServices.GetService<IOptions<AppSettings>>().Value.UseTransactionCalculator)
				app.ApplicationServices.GetService<TransactionLogoCalculator>();

			if (app.ApplicationServices.GetService<IOptions<AppSettings>>().Value.EnableHangfireDashboard)
				app.UseHangfireDashboard( "/tasks", new DashboardOptions
				{
					AppPath = null,
					Authorization = new [] {new HangfireAuthorizationFilter()}
				});
		}

		private void AddProcessors(IServiceCollection services)
		{
			services.AddTransient<ClientProcessor>();
			services.AddTransient<AccountProcessor>();
			services.AddTransient<TransactionProcessor>();
			services.AddTransient<AuthenticationProcessor>();
			services.AddTransient<DatafeedProcessor>();
			services.AddTransient<StatisticsProcessor>();
			services.AddTransient<TaskProcessor>();
			services.AddTransient<GoalProcessor>();
			services.AddTransient<NotificationProcessor>();
			services.AddTransient<AssetRepository>();
			services.AddTransient<TradeRepository>();
		}

		private void AddDataServices(IServiceCollection services)
		{
			CreateDataServiceTransient<IClientDataService, FinanceAPIMongoDataService.DataService.ClientDataService>(services);
			CreateDataServiceTransient<IAccountDataService, FinanceAPIMongoDataService.DataService.AccountDataService>(services);
			CreateDataServiceTransient<ITransactionsDataService, FinanceAPIMongoDataService.DataService.TransactionsDataService>(services);
			CreateDataServiceTransient<IDatafeedDataService, FinanceAPIMongoDataService.DataService.DatafeedDataService>(services);
			CreateDataServiceTransient<ITaskDataService, FinanceAPIMongoDataService.DataService.TaskDataService>(services);
			CreateDataServiceTransient<IGoalDataService, FinanceAPIMongoDataService.DataService.GoalDataService>(services);
			CreateDataServiceTransient<INotificationDataService, FinanceAPIMongoDataService.DataService.NotificationDataService>(services);
			CreateDataServiceTransient<IAssetDataService, FinanceAPIMongoDataService.DataService.Wealth.AssetDataService>(services);
			CreateDataServiceTransient<ITradeDataService, FinanceAPIMongoDataService.DataService.Wealth.TradeDataService>(services);
		}

		private void CreateDataServiceTransient<TInterface, TDataService>(IServiceCollection services) where TDataService : BaseDataService, TInterface where TInterface : class
		{
			// ReSharper disable once RedundantExplicitParamsArrayCreation
			services.AddTransient<TInterface, TDataService>(x => (TDataService) Activator.CreateInstance(typeof(TDataService), x.GetRequiredService<IOptions<AppSettings>>()));
		}

		private void SetupLogging(IServiceCollection services)
		{
			var sp = services.BuildServiceProvider();
			var appSettings = sp.GetService<IOptions<AppSettings>>().Value;
			
			var loggerConfiguration = new LoggerConfiguration()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
				.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Verbose)
				.Enrich.FromLogContext()
				.WriteTo.Logger(lc => lc
					.Filter.ByIncludingOnly(f => f.Level >= LogEventLevel.Error)
					.WriteTo.File(new CompactJsonFormatter(), "errors.txt"))
				.WriteTo.Logger(lc => lc
					.Filter.ByIncludingOnly(f => f.Level >= LogEventLevel.Information)
					.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss:fff} [{Level}] {Message}{NewLine}{Exception}")
				);

			if (!string.IsNullOrEmpty(appSettings.MongoDB_ConnectionString))
				loggerConfiguration.WriteTo.MongoDB($"{appSettings.MongoDB_ConnectionString}/finance", "logs_financeapi", LogEventLevel.Warning);

			loggerConfiguration.ReadFrom.Configuration(Configuration);

			Log.Logger = loggerConfiguration.CreateLogger();
		}

		private void SetupHangfire(IServiceCollection services)
		{
			var sp = services.BuildServiceProvider();
			var appSettings = sp.GetService<IOptions<AppSettings>>().Value;
			if (string.IsNullOrEmpty(appSettings.MongoDB_ConnectionString))
				return;

			var storageOptions = new MongoStorageOptions
			{
				MigrationOptions = new MongoMigrationOptions
				{
					MigrationStrategy = new MigrateMongoMigrationStrategy(),
					BackupStrategy = new CollectionMongoBackupStrategy()
				},
				CheckConnection = false
			};
			
			// Add Hangfire services.
			services.AddHangfire(configuration => configuration
				.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
				.UseSimpleAssemblyNameTypeSerializer()
				.UseRecommendedSerializerSettings()
				.UseConsole()
				.UseMongoStorage(appSettings.MongoDB_ConnectionString + "/finance_hangfire", storageOptions)
			);

			// Add the processing server as IHostedService
			services.AddHangfireServer();
		}

		public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
		{
			public bool Authorize(DashboardContext context)
			{
				// Allow all access
				return true;
			}
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
