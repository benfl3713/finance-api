using System;
using System.Collections.Generic;
using System.Linq;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using FinanceAPIData.Tasks;
using Hangfire;

namespace FinanceAPIData
{
	public class TransactionProcessor
	{
		private readonly ITransactionsDataService _transactionDataService;
		private readonly AccountProcessor _accountProcessor;
		private readonly TransactionLogoCalculator _logoCalculator;

		public TransactionProcessor(ITransactionsDataService transactionDataService, AccountProcessor accountProcessor, TransactionLogoCalculator logoCalculator)
		{
			_logoCalculator = logoCalculator;
			_transactionDataService = transactionDataService;
			_accountProcessor = accountProcessor;
		}
		public string InsertTransaction(Transaction transaction)
		{
			// Force transaction id to be empty
			transaction.ID = Guid.NewGuid().ToString();
			if (string.IsNullOrEmpty(transaction.ClientID))
				return null;

			if (_accountProcessor.GetAccountById(transaction.AccountID, transaction.ClientID) == null)
				throw new Exception("Account does not exist");

			transaction.Owner = "User";
			transaction = _logoCalculator.RunForTransaction(transaction);

			string result = _transactionDataService.InsertTransaction(transaction) ? transaction.ID : null;

			// Run logo calculator task on affected account
			Task logoTask = new Task($"Logo Calculator [{transaction.AccountName}]", transaction.ClientID, TaskType.LogoCalculator, DateTime.Now) {Data = new Dictionary<string, object> {{"ClientID", transaction.ClientID}, {"AccountID", transaction.AccountID}}};
			BackgroundJob.Enqueue<LogoCalculatorTask>(t => t.Execute(logoTask));

			return result;
		}

		public Transaction GetTransactionById(string transactionId, string clientId)
		{
			if (string.IsNullOrEmpty(transactionId))
				return null;
			Transaction transaction = _transactionDataService.GetTransactionById(transactionId, clientId);
			transaction.AccountName = _accountProcessor.GetAccountNameById(transaction.AccountID, clientId);
			return transaction;
		}

		public bool UpdateTransaction(Transaction transaction)
		{
			if (string.IsNullOrEmpty(transaction.ClientID))
				return false;

			transaction.Owner = "User";
			transaction = _logoCalculator.RunForTransaction(transaction);
			bool result =  _transactionDataService.UpdateTransaction(transaction);

			Task logoTask = new Task($"Logo Calculator [{transaction.AccountName}]", transaction.ClientID, TaskType.LogoCalculator, DateTime.Now) {Data = new Dictionary<string, object> {{"ClientID", transaction.ClientID}, {"AccountID", transaction.AccountID}}};
			//new TransactionLogoCalculator()
			BackgroundJob.Enqueue<LogoCalculatorTask>(t => t.Execute(logoTask));

			return result;
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

		public void LoadTransactionAccountNames(List<Transaction> transactions, string clientId)
		{
			Dictionary<string, string> accountNames = new Dictionary<string, string>();
			foreach (Transaction transaction in transactions)
			{
				if (accountNames.ContainsKey(transaction.AccountID))
				{
					transaction.AccountName = accountNames[transaction.AccountID];
				}
				else
				{
					string accountName = _accountProcessor.GetAccountNameById(transaction.AccountID, clientId);
					transaction.AccountName = accountName;
					accountNames.Add(transaction.AccountID, accountName);
				}
			}
		}
	}
}
