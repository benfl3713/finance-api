using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FinanceAPICore.Wealth
{
    public class Asset
    {
        [BsonId]
        public string Id { get; set; }
        public string Name { get; set; }
        public string ClientId { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Source { get; set; }
        public string Owner { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public AssetTypes Type { get; set; }
        public string Code { get; set; }
        public string Exchange { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public Dictionary<string, string> MarketIdentifiers { get; set; }

        public static void Validate(Asset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));
            if (string.IsNullOrEmpty(asset.ClientId))
                throw new ArgumentNullException(nameof(asset.ClientId));
            if (string.IsNullOrEmpty(asset.Name))
                throw new ArgumentNullException(nameof(asset.Name));

            if (string.IsNullOrEmpty(asset.Owner))
                asset.Owner = "User";
        }

        public enum AssetTypes
        {
            Unknown,
            Stock,
            Cash,
            Crypto,
            Fiat
        }
    }
}