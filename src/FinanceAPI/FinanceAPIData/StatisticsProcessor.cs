using System;
using System.Collections.Generic;
using System.Linq;
using FinanceAPICore;

namespace FinanceAPIData
{
    public class StatisticsProcessor
    {
        private TransactionProcessor _transactionProcessor;
        private AccountProcessor _accountProcessor;
        public StatisticsProcessor(AccountProcessor accountProcessor, TransactionProcessor transactionProcessor)
        {
            _transactionProcessor = transactionProcessor;
            _accountProcessor = accountProcessor;
        }

        public Dictionary<string, AccountBalanceHistory> GetBalanceHistoryV2(string clientId, string accountId = null, DateTime? dateFrom = null)
        {
            Dictionary<string, AccountBalanceHistory> result = new Dictionary<string, AccountBalanceHistory>();
            if(!dateFrom.HasValue)
                dateFrom = DateTime.Today.AddYears(-1);


            List<Account> accounts = _accountProcessor.GetAccounts(clientId).Where(a => string.IsNullOrEmpty(accountId) || a.ID == accountId).ToList();
            
            foreach (Account account in accounts)
            {
                result.Add(account.ID, new AccountBalanceHistory(dateFrom.Value));
                result[account.ID].AccountID = account.ID;
                result[account.ID].AccountName = account.AccountName;
                List<Transaction> allTransactions = _transactionProcessor.GetTransactions(clientId, account.ID).Where(t => t.Status == Status.SETTLED).ToList();
                result[account.ID].History.Clear();
                for (DateTime date = dateFrom.Value.Date; date <= DateTime.Today; date = date.AddDays(1))
                {
                    result[account.ID].History.Add(date, GetAccountCurrentBalanceAtDate(clientId, account.ID, date, allTransactions));
                }
            }

            return result;
        }

        public decimal GetAccountCurrentBalanceAtDate(string clientId, string accountId, DateTime dateTo, List<Transaction> accountTransactions = null)
        {
            accountTransactions ??= _transactionProcessor.GetTransactions(clientId, accountId).Where(t => t.Status == Status.SETTLED).ToList();
            return accountTransactions.Where(t => t.Date <= dateTo).Sum(t => t.Amount);
        }

        public Dictionary<string, decimal> GetSpentAmountPerCategory(string clientId, DateTime? dateFrom = null)
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>();
            dateFrom ??= DateTime.Today.AddMonths(-1);
            List<Transaction> transactions = _transactionProcessor.GetTransactions(clientId).Where(t => t.Amount < 0 && t.Date >= dateFrom.Value).ToList();
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
                    History.Add(date.Date, null);
                }
            }
        }
    }
}