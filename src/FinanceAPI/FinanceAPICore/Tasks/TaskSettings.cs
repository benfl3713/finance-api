using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore.Tasks
{
	public class TaskSettings
	{
		public int PollingInterval = 10000;
		public string TrueLayer_ClientID { get; set; }
		public string TrueLayer_ClientSecret { get; set; }
		public string TrueLayer_Mode { get; set; }
		public string MongoDB_ConnectionString { get; set; } = "mongodb://localhost";
		public Dictionary<string, Logo> LogoOverrides { get; set; }
	}
}
