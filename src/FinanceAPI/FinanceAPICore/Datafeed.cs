using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FinanceAPICore
{
    public class Datafeed
    {
        [BsonId]
        public string _id { get; set; }
        public string ClientId { get; set; }
        public string Provider { get; set; }
        public string VendorID { get; set; }
        public string VendorName { get; set; }
        public string AccessKey { get; set; }
        public string RefreshKey { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool NeedsReconnecting { get; set; }


		public Datafeed()
		{
		}
        public Datafeed(string clientId, string provider, string vendorID, string vendorName, DateTime lastUpdated)
        {
            ClientId = clientId;
            Provider = provider;
            VendorID = vendorID;
            VendorName = vendorName;
            LastUpdated = lastUpdated;
            _id = $"{ClientId}_{Provider}_{VendorID}";
        }

        public Datafeed(string clientId, string provider, string vendorID, string vendorName, DateTime lastUpdated, string accessKey, string refreshKey)
        {
            ClientId = clientId;
            Provider = provider;
            VendorID = vendorID;
            VendorName = vendorName;
            LastUpdated = lastUpdated;
            AccessKey = accessKey;
            RefreshKey = refreshKey;
            _id = $"{ClientId}_{Provider}_{VendorID}";
        }
    }
}
