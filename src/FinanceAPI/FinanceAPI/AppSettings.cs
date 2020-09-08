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
	}
}
