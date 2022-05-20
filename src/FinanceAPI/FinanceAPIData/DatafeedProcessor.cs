using System.Collections.Generic;
using System.Linq;
using Coinbase;
using Coinbase.Models;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Utilities;
using Microsoft.Extensions.Options;

namespace FinanceAPIData
{
	public class DatafeedProcessor
	{
		IDatafeedDataService _datafeedDataService;
		private AccountProcessor _accountProcessor;
		private AppSettings _appSettings;

		public DatafeedProcessor(IDatafeedDataService datafeedDataService, AccountProcessor accountProcessor, IOptions<AppSettings> appSettings)
		{
			_datafeedDataService = datafeedDataService;
			_accountProcessor = accountProcessor;
			_appSettings = appSettings.Value;
		}
		public List<Datafeed> GetDatafeeds(string clientId, string datafeedType = null)
		{
			List<Datafeed> datafeeds = _datafeedDataService.GetDatafeeds(clientId);
			if (!string.IsNullOrEmpty(datafeedType))
				return datafeeds.Where(d => d.Provider == datafeedType.ToUpper()).ToList();

			return datafeeds;
		}

		public bool AddExternalAccountMapping(string clientId, string datafeed, string vendorID, string accountID, string externalAccountID, Dictionary<string, string> extraDetails)
		{
			if (string.IsNullOrEmpty(datafeed) || string.IsNullOrEmpty(vendorID) || string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(externalAccountID))
				return false;
			var account = _accountProcessor.GetAccountById(accountID, clientId);
			if (account == null)
				return false;

			return _datafeedDataService.AddAccountDatafeedMapping(clientId, datafeed, vendorID, accountID, externalAccountID, extraDetails);
		}

		public bool RemoveExternalAccountMapping(string clientId, string accountID, string externalAccountId)
		{
			if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(externalAccountId))
				return false;

			var account = _accountProcessor.GetAccountById(accountID, clientId);
			if (account == null)
				return false;

			return _datafeedDataService.RemoveAccountDatafeedMapping(clientId, accountID, externalAccountId);
		}

		public bool DeleteClientDatafeed(string clientId, string provider, string vendorID)
		{
			if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(vendorID))
				return false;

			return _datafeedDataService.DeleteClientDatafeed(clientId, provider, vendorID);
		}

		public List<ExternalAccount> GetExternalAccounts(string clientId, string accountId = null)
		{
			return _datafeedDataService.GetExternalAccounts(clientId, accountId);
		}
	}
}
