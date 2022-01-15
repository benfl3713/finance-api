using System.Collections.Generic;
using FinanceAPICore.Wealth;

namespace FinanceAPICore.DataService.Wealth
{
    public interface ITradeDataService
    {
        public bool Insert(Trade trade);
        public bool Update(Trade trade);
        public bool Delete(string id, string clientId);
        public List<Trade> GetTrades(string clientId);
        public Trade GetTradeById(string id, string clientId);
    }
}