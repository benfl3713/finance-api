using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore.DataService
{
	public interface IAccountDataService
	{
		bool InsertAccount(Account account);
		Account GetAccountById(string accountId);
		bool UpdateAccount(Account account);
		bool DeleteAccount(string accountId);
		decimal GetAccountBalance(string accountId);
	}
}
