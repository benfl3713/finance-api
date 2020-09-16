using FinanceAPICore;
using FinanceAPICore.DataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinanceAPIData
{
	public class TransactionProcessor
	{
		ITransactionsDataService _transactionDataService;
		string _connectionString;

		public TransactionProcessor(string connectionString)
		{
			_connectionString = connectionString;
			_transactionDataService = new FinanceAPIMongoDataService.DataService.TransactionsDataService(_connectionString);
		}
		public string InsertTransaction(Transaction transaction)
		{
			// Force transaction id to be empty
			transaction.ID = Guid.NewGuid().ToString();
			if (string.IsNullOrEmpty(transaction.ClientID))
				return null;

			if (new AccountProcessor(_connectionString).GetAccountById(transaction.AccountID, transaction.ClientID) == null)
				throw new Exception("Account does not exist");

			transaction.Owner = "User";

			return _transactionDataService.InsertTransaction(transaction) ? transaction.ID : null;
		}

		public Transaction GetTransactionById(string transactionId, string clientId)
		{
			if (string.IsNullOrEmpty(transactionId))
				return null;
			Transaction transaction = _transactionDataService.GetTransactionById(transactionId, clientId);
			transaction.AccountName = new AccountProcessor(_connectionString).GetAccountNameById(transaction.AccountID, clientId);
			return transaction;
		}

		public bool UpdateTransaction(Transaction transaction)
		{
			if (string.IsNullOrEmpty(transaction.ClientID))
				return false;

			transaction.Owner = "User";
			return _transactionDataService.UpdateTransaction(transaction);
		}

		public bool DeleteTransaction(string transactionId, string clientId)
		{
			if (!string.IsNullOrEmpty(transactionId))
				return _transactionDataService.DeleteTransaction(transactionId, clientId);
			return false;
		}

		public List<Transaction> GetTransactions(string clientId, string accountId = null)
		{
			if (string.IsNullOrEmpty(clientId))
				return null;

			List<Transaction> transactions = _transactionDataService.GetTransactions(clientId);
			LoadTransactionAccountNames(transactions, clientId);

			if (!string.IsNullOrEmpty(accountId))
				transactions = transactions.Where(t => t.AccountID == accountId).ToList();
			return transactions;
		}

		private void LoadTransactionAccountNames(List<Transaction> transactions, string clientId)
		{
			Dictionary<string, string> accountNames = new Dictionary<string, string>();
			AccountProcessor accountProcessor = new AccountProcessor(_connectionString);
			foreach (Transaction transaction in transactions)
			{
				if (accountNames.ContainsKey(transaction.AccountID))
				{
					transaction.AccountName = accountNames[transaction.AccountID];
				}
				else
				{
					string accountName = accountProcessor.GetAccountNameById(transaction.AccountID, clientId);
					transaction.AccountName = accountName;
					accountNames.Add(transaction.AccountID, accountName);
				}
			}
		}
	}
}
