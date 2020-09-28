using System;
using System.Collections.Generic;
using System.Linq;
using FinanceAPICore;
using FinanceAPICore.Extensions;

namespace FinanceAPIData
{
    public class StatisticsProcessor
    {
        private TransactionProcessor _transactionProcessor;
        private AccountProcessor _accountProcessor;
        public StatisticsProcessor(string connectionString)
        {
            _transactionProcessor = new TransactionProcessor(connectionString);
            _accountProcessor = new AccountProcessor(connectionString);
        }
        public Dictionary<string, AccountBalanceHistory> GetBalanceHistory(string clientId, string accountId = null, DateTime? dateFrom = null)
        {
            Dictionary<string, AccountBalanceHistory> result = new Dictionary<string, AccountBalanceHistory>();
            if(!dateFrom.HasValue)
                dateFrom = DateTime.Today.AddYears(-1);
            
            List<Account> accounts = _accountProcessor.GetAccounts(clientId).Where(a => string.IsNullOrEmpty(accountId) || a.ID == accountId).ToList();
            
            foreach (Account account in accounts)
            {
                result.Add(account.ID, new AccountBalanceHistory());
                result[account.ID].AccountID = account.ID;
                result[account.ID].AccountName = account.AccountName;
                List<Transaction> allTransactions = _transactionProcessor.GetTransactions(clientId, account.ID).Where(t => t.Status == Status.SETTLED).ToList();
                List<Transaction> transactions = allTransactions.Where(t => t.Date.Date >= dateFrom).ToList();
                IEnumerable<IGrouping<DateTime, Transaction>> dateGroups = transactions.GroupBy(t => t.Date.Date);

                foreach (IGrouping<DateTime, Transaction> dateGroup in dateGroups.OrderBy(d => d.Key))
                {
                    decimal accountBalance = allTransactions.Where(t => t.Date.Date <= dateGroup.Key.Date).Sum(t => t.Amount);
                    result[account.ID].History[dateGroup.Key] = accountBalance;
                    // Set all older values that are null
                    decimal? previosValue = result[account.ID].History.OrderByDescending(h => h.Key).Where(h => h.Value != null && h.Key < dateGroup.Key)
                        .Select(h => h.Value).FirstOrDefault(0);
                    var nonSet = result[account.ID].History.Where(h => h.Value == null && h.Key < dateGroup.Key).ToList();
                    for (int i = 0; i < nonSet.Count; i++)
                    {
                        result[account.ID].History[nonSet[i].Key] = previosValue;
                    }
                }

                // Fill in any null values that would be all values after the last transactions
                decimal? lastValue = result[account.ID].History.OrderByDescending(h => h.Key).Where(h => h.Value != null)
                    .Select(h => h.Value).First();
                
                var notSet = result[account.ID].History.Where(h => h.Value == null).ToList();
                for (int i = 0; i < notSet.Count; i++)
                {
                    result[account.ID].History[notSet[i].Key] = lastValue;
                }
            }

            return result;
        }

        public class AccountBalanceHistory
        {
            public string AccountID;
            public string AccountName;
            public Dictionary<DateTime, decimal?> History = new Dictionary<DateTime, decimal?>();

            public AccountBalanceHistory()
            {
                for (DateTime date = DateTime.Today.AddYears(-1); date <= DateTime.Today; date = date.AddDays(1))
                {
                    History.Add(date, null);
                }
            }
        }
    }
}