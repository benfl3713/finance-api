using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using FinanceAPIData.Datafeeds.APIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Options;
using Task = FinanceAPICore.Tasks.Task;

namespace FinanceAPIData.Tasks
{
	public class AccountRefresh : BaseTask
    {
        private IDatafeedDataService _datafeedDataService;
        private IAccountDataService _accountDataService;
        private ITransactionsDataService _transactionDataService;
        private Task Task;

        public AccountRefresh(IOptions<TaskSettings> taskSettings): base(taskSettings)
        {
        }
        
        public override void Execute(Task task)
        {
            Task = task;
            var args = Task.Data;
            _datafeedDataService = new FinanceAPIMongoDataService.DataService.DatafeedDataService(Settings.MongoDB_ConnectionString);
            _accountDataService = new FinanceAPIMongoDataService.DataService.AccountDataService(Settings.MongoDB_ConnectionString);
            _transactionDataService = new FinanceAPIMongoDataService.DataService.TransactionsDataService(Settings.MongoDB_ConnectionString);

            if (string.IsNullOrEmpty(args["AccountID"].ToString()))
            {
                base.Execute(Task);
                return;
            }

            string accountID = args["AccountID"].ToString();
            if (!_accountDataService.GetAccounts(Task.ClientID).Any(a => a.ID == accountID))
            {
                base.Execute(Task);
                return;
            }

            List<ExternalAccount> externalAccounts =  _datafeedDataService.GetExternalAccounts(Task.ClientID, accountID);
            Account account = _accountDataService.GetAccountById(accountID, Task.ClientID);
            IDatafeedAPI datafeedApi = new TrueLayerAPI(Settings.MongoDB_ConnectionString, Settings.TrueLayer_ClientID, Settings.TrueLayer_ClientSecret, Settings.TrueLayer_Mode);

            if(account == null || externalAccounts.Count == 0)
			{
                base.Execute(Task);
                return;
            }

            AccountSettings accountSettings = _accountDataService.GetAccountSettings(accountID);

            decimal totalAccountBalance = 0;
            decimal totalAvailableAccountBalance = 0;

            foreach (var externalAccount in externalAccounts)
			{
                totalAccountBalance += ProcessExternalAccount(externalAccount, datafeedApi, account, out decimal availableBalance);
                totalAvailableAccountBalance += availableBalance;
            }

            // Reload account to get new balance
            account = _accountDataService.GetAccountById(accountID, Task.ClientID);

            if(accountSettings != null && accountSettings.GenerateAdjustments)
                BalanceAccount(account, totalAccountBalance);
            
            // Enqueue task to calculate logos on new transactions
            Task logoTask = new Task($"Logo Calculator [{account.AccountName}]", Task.ClientID, TaskType.LogoCalculator, DateTime.Now);
            logoTask.Data = new Dictionary<string, object>{{"ClientID", Task.ClientID}, {"AccountID", accountID}};

            // Set Account Last Refreshed Date
            _accountDataService.UpdateLastRefreshedDate(accountID, DateTime.Now);

            BackgroundJob.Enqueue<LogoCalculatorTask>(t => t.Execute(logoTask));
            base.Execute(Task);
        }

        private decimal ProcessExternalAccount(ExternalAccount externalAccount, IDatafeedAPI datafeedApi, Account account, out decimal availableBalance)
		{
            string encryptedAccessKey = _datafeedDataService.GetAccessKeyForExternalAccount(externalAccount.Provider, externalAccount.VendorID, Task.ClientID);
            availableBalance = 0;

            if (string.IsNullOrEmpty(externalAccount?.AccountID) || string.IsNullOrEmpty(encryptedAccessKey) || datafeedApi == null)
            {
                return 0;
            }

            List<Transaction> transactions = datafeedApi.GetAccountTransactions(externalAccount.AccountID, encryptedAccessKey, out decimal accountBalance, out availableBalance);
            Log($"Fetched [{transactions.Count}] transactions from provider");

            List<Transaction> sortedTransactions = new List<Transaction>();
            foreach (var transaction in transactions)
            {
                transaction.ClientID = Task.ClientID;
                transaction.AccountID = account.ID;
                transaction.AccountName = account?.AccountName ?? "Unknown";
                if (sortedTransactions.Where(t => t.ID == transaction.ID).Count() == 0)
                    sortedTransactions.Add(transaction);
            }

            //Run Algorithms
            sortedTransactions = MerchantAlgorithm(sortedTransactions);
            sortedTransactions = VendorAlgorithm(sortedTransactions);

            // Remove any pending transactions that have now been settled (Pending transaction not supplied by provider anymore indicates that  it has settled under a different transaction id)
            var exitingPendingTransactions = _transactionDataService.GetTransactions(account.ClientID).Where(t => t.Status == Status.PENDING && t.Owner != "User");
			foreach (var transaction in exitingPendingTransactions)
			{
                if (!sortedTransactions.Where(t => t.Status == Status.PENDING).Any(t => t.ID == transaction.ID))
                    _transactionDataService.DeleteTransaction(transaction.ID, transaction.ClientID);
			}

            //Add All sorted transactions
            foreach (Transaction transaction in sortedTransactions)
            {
                bool? imported = _transactionDataService.ImportDatafeedTransaction(transaction);
            }

            return accountBalance;
        }

        private void BalanceAccount(Account account, decimal accountBalance)
        {
			if (accountBalance != 0 && account.CurrentBalance.HasValue && account.CurrentBalance != accountBalance)
			{
				decimal difference = Math.Abs(account.CurrentBalance.Value - accountBalance);
				List<Transaction> recentTransactions = _transactionDataService.GetTransactions(Task.ClientID).Where(t => t.Date > DateTime.Now.AddMonths(-1)).ToList();
				List<Transaction> adjusts = recentTransactions.Where(t => t.Merchant == "Adjustment Transaction" && t.Type == "Adjust" && t.Amount == (account.CurrentBalance <= accountBalance ? -difference : difference)).ToList();
				Transaction transaction = new Transaction(Guid.NewGuid().ToString(), DateTime.Now, account.ID, "Adjust", account.CurrentBalance > accountBalance ? -difference : difference, "Adjust", "Adjustment Transaction", "Adjust", "This transaction is created from an account refresh to ensure that the account is balanced to the provider");
                transaction.ClientID = Task.ClientID;
                transaction.Owner = nameof(AccountRefresh);
				Log($"Account [{account.AccountName}] is out of balance. Creating adjustment of amount [{transaction.Amount}]");
                _transactionDataService.InsertTransaction(transaction);
			}
		}

        private List<Transaction> MerchantAlgorithm(List<Transaction> transactions)
        {
            Parallel.For(0, transactions.Count,
                   index => {
                       transactions[index].Merchant = transactions[index].Merchant.Replace("Visa Purchase ", "");
                       transactions[index].Merchant = transactions[index].Merchant.Replace("Contactless Payment ", "");
                   });

            return transactions;
        }

        private List<Transaction> VendorAlgorithm(List<Transaction> transactions)
        {
            Parallel.For(0, transactions.Count,
                   index => {
                       if (string.IsNullOrEmpty(transactions[index].Vendor) && transactions[index].Category == "Transfer")
                       {
                           transactions[index].Vendor = "Transfer";
                       }
                   });

            return transactions;
        }
    }
}
