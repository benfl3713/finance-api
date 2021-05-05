using Microsoft.Extensions.Options;

namespace FinanceAPICore.DataService
{
    public abstract class BaseDataService
    {
        protected readonly AppSettings _appSettings;
        public BaseDataService(IOptions<AppSettings> appSettings)
        {
            _appSettings = new AppSettings();
        }
    }
}