using FinanceAPICore;
using FinanceAPICore.DataService;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData
{
	public class AccountProcessor
	{
		IAccountDataService _accountDataService = new FinanceAPIMongoDataService.DataService.AccountDataService();
		public string InsertAccount(Account account)
		{
			// Force account id to be empty
			account.ID = Guid.NewGuid().ToString();
			if (string.IsNullOrEmpty(account.ClientID))
				return null;
			return _accountDataService.InsertAccount(account) ? account.ID : null;
		}

		public Account GetAccountById(string accountId, string clientId)
		{
			if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(clientId))
				return null;
			return _accountDataService.GetAccountById(accountId, clientId);
		}

		public bool UpdateAccount(Account account)
		{
			if (string.IsNullOrEmpty(account.ClientID))
				return false;
			return _accountDataService.UpdateAccount(account);
		}

		public bool DeleteAccount(string accountId, string clientId)
		{
			if (!string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(clientId))
				return _accountDataService.DeleteAccount(accountId, clientId);
			return false;
		}

		public List<Account> GetAccounts(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
				return null;

			return _accountDataService.GetAccounts(clientId);
		}
	}
}
