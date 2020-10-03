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
        
        /// <summary>
        /// Calculates the account balance for every day in the last year
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accountId">If specified it will only calculate the account you provided</param>
        /// <param name="dateFrom">Ff specified it will only show it from the date provided</param>
        /// <returns></returns>
        public Dictionary<string, AccountBalanceHistory> GetBalanceHistory(string clientId, string accountId = null, DateTime? dateFrom = null)
        {
            Dictionary<string, AccountBalanceHistory> result = new Dictionary<string, AccountBalanceHistory>();
            if(!dateFrom.HasValue)
                dateFrom = DateTime.Today.AddYears(-1);
            
            List<Account> accounts = _accountProcessor.GetAccounts(clientId).Where(a => string.IsNullOrEmpty(accountId) || a.ID == accountId).ToList();
            
            foreach (Account account in accounts)
            {
                result.Add(account.ID, new AccountBalanceHistory(dateFrom.Value.Date));
                result[account.ID].AccountID = account.ID;
                result[account.ID].AccountName = account.AccountName;
                List<Transaction> allTransactions = _transactionProcessor.GetTransactions(clientId, account.ID).Where(t => t.Status == Status.SETTLED).ToList();
                List<Transaction> transactions = allTransactions.Where(t => t.Date.Date >= dateFrom.Value.Date).ToList();
                IEnumerable<IGrouping<DateTime, Transaction>> dateGroups = transactions.GroupBy(t => t.Date.Date);

                foreach (IGrouping<DateTime, Transaction> dateGroup in dateGroups.OrderBy(d => d.Key))
                {
                    // Calculate and set the dates balance
                    decimal accountBalance = allTransactions.Where(t => t.Date.Date <= dateGroup.Key.Date).Sum(t => t.Amount);
                    result[account.ID].History[dateGroup.Key] = accountBalance;
                    
                    // Set all older values that are null to the previous value. This accounts for when the were gaps where no transactions happened
                    decimal? previousValue = result[account.ID].History.OrderByDescending(h => h.Key).Where(h => h.Value != null && h.Key < dateGroup.Key)
                        .Select(h => h.Value).FirstOrDefault(0);
                    var nonSet = result[account.ID].History.Where(h => h.Value == null && h.Key < dateGroup.Key).ToList();
                    for (int i = 0; i < nonSet.Count; i++)
                    {
                        result[account.ID].History[nonSet[i].Key] = previousValue;
                    }
                }

                // Fill in any null values that would be all the values after the last transaction
                decimal? lastValue = result[account.ID].History.OrderByDescending(h => h.Key).Where(h => h.Value != null)
                    .Select(h => h.Value).FirstOrDefault(0);
                var notSet = result[account.ID].History.Where(h => h.Value == null).ToList();
                foreach (KeyValuePair<DateTime, decimal?> t in notSet)
                {
                    result[account.ID].History[t.Key] = lastValue;
                }
            }

            return result;
        }

        public Dictionary<string, decimal> GetSpentAmountPerCategory(string clientId, DateTime? dateFrom = null)
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>();
            dateFrom ??= DateTime.Today.AddMonths(-1);
            List<Transaction> transactions = _transactionProcessor.GetTransactions(clientId).Where(t => t.Amount < 0 && t.Date >= dateFrom).ToList();
            foreach (IGrouping<string,Transaction> categoryGroups in transactions.GroupBy(t => t.Category))
            {
                result.Add(categoryGroups.Key, categoryGroups.Sum(t => t.Amount * -1));
            }

            return result;
        }

        public class AccountBalanceHistory
        {
            public string AccountID;
            public string AccountName;
            public Dictionary<DateTime, decimal?> History = new Dictionary<DateTime, decimal?>();

            public AccountBalanceHistory(DateTime? dateFrom = null)
            {
                dateFrom ??= DateTime.Today.AddYears(-1);
                for (DateTime date = dateFrom.Value.Date; date <= DateTime.Today; date = date.AddDays(1))
                {
                    History.Add(date, null);
                }
            }
        }
    }
}