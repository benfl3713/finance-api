using FinanceAPI;
using FinanceAPIData.Datafeeds.APIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using Hangfire;
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

        public AccountRefresh(IOptions<TaskSettings> taskSettings, IDatafeedDataService datafeedDataService, IAccountDataService accountDataService, ITransactionsDataService transactionsDataService): base(taskSettings)
        {
            _datafeedDataService = datafeedDataService;
            _accountDataService = accountDataService;
            _transactionDataService = transactionsDataService;
        }
        
        public override void Execute(Task task)
        {
            Task = task;
            var args = Task.Data;

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
            IDatafeedAPI datafeedApi = new TrueLayerAPI(_datafeedDataService, Settings.TrueLayer_ClientID, Settings.TrueLayer_ClientSecret, Settings.TrueLayer_Mode);

            if(account == null || externalAccounts.Count == 0)
			{
                base.Execute(Task);
                return;
            }

            AccountSettings accountSettings = _accountDataService.GetAccountSettings(accountID);

            decimal? totalAccountBalance = null;
            decimal? totalAvailableAccountBalance = 0;
            int transactionsImportedCount = 0;

            foreach (var externalAccount in externalAccounts)
			{
                var balance =  ProcessExternalAccount(externalAccount, datafeedApi, account, out decimal availableBalance, out int tCount);
                if (balance.HasValue && totalAccountBalance == null)
                    totalAccountBalance = 0;
                totalAccountBalance += balance;
                totalAvailableAccountBalance += availableBalance;
                transactionsImportedCount += tCount;
            }

            // Reload account to get new balance
            account = _accountDataService.GetAccountById(accountID, Task.ClientID);

            if(accountSettings != null && accountSettings.GenerateAdjustments && totalAccountBalance.HasValue)
                BalanceAccount(account, ref totalAccountBalance, ref totalAvailableAccountBalance);
            
            // Enqueue task to calculate logos on new transactions
            Task logoTask = new Task($"Logo Calculator [{account.AccountName}]", Task.ClientID, TaskType.LogoCalculator, DateTime.Now);
            logoTask.Data = new Dictionary<string, object>{{"ClientID", Task.ClientID}, {"AccountID", accountID}};

            // Set Account Last Refreshed Date
            _accountDataService.UpdateLastRefreshedDate(accountID, DateTime.Now);

            if (accountSettings?.NotifyAccountRefreshes ?? false)
            {
                Notification.InsertNew(new Notification(Task.ClientID, $"Account Refresh for {account.AccountName} completed. \nImported {transactionsImportedCount} transactions")
                {
                    Details = new Dictionary<string, string>
                    {
                        {"AsAtAccountBalance", totalAccountBalance?.ToString()}
                    }
                });
            }

            BackgroundJob.Enqueue<LogoCalculatorTask>(t => t.Execute(logoTask));
            base.Execute(Task);
        }

        private decimal? ProcessExternalAccount(ExternalAccount externalAccount, IDatafeedAPI datafeedApi, Account account, out decimal availableBalance, out int transactionsImportedCount)
		{
            string encryptedAccessKey = _datafeedDataService.GetAccessKeyForExternalAccount(externalAccount.Provider, externalAccount.VendorID, Task.ClientID);
            availableBalance = 0;
            transactionsImportedCount = 0;

            if (string.IsNullOrEmpty(externalAccount?.AccountID) || string.IsNullOrEmpty(encryptedAccessKey) || datafeedApi == null)
            {
                return 0;
            }

            List<Transaction> transactions = datafeedApi.GetAccountTransactions(externalAccount, encryptedAccessKey, out decimal? accountBalance, out availableBalance);
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
            var exitingPendingTransactions = _transactionDataService.GetTransactions(account.ClientID).Where(t => t.Status == Status.PENDING && (t.Owner != "User" || t.Source != "User"));
			foreach (var transaction in exitingPendingTransactions)
			{
                if (sortedTransactions.Where(t => t.Status == Status.PENDING).All(t => t.ID != transaction.ID))
                    _transactionDataService.DeleteTransaction(transaction.ID, transaction.ClientID);
			}

            //Add All sorted transactions
            foreach (Transaction transaction in sortedTransactions)
            {
                bool? imported = _transactionDataService.ImportDatafeedTransaction(transaction);
                if (imported == true)
                    transactionsImportedCount++;
            }

            return accountBalance;
        }

        private void BalanceAccount(Account account, ref decimal? accountBalance, ref decimal? accountAvailableBalance)
        {
			if (accountBalance.HasValue && accountBalance != 0 && account.CurrentBalance.HasValue && account.CurrentBalance != accountBalance)
			{
                decimal difference = Math.Abs(account.CurrentBalance.Value - accountBalance.Value);
                
                if (FindInverseValueAdjustment(account.ClientID, account.ID, difference, false, out Transaction matchedAdjust))
                {
                    _transactionDataService.DeleteTransaction(matchedAdjust.ID, account.ClientID);
                }
                else
                {
                    Transaction transaction = new Transaction(Guid.NewGuid().ToString(), DateTime.Now, account.ID, "Adjust", account.CurrentBalance > accountBalance ? -difference : difference, "Adjust", "Adjustment Transaction", "Adjust", "This transaction is created from an account refresh to ensure that the account is balanced to the provider");
                    transaction.ClientID = Task.ClientID;
                    transaction.Owner = nameof(AccountRefresh);
                    Log($"Account [{account.AccountName}] is out of balance. Creating adjustment of amount [{transaction.Amount}]");
                    _transactionDataService.InsertTransaction(transaction);
                    accountBalance += difference;
                }
            }

            if (accountAvailableBalance.HasValue && accountAvailableBalance != 0 && account.AvailableBalance.HasValue && account.AvailableBalance != accountAvailableBalance)
            {
                decimal difference = Math.Abs(account.AvailableBalance.Value - accountAvailableBalance.Value);

                if (FindInverseValueAdjustment(account.ClientID, account.ID, difference, true, out Transaction matchedAdjust))
                {
                    _transactionDataService.DeleteTransaction(matchedAdjust.ID, account.ClientID);
                }
                else
                {
                    Transaction transaction = new Transaction(Guid.NewGuid().ToString(), DateTime.Now, account.ID, "Adjust", account.AvailableBalance > accountAvailableBalance ? -difference : difference, "Adjust", "Adjustment Pending Transaction", "Adjust", "This transaction is created from an account refresh to ensure that the account is balanced to the provider") { Status = Status.PENDING };
                    transaction.ClientID = Task.ClientID;
                    transaction.Owner = nameof(AccountRefresh);
                    Log($"Account [{account.AccountName}] is out of balance (Pending). Creating Pending adjustment of amount [{transaction.Amount}]");
                    _transactionDataService.InsertTransaction(transaction);
                    accountAvailableBalance += difference;
                }
            }
		}

        private bool FindInverseValueAdjustment(string clientId, string accountId, decimal value, bool pending, out Transaction transaction)
        {
            transaction =  _transactionDataService
                .GetTransactions(clientId)
                .Where(t => t.AccountID == accountId)
                .Where(t => t.Vendor == "Adjust")
                .Where(t => t.Status == (pending ? Status.PENDING : Status.SETTLED))
                .FirstOrDefault(t => t.Amount == value * -1);

            return transaction != null;
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
