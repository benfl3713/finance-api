using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore.DataService
{
	public interface IAccountDataService
	{
		bool InsertAccount(Account account);
		Account GetAccountById(string accountId, string clientId);
		bool UpdateAccount(Account account);
		bool DeleteAccount(string accountId, string clientId);
		decimal GetAccountBalance(string accountId, string clientId);
		List<Account> GetAccounts(string clientId);
	}
}
