using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore.DataService
{
	public interface ITransactionsDataService
	{
		bool InsertTransaction(Transaction transaction);
		Transaction GetTransactionById(string transactionId, string clientId);
		bool UpdateTransaction(Transaction transaction);
		bool DeleteTransaction(string transactionId, string clientId);
		List<Transaction> GetTransactions(string clientId);
	}
}
