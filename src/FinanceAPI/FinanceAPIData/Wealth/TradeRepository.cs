using System;
using System.Collections.Generic;
using FinanceAPICore.DataService.Wealth;
using FinanceAPICore.Wealth;

namespace FinanceAPIData.Wealth
{
    public class TradeRepository
    {
        private readonly ITradeDataService _tradeDataService;
        public TradeRepository(ITradeDataService tradeDataService)
        {
            _tradeDataService = tradeDataService;
        }
        
        public List<Trade> GetTrades(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentException("Unknown Client");

            return _tradeDataService.GetTrades(clientId);
        }

        public bool InsertTrade(Trade trade)
        {
            if (string.IsNullOrEmpty(trade.Id))
                trade.Id = Guid.NewGuid().ToString();
            Trade.Validate(trade);
            return _tradeDataService.Insert(trade);
        }

        public bool UpdateTrade(Trade trade)
        {
            Trade.Validate(trade);
            if (trade.Id == null)
                throw new ArgumentException("Trade Id is required", nameof(trade.Id));

            return _tradeDataService.Update(trade);
        }

        public Trade GetTradeById(string id, string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id), "id is required");

            return _tradeDataService.GetTradeById(id, clientId);
        }

        public bool DeleteTrade(string id, string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id), "id is required");

            return _tradeDataService.Delete(id, clientId);
        }

        public bool ImportTrade(Trade trade)
        {
            try
            {
                Trade existingTrade = GetTradeById(trade.Id, trade.ClientID);
                if (existingTrade != null)
                {
                    if (existingTrade.Owner == "User")
                        return false;

                    if (!string.IsNullOrEmpty(existingTrade.Note))
                        trade.Note = existingTrade.Note;

                    return UpdateTrade(existingTrade);
                }
            }
            catch
            {
                // Means we could not find the trade
            }

            return InsertTrade(trade);
        }
    }
}