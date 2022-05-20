using System;
using System.Collections.Generic;
using FinanceAPICore;

namespace FinanceAPIData.Datafeeds.APIs
{
	public interface IDatafeedAPI
	{
		List<ExternalAccount> GetExternalAccounts(string clientId, string encryptedAccessKey, string vendorID, string vendorName, string provider, bool refreshToken = true);
		List<Transaction> GetAccountTransactions(ExternalAccount externalAccount, string encryptedAccessKey, out decimal? accountBalance, out decimal availableBalance, DateTime? dateFrom = null, DateTime? dateTo = null, bool refreshToken = true);
	}
}
