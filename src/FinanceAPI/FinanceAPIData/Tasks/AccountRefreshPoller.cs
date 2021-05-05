using System;
using System.Collections.Generic;
using FinanceAPI;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using Hangfire;
using Microsoft.Extensions.Options;

namespace FinanceAPIData.Tasks
{
    public class AccountRefreshPoller : BaseTask
    {
        private IAccountDataService _accountDataService;
        private IDatafeedDataService _datafeedDataService;
        public AccountRefreshPoller(IOptions<TaskSettings> settings, IAccountDataService accountDataService, IDatafeedDataService datafeedDataService) : base(settings)
        {
            _accountDataService = accountDataService;
            _datafeedDataService = datafeedDataService;
        }

        public override void Execute(Task task)
        {
            List<Account> accounts = _accountDataService.GetAllAccounts();
            foreach (Account account in accounts)
            {
                List<ExternalAccount> externalAccounts =  _datafeedDataService.GetExternalAccounts(account.ClientID, account.ID);
                if(externalAccounts == null || externalAccounts.Count == 0)
                    continue;
                
                AccountSettings accountSettings = _accountDataService.GetAccountSettings(account.ID);
                if(accountSettings == null)
                    continue;
                
                if (ShouldRefreshAccount(accountSettings.RefreshInterval, account.LastRefreshed))
                {
                    Task refreshTask = new Task($"Account Refresh [{account.AccountName}]", account.ClientID, TaskType.AccountRefresh);
                    refreshTask.Data.Add("AccountID", account.ID);
                    BackgroundJob.Enqueue<AccountRefresh>(refresh => refresh.Execute(refreshTask));
                }

                
            }
        }

        private bool ShouldRefreshAccount(AccountSettings.RefreshIntervals refreshInterval, DateTime? lastRefreshed)
        {
            lastRefreshed ??= DateTime.MinValue;
            switch (refreshInterval)
            {
                case AccountSettings.RefreshIntervals.hourly:
                    return lastRefreshed.Value.ToUniversalTime() < DateTime.UtcNow.AddHours(-1);
                case AccountSettings.RefreshIntervals.sixHours:
                    return lastRefreshed.Value.ToUniversalTime() < DateTime.UtcNow.AddHours(-6);
                case AccountSettings.RefreshIntervals.biDaily:
                    return lastRefreshed.Value.ToUniversalTime() < DateTime.UtcNow.AddHours(-12);
                case AccountSettings.RefreshIntervals.Daily:
                    return lastRefreshed.Value.ToUniversalTime() < DateTime.UtcNow.AddDays(-1);
                default:
                    return false;
            }
        }
    }
}