using System;
using System.Collections.Generic;

namespace FinanceAPICore.DataService
{
	public interface IDatafeedDataService
	{
		public List<Datafeed> GetDatafeeds(string clientId);
		public Datafeed GetDatafeedByAccessKey(string encryptedAccesskey);
		public bool AddUpdateClientDatafeed(Datafeed datafeed);
		public bool DeleteClientDatafeed(string clientId, string provider, string vendorID);
		public bool UpdateAccessKey(string newAccessKey, string newRefreshToken, string oldAccessKey, DateTime lastUpdated);
		public string GetRefreshTokenByAccessKey(string encryptedAccesskey);
		public bool AddAccountDatafeedMapping(string clientId, string datafeed, string vendorID, string accountID, string externalAccountID, Dictionary<string, string> extraDetails);
		public bool RemoveAccountDatafeedMapping(string clientId, string accountID, string externalAccountID);
		public bool RemoveAllAccountDatafeedMappings(string clientId, string accountID);
		public bool IsExternalAccountMapped(string clientId, string externalAccountID, string vendorID, out string mappedAccount);
		public List<ExternalAccount> GetExternalAccounts(string clientId, string accountId = null);
		public string GetAccessKeyForExternalAccount(string provider, string vendorId, string clientId);
	}
}
