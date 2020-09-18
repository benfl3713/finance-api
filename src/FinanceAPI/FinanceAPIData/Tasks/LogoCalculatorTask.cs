using FinanceAPICore.Tasks;
using Microsoft.Extensions.Options;

namespace FinanceAPIData.Tasks
{
    public class LogoCalculatorTask : BaseTask
    {
        public LogoCalculatorTask(IOptions<TaskSettings> settings) : base(settings)
        {
        }

        public override void Execute(Task Task)
        {
            var calculator = new TransactionLogoCalculator(Settings.MongoDB_ConnectionString, Settings.LogoOverrides);
            var filterClientId = Task.Data["ClientID"]?.ToString();
            var filterAccountId = Task.Data["AccountID"]?.ToString();
            
            // Run Calculator
            calculator.Run(filterClientId, filterAccountId);
            
            base.Execute(Task);
        }
    }
}