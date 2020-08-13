using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FinanceAPICore
{
    public class Account
    {
        [BsonId]
        public string AccountID;
        public string AccountName;
        public double CurrentBalance;

        public Account(string ID,string AccountName)
        {
            AccountID = ID;
            this.AccountName = AccountName;
        }

        public Account(string AccountName)
        {
            this.AccountName = AccountName;
        }
    }
}
