using System;

namespace FinanceAPICore.Wealth
{
    public class AssetPrice : Asset
    {
        public DateTime DateTimeAsAt { get; set; }

        public AssetPrice(Asset asset, DateTime asAt)
        {
            DateTimeAsAt = asAt;
            Id = $"{asset.Id}_{asAt:ddMMyyyyHHmmss}";
            Name = asset.Name;
            ClientId = asset.ClientId;
            LastUpdated = asAt;
            Source = asset.Source;
            Owner = asset.Owner;
            Type = asset.Type;
            Code = asset.Code;
            Exchange = asset.Exchange;
            Currency = asset.Currency;
            Balance = asset.Balance;
            MarketIdentifiers = asset.MarketIdentifiers;
        }
    }
}