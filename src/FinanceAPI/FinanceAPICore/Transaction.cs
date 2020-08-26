using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore
{
    public class Transaction
    {
        [BsonId]
        public string ID;
        public DateTime Date;
        public string AccountID;
        public string AccountName;
        public string Category;
        public decimal Amount;
        public string Vendor;
        public string Merchant;
        public string Type;
        public string Note;
        public string Logo;

        public Transaction()
        {
        }
        public Transaction(string id,DateTime date,string accountID,string accountName, string category, decimal amount, string vendor,string merchant="", string type = "", string note = "", string logo = "")
        {
            ID = id;
            Date = date;
            AccountID = accountID;
            AccountName = accountName;
            Category = category;
            Amount = amount;
            Vendor = vendor;
            Merchant = merchant;
            Type = type;
            Note = note;
            Logo = logo;
            CalculateLogo();
        }

        public Transaction(string id, DateTime date, string accountID, string category, decimal amount, string vendor, string merchant = "", string type = "", string note = "", string logo = "")
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
    }
}
