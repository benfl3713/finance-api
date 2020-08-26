using System;
using System.Collections.Generic;
using System.Text;
using FinanceAPICore;
using FinanceAPICore.DataService;

namespace FinanceAPIMongoDataService.DataService
{
	public class TransactionsDataService : ITransactionsDataService
	{
		public Transaction GetTransactionById(string clientId, string transactionId)
		{
			throw new NotImplementedException();
		}

		public List<Transaction> GetTransactions(string clientId)
		{
			throw new NotImplementedException();
		}

		public bool InsertTransaction(string clientId, Transaction transaction)
		{
			throw new NotImplementedException();
		}

		public bool UpdateTransaction(string clientId, Transaction transaction)
		{
			throw new NotImplementedException();
		}

		public bool DeleteTransaction(string clientId, string transactionId)
		{
			throw new NotImplementedException();
		}
	}
}
