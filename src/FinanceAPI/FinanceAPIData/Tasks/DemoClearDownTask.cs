using System;
using System.Collections.Generic;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using Microsoft.Extensions.Options;
using Task = FinanceAPICore.Tasks.Task;

namespace FinanceAPIData.Tasks
{
    public class DemoClearDownTask : BaseTask
    {
        protected ITransactionsDataService _transactionsDataService;
        protected IClientDataService _clientDataService;
        private TaskSettings _settings;
        
        public DemoClearDownTask(IOptions<TaskSettings> settings, ITransactionsDataService transactionsDataService, IClientDataService clientDataService) : base(settings)
        {
            _settings = settings.Value;
            _transactionsDataService = transactionsDataService;
            _clientDataService = clientDataService;
        }

        public override void Execute(Task task)
        {
            try
            {
                if (_settings.IsDemo == false)
                {
                    base.Execute(task);
                    return;
                }

                foreach (Client client in _clientDataService.GetAllClients())
                {
                    List<Transaction> transactions = _transactionsDataService.GetTransactions(client.ID);
                    foreach (Transaction transaction in transactions)
                    {
                        if(transaction.Owner == "User")
                            _transactionsDataService.DeleteTransaction(transaction.ID, client.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger?.Error(ex, "Demo Clean Down Failed with error: {ex.Message}");
            }

            base.Execute(task);
        }
    }
}