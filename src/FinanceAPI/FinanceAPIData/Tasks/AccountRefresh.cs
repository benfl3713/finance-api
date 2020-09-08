using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPIData.Datafeeds.APIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceAPIData.Tasks
{
	public class AccountRefresh
	{
        private IDatafeedDataService _datafeedDataService = new FinanceAPIMongoDataService.DataService.DatafeedDataService();
        private IAccountDataService _accountDataService = new FinanceAPIMongoDataService.DataService.AccountDataService();
        public void Execute(Dictionary<string, object> args)
        {
            if (string.IsNullOrEmpty(args["AccountID"].ToString()))
            {
                return;
            }

            string accountID = args["AccountID"].ToString();
            if (_accountDataService.GetAccounts("clientID").Where(a => a.ID == accountID).Count() == 0)
            {
                return;
            }

            string externalAccountID = "" /*= _datafeedDataService.GetExternalAccountMapping(accountID)*/;
            string encryptedAccessKey = "" /*= _datafeedDataService.GetAccessKeyForExternalAccount(externalAccountID, Task.ClientID)*/;
            string datafeed = "TRUELAYER";
            Account account = _accountDataService.GetAccountById(accountID, "clientID");
            IDatafeedAPI datafeedApi = new TrueLayerAPI("", "");

            if (string.IsNullOrEmpty(externalAccountID) || string.IsNullOrEmpty(encryptedAccessKey) || account == null || datafeedApi == null)
            {
                return;
            }

            List<Transaction> transactions = datafeedApi.GetAccountTransactions(externalAccountID, encryptedAccessKey, out decimal accountBalance);
            Console.WriteLine($"Fetched [{transactions.Count}] transactions from provider");

            if (datafeed == "PLAID" && transactions.Count == 500)
            {
                int lastCount = 500;
                for (int i = 1; i < 51; i++)
                {
                    transactions.AddRange(datafeedApi.GetAccountTransactions(externalAccountID, encryptedAccessKey, out decimal _, null, DateTime.Now.AddMonths(-i)));
                    if (lastCount + 500 != transactions.Count)
                        break;

                    lastCount += 500;
                }
            }

            List<Transaction> sortedTransactions = new List<Transaction>();
            foreach (var transaction in transactions)
            {
                transaction.AccountID = accountID;
                transaction.AccountName = account?.AccountName ?? "Unknown";
                if (sortedTransactions.Where(t => t.ID == transaction.ID).Count() == 0)
                    sortedTransactions.Add(transaction);
            }

            //Run Algorithms
            sortedTransactions = MerchantAlgorithm(sortedTransactions);
            sortedTransactions = VendorAlgorithm(sortedTransactions);

            int newTransactions = 0, updatedTransactions = 0;

            //Add All sorted transactions
            foreach (Transaction transaction in sortedTransactions)
            {
                //bool? imported = TransactionsDataService.ImportTransaction(transaction, Task.ClientID);
                //if (imported == true)
                //    newTransactions++;
                //else if (imported == null)
                //    updatedTransactions++;
            }

            Console.WriteLine($"{newTransactions} new transactions were imported, {updatedTransactions} transactions were updated");

            BalanceAccount(account, accountBalance);
        }

        private void BalanceAccount(Account account, decimal accountBalance)
        {
            //if (accountBalance != 0 && account.CurrentBalance != accountBalance)
            //{
            //    decimal difference = Math.Abs(account.CurrentBalance.Value - accountBalance);
            //    List<Transaction> recentTransactions = TransactionsDataService.GetTransactions(Task.ClientID).Where(t => t.Date > DateTime.Now.AddMonths(-1)).ToList();
            //    List<Transaction> adjusts = recentTransactions.Where(t => t.Merchant == "Atlas Adjustment Transaction" && t.Type == "Adjust" && t.Amount == (account.CurrentBalance <= accountBalance ? -difference : difference)).ToList();
            //    Transaction transaction = new Transaction(null, DateTime.Now, account.ID, "Adjust", account.CurrentBalance > accountBalance ? -difference : difference, "Adjust", "Atlas Adjustment Transaction", "Adjust", "This transaction is created from an account refresh to ensure that the account is balanced to the provider");
            //    Console.WriteLine($"Account [{account.AccountName}] is out of balance. Creating adjustment of amount [{transaction.Amount}]");
            //    transaction.AddTransaction();
            //}
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
