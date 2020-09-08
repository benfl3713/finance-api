using FinanceAPICore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData.Datafeeds.APIs
{
	public interface IDatafeedAPI
	{
		List<ExternalAccount> GetExternalAccounts(string clientId, string encryptedAccessKey, string vendorID, string vendorName, string provider, bool refreshToken = true);
		List<Transaction> GetAccountTransactions(string externalAccountID, string encryptedAccessKey, out decimal accountBalance, DateTime? dateFrom = null, DateTime? dateTo = null, bool refreshToken = true);
	}
}
