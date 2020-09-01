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
		ITransactionsDataService _transactionDataService = new FinanceAPIMongoDataService.DataService.TransactionsDataService();
		public string InsertTransaction(Transaction transaction)
		{
			// Force transaction id to be empty
			transaction.ID = Guid.NewGuid().ToString();
			if (string.IsNullOrEmpty(transaction.ClientID))
				return null;

			if (new AccountProcessor().GetAccountById(transaction.AccountID, transaction.ClientID) == null)
				return "ERROR:Account Does not exist";

			return _transactionDataService.InsertTransaction(transaction) ? transaction.ID : null;
		}

		public Transaction GetTransactionById(string transactionId, string clientId)
		{
			if (string.IsNullOrEmpty(transactionId))
				return null;
			Transaction transaction = _transactionDataService.GetTransactionById(transactionId, clientId);
			transaction.AccountName = new AccountProcessor().GetAccountNameById(transaction.AccountID, clientId);
			return transaction;
		}

		public bool UpdateTransaction(Transaction transaction)
		{
			if (string.IsNullOrEmpty(transaction.ClientID))
				return false;

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
			transactions.ForEach(t => t.AccountName = new AccountProcessor().GetAccountNameById(t.AccountID, clientId));

			if (!string.IsNullOrEmpty(accountId))
				transactions = transactions.Where(t => t.AccountID == accountId).ToList();
			return transactions;
		}
	}
}
