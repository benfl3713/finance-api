using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore
{
    public class Transaction
    {
        [BsonId]
        public string ID;
        [JsonIgnore]
        public string ClientID;
        public DateTime Date;
        public string AccountID;
        [BsonIgnore]
        public string AccountName;
        public string Category;
        public decimal Amount;
        public string Currency;
        public string Vendor;
        public string Merchant;
        public string Type;
        public string Note;
        public string Logo;
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status = Status.SETTLED;


        public Transaction()
        {
        }
        public Transaction(string id,DateTime date,string accountID, string category, decimal amount, string vendor,string merchant="", string type = "", string note = "", string logo = "")
        {
            ID = id;
            Date = date;
            AccountID = accountID;
            Category = category;
            Amount = amount;
            Vendor = vendor;
            Merchant = merchant;
            Type = type;
            Note = note;
            Logo = logo;
            CalculateLogo();
        }

        private void CalculateLogo()
        {
            if (string.IsNullOrEmpty(Logo))
            {
                if(!string.IsNullOrEmpty(Vendor))
                    Logo = $"https://logo.clearbit.com/{Vendor.Replace("'", "").Replace(" ", "").Replace(",", "")}.com";
                else if (!string.IsNullOrEmpty(Merchant))
                    Logo = $"https://logo.clearbit.com/{Merchant.Replace("'", "").Replace(" ", "").Replace(",", "")}.com";
            }
        }

        public static Transaction CreateFromJson(JObject jTransaction, string clientId)
        {
            Transaction transaction = new Transaction();
            transaction.ID = jTransaction["ID"]?.ToString();
            transaction.Date = DateTime.Parse(jTransaction["Date"]?.ToString());
            transaction.AccountID = jTransaction["AccountID"]?.ToString();
            transaction.Category = jTransaction["Category"]?.ToString();
            transaction.Amount = decimal.Parse(jTransaction["Amount"]?.ToString());
            transaction.Currency = jTransaction["Currency"]?.ToString();
            transaction.Vendor = jTransaction["Vendor"]?.ToString();
            transaction.Merchant = jTransaction["Merchant"]?.ToString();
            transaction.Type = jTransaction["Type"]?.ToString();
            transaction.Note = jTransaction["Note"]?.ToString();
            transaction.ClientID = clientId;
			Enum.TryParse(jTransaction["Status"]?.ToString()?.ToUpper(), out transaction.Status);
            return transaction;
        }
    }

    public enum Status
    {
        SETTLED,
        PENDING
    }
}
