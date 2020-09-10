using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using FinanceAPIData.Datafeeds.APIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceAPIData.Tasks
{
	public class AccountRefresh : BaseTask
    {
        private IDatafeedDataService _datafeedDataService = new FinanceAPIMongoDataService.DataService.DatafeedDataService();
        private IAccountDataService _accountDataService = new FinanceAPIMongoDataService.DataService.AccountDataService();
        private ITransactionsDataService _transactionDataService = new FinanceAPIMongoDataService.DataService.TransactionsDataService();
        public override void Execute(Dictionary<string, object> args, TaskSettings settings)
        {
            if (string.IsNullOrEmpty(args["AccountID"].ToString()))
            {
                base.Execute(args, settings);
                return;
            }

            string accountID = args["AccountID"].ToString();
            if (_accountDataService.GetAccounts(Task.ClientID).Where(a => a.ID == accountID).Count() == 0)
            {
                base.Execute(args, settings);
                return;
            }

            ExternalAccount externalAccount =  _datafeedDataService.GetExternalAccounts(Task.ClientID, accountID).FirstOrDefault();
            string encryptedAccessKey = _datafeedDataService.GetAccessKeyForExternalAccount(externalAccount.Provider, externalAccount.VendorID, Task.ClientID);
            Account account = _accountDataService.GetAccountById(accountID, Task.ClientID);
            IDatafeedAPI datafeedApi = new TrueLayerAPI(settings.TrueLayer_ClientID, settings.TrueLayer_ClientSecret);

            if (string.IsNullOrEmpty(externalAccount?.AccountID) || string.IsNullOrEmpty(encryptedAccessKey) || account == null || datafeedApi == null)
            {
                return;
            }

            List<Transaction> transactions = datafeedApi.GetAccountTransactions(externalAccount.AccountID, encryptedAccessKey, out decimal accountBalance);
            Console.WriteLine($"Fetched [{transactions.Count}] transactions from provider");

            if (externalAccount.Provider == "PLAID" && transactions.Count == 500)
            {
                int lastCount = 500;
                for (int i = 1; i < 51; i++)
                {
                    transactions.AddRange(datafeedApi.GetAccountTransactions(externalAccount.AccountID, encryptedAccessKey, out decimal _, null, DateTime.Now.AddMonths(-i)));
                    if (lastCount + 500 != transactions.Count)
                        break;

                    lastCount += 500;
                }
            }

            List<Transaction> sortedTransactions = new List<Transaction>();
            foreach (var transaction in transactions)
            {
                transaction.ClientID = Task.ClientID;
                transaction.AccountID = accountID;
                transaction.AccountName = account?.AccountName ?? "Unknown";
                if (sortedTransactions.Where(t => t.ID == transaction.ID).Count() == 0)
                    sortedTransactions.Add(transaction);
            }

            //Run Algorithms
            sortedTransactions = MerchantAlgorithm(sortedTransactions);
            sortedTransactions = VendorAlgorithm(sortedTransactions);

            //Add All sorted transactions
            foreach (Transaction transaction in sortedTransactions)
            {
				bool? imported = _transactionDataService.ImportDatafeedTransaction(transaction);
			}

            // Reload account to get new balance
            account = _accountDataService.GetAccountById(accountID, Task.ClientID);

            BalanceAccount(account, accountBalance);

            base.Execute(args, settings);
        }

        private void BalanceAccount(Account account, decimal accountBalance)
        {
			if (accountBalance != 0 && account.CurrentBalance != accountBalance)
			{
				decimal difference = Math.Abs(account.CurrentBalance.Value - accountBalance);
				List<Transaction> recentTransactions = _transactionDataService.GetTransactions(Task.ClientID).Where(t => t.Date > DateTime.Now.AddMonths(-1)).ToList();
				List<Transaction> adjusts = recentTransactions.Where(t => t.Merchant == "Adjustment Transaction" && t.Type == "Adjust" && t.Amount == (account.CurrentBalance <= accountBalance ? -difference : difference)).ToList();
				Transaction transaction = new Transaction(Guid.NewGuid().ToString(), DateTime.Now, account.ID, "Adjust", account.CurrentBalance > accountBalance ? -difference : difference, "Adjust", "Adjustment Transaction", "Adjust", "This transaction is created from an account refresh to ensure that the account is balanced to the provider");
                transaction.ClientID = Task.ClientID;
                transaction.Owner = nameof(AccountRefresh);
				Console.WriteLine($"Account [{account.AccountName}] is out of balance. Creating adjustment of amount [{transaction.Amount}]");
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
