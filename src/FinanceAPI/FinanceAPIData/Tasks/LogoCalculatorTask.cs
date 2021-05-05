using FinanceAPICore.Tasks;
using Microsoft.Extensions.Options;

namespace FinanceAPIData.Tasks
{
    public class LogoCalculatorTask : BaseTask
    {
        private readonly TransactionLogoCalculator _transactionLogoCalculator;
        public LogoCalculatorTask(IOptions<TaskSettings> settings, TransactionLogoCalculator transactionLogoCalculator) : base(settings)
        {
            _transactionLogoCalculator = transactionLogoCalculator;
        }

        public override void Execute(Task task)
        {
            var filterClientId = task.Data["ClientID"]?.ToString();
            var filterAccountId = task.Data["AccountID"]?.ToString();
            
            // Run Calculator
            _transactionLogoCalculator.Run(filterClientId, filterAccountId);
            
            base.Execute(task);
        }
    }
}