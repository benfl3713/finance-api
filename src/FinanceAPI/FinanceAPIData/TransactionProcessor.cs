using FinanceAPICore;
using FinanceAPICore.DataService;
using System;
using System.Collections.Generic;
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
			return _transactionDataService.InsertTransaction(transaction) ? transaction.ID : null;
		}

		public Transaction GetTransactionById(string transactionId)
		{
			if (string.IsNullOrEmpty(transactionId))
				return null;
			return _transactionDataService.GetTransactionById(transactionId);
		}

		public bool UpdateTransaction(Transaction transaction)
		{
			return _transactionDataService.UpdateTransaction(transaction);
		}

		public bool DeleteTransaction(string transactionId)
		{
			if (!string.IsNullOrEmpty(transactionId))
				return _transactionDataService.DeleteTransaction(transactionId);
			return false;
		}

		public List<Transaction> GetTransactions(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
				return null;

			return _transactionDataService.GetTransactions(clientId);
		}
	}
}
