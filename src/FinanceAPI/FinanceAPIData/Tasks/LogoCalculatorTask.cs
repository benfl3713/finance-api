using FinanceAPICore.Tasks;
using Microsoft.Extensions.Options;

namespace FinanceAPIData.Tasks
{
    public class LogoCalculatorTask : BaseTask
    {
        public LogoCalculatorTask(IOptions<TaskSettings> settings) : base(settings)
        {
        }

        public override void Execute(Task task)
        {
            var calculator = new TransactionLogoCalculator(Settings.MongoDB_ConnectionString, Settings.LogoOverrides);
            var filterClientId = task.Data["ClientID"]?.ToString();
            var filterAccountId = task.Data["AccountID"]?.ToString();
            
            // Run Calculator
            calculator.Run(filterClientId, filterAccountId);
            
            base.Execute(task);
        }
    }
}