using FinanceAPICore;
using FinanceAPICore.DataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinanceAPIData
{
	public class DatafeedProcessor
	{
		IDatafeedDataService _datafeedDataService = new FinanceAPIMongoDataService.DataService.DatafeedDataService();
		public List<Datafeed> GetDatafeeds(string clientId, string datafeedType = null)
		{
			List<Datafeed> datafeeds = _datafeedDataService.GetDatafeeds(clientId);
			if (!string.IsNullOrEmpty(datafeedType))
				return datafeeds.Where(d => d.Provider == datafeedType.ToUpper()).ToList();

			return datafeeds;
		}

		public bool AddExternalAccountMapping(string clientId, string datafeed, string vendorID, string accountID, string externalAccountID)
		{
			if (string.IsNullOrEmpty(datafeed) || string.IsNullOrEmpty(vendorID) || string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(externalAccountID))
				return false;
			var account = new AccountProcessor().GetAccountById(accountID, clientId);
			if (account == null)
				return false;

			return _datafeedDataService.AddAccountDatafeedMapping(clientId, datafeed, vendorID, accountID, externalAccountID);
		}

		public bool RemoveExternalAccountMapping(string clientId, string accountID, string externalAccountId)
		{
			if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(externalAccountId))
				return false;

			var account = new AccountProcessor().GetAccountById(accountID, clientId);
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
