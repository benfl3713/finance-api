using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using FinanceAPICore;
using FinanceAPICore.DataService;

namespace FinanceAPIData
{
	public class TransactionLogoCalculator
	{
		protected ITransactionsDataService _transactionsDataService;
		protected IClientDataService _clientDataService;
		protected Dictionary<string, Logo> _logoOverrides;

		public TransactionLogoCalculator(ITransactionsDataService transactionsDataService, IClientDataService clientDataService, Dictionary<string, Logo> logoOverrides)
		{
			_transactionsDataService = transactionsDataService;
			_clientDataService = clientDataService;
			_logoOverrides = logoOverrides;
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
					Parallel.ForEach(transactions, t => CalculateLogo(ref t));
					transactions.Where(t => !string.IsNullOrEmpty(t.Logo)).ToList().ForEach(t => _transactionsDataService.UpdateTransactionLogo(t.ID, t.Logo));
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger?.Error(ex, "Logo Calculator Failed with error: {ex.Message}");
			}
		}

		public Transaction RunForTransaction(Transaction transaction)
		{
			CalculateLogo(ref transaction);
			return transaction;
		}

		private void CalculateLogo(ref Transaction transaction)
		{
			if (!string.IsNullOrEmpty(transaction.Logo))
				return;

			if (_logoOverrides != null)
			{
				foreach (var lo in _logoOverrides.Where(l => l.Value.ForceOverride))
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

				testLogo = $"https://logo.clearbit.com/{transaction.Vendor.Replace("'", "").Replace(" ", "").Replace(",", "").Split(' ')[0]}.com";
				if (DoesImageExit(testLogo))
				{
					transaction.Logo = testLogo;
					return;
				}

				if (transaction.Vendor.Contains("Visa Debit Transaction "))
				{
					testLogo = $"https://logo.clearbit.com/{transaction.Vendor.Replace("'", "").Replace(" ", "").Replace(",", "").Replace("Visa Debit Transaction ", "")}.com";
					if (DoesImageExit(testLogo))
					{
						transaction.Logo = testLogo;
						return;
					}
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

		protected virtual bool DoesImageExit(string url)
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
