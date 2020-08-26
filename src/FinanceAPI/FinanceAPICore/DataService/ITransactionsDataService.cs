using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore.DataService
{
	public interface ITransactionsDataService
	{
		List<Transaction> GetTransactions(string clientId);
		Transaction GetTransactionById(string clientId, string transactionId);
		bool InsertTransaction(string clientId, Transaction transaction);
		bool UpdateTransaction(string clientId, Transaction transaction);
		bool DeleteTransaction(string clientId, string transactionId);
	}
}
