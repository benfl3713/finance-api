using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanceAPICore.Wealth
{
    public class Trade
    {
        [BsonId]
        public string Id { get; set; }
        public string ClientID;
        public string AssetId { get; set; }
        public DateTime TradeDateTime { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        [BsonIgnore] 
        public string Type => Amount < 0 ? "Sale" : "Buy";

        public string Note;
        public string Source { get; set; }
        public string Owner { get; set; }
        public Dictionary<string, string> ExtraDetails { get; set; }
        public static void Validate(Trade trade)
        {
            if (trade == null)
                throw new ArgumentNullException(nameof(trade));
            if (string.IsNullOrEmpty(trade.ClientID))
                throw new ArgumentNullException(nameof(trade.ClientID));

            if (string.IsNullOrEmpty(trade.Owner))
                trade.Owner = "User";
        }
    }
}