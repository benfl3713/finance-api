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
			return _accountDataService.InsertAccount(account) ? account.ID : null;
		}

		public Account GetAccountById(string accountId)
		{
			if (string.IsNullOrEmpty(accountId))
				return null;
			return _accountDataService.GetAccountById(accountId);
		}

		public bool UpdateAccount(Account account)
		{
			return _accountDataService.UpdateAccount(account);
		}

		public bool DeleteAccount(string accountId)
		{
			if (!string.IsNullOrEmpty(accountId))
				return _accountDataService.DeleteAccount(accountId);
			return false;
		}
	}
}
