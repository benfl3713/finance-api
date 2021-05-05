using System.Collections.Generic;

namespace FinanceAPICore.DataService
{
	public interface ITransactionsDataService
	{
		bool InsertTransaction(Transaction transaction);
		Transaction GetTransactionById(string transactionId, string clientId);
		bool UpdateTransaction(Transaction transaction);
		bool UpdateTransactionLogo(string transactionId, string logo);
		bool DeleteTransaction(string transactionId, string clientId);
		bool DeleteAllAccountTransactions(string accountId, string clientId);
		List<Transaction> GetTransactions(string clientId);
		bool ImportDatafeedTransaction(Transaction transaction);
	}
}
