using FinanceAPICore.DataService;
using System;
using System.Collections.Generic;
using System.Text;
using FinanceAPICore;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;

namespace FinanceAPIData
{
	public class TransactionLogoCalculator
	{
		ITransactionsDataService _transactionsDataService;
		IClientDataService _clientDataService;
		string _connectionString;
		System.Threading.Tasks.Task task;
		Dictionary<string, Logo> _logoOverrides;

		public TransactionLogoCalculator(string connectionString, Dictionary<string, Logo> logoOverrides, bool startIntervalRunner = false)
		{
			_connectionString = connectionString;
			_transactionsDataService = new FinanceAPIMongoDataService.DataService.TransactionsDataService(_connectionString);
			_clientDataService = new FinanceAPIMongoDataService.DataService.ClientDataService(_connectionString);
			_logoOverrides = logoOverrides;

			if (startIntervalRunner)
				StartIntervalRunner();
		}

		public void StartIntervalRunner()
		{
			if (task == null)
			{
				task = new System.Threading.Tasks.Task(() => IntervalRunner());
				task.Start();
			}
		}

		/// <summary>
		/// Runs every 30 minutes
		/// </summary>
		private void IntervalRunner()
		{
			System.Threading.Thread.Sleep(5000);
			while (true)
			{
				Serilog.Log.Logger?.Information("Running Logo Calculator");
				Run();
				Serilog.Log.Logger?.Information("Logo Calculator Complete Successfully");
				System.Threading.Thread.Sleep(60 * 60000);
			}
		}

		public void Run(string clientId = null, string accountId = null)
		{
			try
			{
				foreach (Client client in _clientDataService.GetAllClients())
				{
					if (!string.IsNullOrEmpty(clientId) && clientId != client.ID)
						continue;

					List<Transaction> transactions = string.IsNullOrEmpty(accountId) ? _transactionsDataService.GetTransactions(client.ID) : _transactionsDataService.GetTransactions(client.ID).Where(t => t.AccountID == accountId).ToList();
					Parallel.ForEach(transactions, t => CalculateLogo(t));
					transactions.Where(t => !string.IsNullOrEmpty(t.Logo)).ToList().ForEach(t => _transactionsDataService.UpdateTransactionLogo(t.ID, t.Logo));
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger?.Error(ex, "Logo Calculator Failed with error: {ex.Message}");
			}
		}

		private void CalculateLogo(Transaction transaction)
		{
			if (!string.IsNullOrEmpty(transaction.Logo))
				return;

			if (_logoOverrides != null)
			{
				foreach (var lo in _logoOverrides.Where(l => l.Value.ForceOverride == true))
				{
					if ((lo.Value.Types.Contains("Vendor") && lo.Key.Like(transaction.Vendor))
						|| (lo.Value.Types.Contains("Merchant") && lo.Key.Like(transaction.Merchant))
						|| (lo.Value.Types.Contains("Type") && lo.Key.Like(transaction.Type)))
					{
						transaction.Logo = lo.Value.Url;
						return;
					}
				}
			}

			if (!string.IsNullOrEmpty(transaction.Vendor))
			{
				var testLogo = $"https://logo.clearbit.com/{transaction.Vendor.Replace("'", "").Replace(" ", "").Replace(",", "")}.com";
				if (DoesImageExit(testLogo))
				{
					transaction.Logo = testLogo;
					return;
				}

				testLogo = $"https://logo.clearbit.com/{transaction.Vendor.Replace("'", "").Replace(" ", "").Replace(",", "")}.co.uk";
				if (DoesImageExit(testLogo))
				{
					transaction.Logo = testLogo;
					return;
				}
			}

			if (!string.IsNullOrEmpty(transaction.Merchant))
			{
				var testLogo = $"https://logo.clearbit.com/{transaction.Merchant.Replace("'", "").Replace(" ", "").Replace(",", "")}.com";
				if (DoesImageExit(testLogo))
				{
					transaction.Logo = testLogo;
					return;
				}

				testLogo = $"https://logo.clearbit.com/{transaction.Merchant.Replace("'", "").Replace(" ", "").Replace(",", "")}.co.uk";
				if (DoesImageExit(testLogo))
				{
					transaction.Logo = testLogo;
					return;
				}
			}


			if (_logoOverrides != null)
			{
				// Runs overrides again but without the force override requirement
				foreach (var lo in _logoOverrides)
				{
					if ((lo.Value.Types.Contains("Vendor") && lo.Key.Like(transaction.Vendor))
						|| (lo.Value.Types.Contains("Merchant") && lo.Key.Like(transaction.Merchant))
						|| (lo.Value.Types.Contains("Type") && lo.Key.Like(transaction.Type)))
					{
						transaction.Logo = lo.Value.Url;
						return;
					}
				}
			}

		}

		private bool DoesImageExit(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "HEAD";

			try
			{
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				return response.StatusCode == HttpStatusCode.OK;
			}
			catch
			{
				return false;
			}
		}

		public class Logo
		{
			public string Url { get; set; }
			public bool ForceOverride { get; set; } = false;
			public List<string> Types { get; set; } = new List<string>();
		}
	}

	public static class StringExtensions
	{
		public static bool Like(this string toSearch, string toFind)
		{
			if (string.IsNullOrEmpty(toSearch) || string.IsNullOrEmpty(toFind))
				return false;

			var test = LikeToRegular(toSearch);
			var isMatch = Regex.IsMatch(toFind, test);

			return Regex.IsMatch(toFind, LikeToRegular(toSearch));
		}

		private static String LikeToRegular(String value)
		{
			return "^" + Regex.Escape(value).Replace("_", ".").Replace("%", ".*") + "$";
		}
	}
}
