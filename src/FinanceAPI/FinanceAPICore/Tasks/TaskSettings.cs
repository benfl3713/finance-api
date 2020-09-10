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
	}
}
