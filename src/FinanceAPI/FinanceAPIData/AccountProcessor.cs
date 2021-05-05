using FinanceAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Extensions;

namespace FinanceAPIData
{
	public class AccountProcessor
	{
		IAccountDataService _accountDataService;
		ITransactionsDataService _transactionDataService;
		IDatafeedDataService _datafeedDataService;

		public AccountProcessor(IAccountDataService accountDataService, ITransactionsDataService transactionsDataService, IDatafeedDataService datafeedDataService)
		{
			_accountDataService = accountDataService;
			_transactionDataService = transactionsDataService;
			_datafeedDataService = datafeedDataService;
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
				throw new ArgumentException("accountId is required");
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
				_datafeedDataService.RemoveAllAccountDatafeedMappings(clientId, accountId);
				if (!_transactionDataService.DeleteAllAccountTransactions(accountId, clientId))
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
			return account?.AccountName;
		}

		public decimal? GetSpentThisWeek(string accountId, string clientId)
		{
			Account account = _accountDataService.GetAccountById(accountId, clientId);
			if (account == null)
				throw new Exception("Cannot find account");

			DateTime dateFrom = DateTime.UtcNow.StartOfWeek(DayOfWeek.Sunday).Date;
			
			return _transactionDataService.GetTransactions(clientId).Where(t => t.AccountID == accountId 
				&& t.Date.Date >= dateFrom.Date
				&& t.Amount < 0)
				.Sum(t => t.Amount)
				* -1;
		}

		public bool SetAccountSettings(AccountSettings accountSettings, string clientId)
		{
			Account account = _accountDataService.GetAccountById(accountSettings.AccountID, clientId);
			if (account == null)
				throw new Exception("Cannot find account");

			return _accountDataService.SetAccountSettings(accountSettings);
		}

		public AccountSettings GetAccountSettings(string accountId, string clientId)
		{
			Account account = _accountDataService.GetAccountById(accountId, clientId);
			if (account == null)
				throw new Exception("Cannot find account");

			return _accountDataService.GetAccountSettings(accountId);
		}
	}
}
