using FinanceAPICore;
using FinanceAPICore.DataService;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData
{
	public class AccountProcessor
	{
		IAccountDataService _accountDataService;
		ITransactionsDataService _transactionDataService;
		IDatafeedDataService _datafeedDataService;
		string _connectionString;

		public AccountProcessor(string connectionString)
		{
			_connectionString = connectionString;
			_accountDataService = new FinanceAPIMongoDataService.DataService.AccountDataService(_connectionString);
			_transactionDataService = new FinanceAPIMongoDataService.DataService.TransactionsDataService(_connectionString);
			_datafeedDataService = new FinanceAPIMongoDataService.DataService.DatafeedDataService(_connectionString);
		}
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
			{
				if (!_transactionDataService.DeleteAllAccountTransactions(accountId, clientId) 
					|| !_datafeedDataService.RemoveAllAccountDatafeedMappings(clientId, accountId))
					return false;
				return _accountDataService.DeleteAccount(accountId, clientId);
			}
			return false;
		}

		public List<Account> GetAccounts(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
				return null;

			return _accountDataService.GetAccounts(clientId);
		}

		public string GetAccountNameById(string accountId, string clientId)
		{
			if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(clientId))
				return null;

			Account account =  _accountDataService.GetAccountById(accountId, clientId);
			if (account != null)
				return account.AccountName;

			return null;
		}
	}
}
