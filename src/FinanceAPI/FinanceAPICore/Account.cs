﻿using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FinanceAPICore
{
    public class Account
    {
        [BsonId]
        public string ID;
		[JsonIgnore]
        public string ClientID;
        public string AccountName;
        public decimal? AvailableBalance;
        public decimal? PendingBalance;

        public Account()
		{
		}
        public Account(string ID,string AccountName)
        {
            this.ID = ID;
            this.AccountName = AccountName;
        }

        public Account(string AccountName)
        {
            this.AccountName = AccountName;
        }

        public static Account CreateFromJson(JObject jAccount, string clientId)
        {
            Account account = new Account();
            account.ID = jAccount["ID"]?.ToString();
            account.AccountName = jAccount["AccountName"]?.ToString();
            account.AvailableBalance = decimal.TryParse(jAccount["AvailableBalance"]?.ToString(), out decimal availableBalance) ? availableBalance as decimal? : null;
            account.PendingBalance = decimal.TryParse(jAccount["PendingBalance"]?.ToString(), out decimal pendingBalance) ? pendingBalance as decimal? : null;
            account.ClientID = clientId;
            return account;
        }
    }
}