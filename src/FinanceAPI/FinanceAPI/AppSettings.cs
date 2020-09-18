using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceAPI
{
	public class AppSettings
	{
		public string JwtSecret { get; set; }
		public string TrueLayer_ClientID { get; set; }
		public string TrueLayer_ClientSecret { get; set; }
		public string TrueLayer_Mode { get; set; }
		public string MongoDB_ConnectionString { get; set; } = "mongodb://localhost";
		public bool UseTransactionCalculator { get; set; } = true;
		public Dictionary<string, FinanceAPIData.TransactionLogoCalculator.Logo> LogoOverrides { get; set; }
		public bool EnableHangfireDashboard { get; set; } = true;
	}
}
