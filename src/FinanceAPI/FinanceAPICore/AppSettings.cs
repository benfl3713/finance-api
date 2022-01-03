using System.Collections.Generic;

namespace FinanceAPICore
{
	public class AppSettings
	{
		public string JwtSecret { get; set; }
		public string TrueLayer_ClientID { get; set; }
		public string TrueLayer_ClientSecret { get; set; }
		public string TrueLayer_Mode { get; set; }
		public string MongoDB_ConnectionString { get; set; } = "mongodb://localhost";
		public bool UseTransactionCalculator { get; set; } = true;
		public Dictionary<string, Logo> LogoOverrides { get; set; }
		/// <summary>
		/// Warning. When enabled any user will be able to see all server tasks running as well as being
		/// able to manually run tasks. This is off by default.
		/// <a href="https://docs.hangfire.io/en/latest/configuration/using-dashboard.html#configuring-authorization">https://docs.hangfire.io/en/latest/configuration/using-dashboard.html#configuring-authorization</a>
		/// </summary>
		public bool EnableHangfireDashboard { get; set; } = false;

		public bool IsDemo { get; set; } = false;
		public string CoinBase_ClientId { get; set; }
		public string CoinBase_ClientSecret { get; set; }
	}
}
