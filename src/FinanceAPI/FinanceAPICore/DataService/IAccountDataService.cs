using System;
using System.Collections.Generic;
using FinanceAPI;

namespace FinanceAPICore.DataService
{
	public interface IAccountDataService
	{
		bool InsertAccount(Account account);
		Account GetAccountById(string accountId, string clientId);
		bool UpdateAccount(Account account);
		bool DeleteAccount(string accountId, string clientId);
		List<Account> GetAccounts(string clientId);
		List<Account> GetAllAccounts();
		bool SetAccountSettings(AccountSettings accountSettings);
		AccountSettings GetAccountSettings(string accountId);
		bool UpdateLastRefreshedDate(string accountId, DateTime? lastRefreshed);
	}
}
