using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

namespace FinanceAPICore
{
    public class Transaction
    {
        [BsonIgnore]
        [JsonIgnore]
        private UID _uid;

        [BsonId]
        [JsonIgnore]
        public UID Uid
        {
            get
            {
                if (_uid == null)
                    _uid = new UID(ID, AccountID, ClientID);
                return _uid;
            }
            set => _uid = value;
        }
        
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
        [JsonIgnore]
        public string Owner = "User";
        [JsonIgnore]
        public string Source = "User";


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

        public class UID
        {
            public string ID;
            public string AccountID;
            public string ClientID;

            public UID(string id, string accountId, string clientId)
            {
                ID = id;
                AccountID = accountId;
                ClientID = clientId;
            }
        }
    }

    public enum Status
    {
        SETTLED,
        PENDING
    }
    
    public class OldTransaction
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
        [JsonIgnore]
        public string Owner = "User";

        // [BsonIgnore] 
        // [JsonIgnore] 
        // private Transaction.UID _uid;
        // [JsonIgnore]
        // public Transaction.UID Uid
        // {
        //     get
        //     {
        //         if (_uid == null)
        //             _uid = new Transaction.UID(ID, AccountID, ClientID);
        //         return _uid;
        //     }
        //     set => _uid = value;
        // }


        public OldTransaction()
        {
        }
        public OldTransaction(string id,DateTime date,string accountID, string category, decimal amount, string vendor,string merchant="", string type = "", string note = "", string logo = "")
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
        }

        public static OldTransaction CreateFromJson(JObject jTransaction, string clientId)
        {
            OldTransaction transaction = new OldTransaction();
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
}
