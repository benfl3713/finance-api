using System;
using System.Collections.Generic;
using FinanceAPICore;
using MongoDB.Bson;

namespace FinanceAPIData
{
    public class TransactionMigrater
    {
        public void Run(string connectionString)
        {
            try
            {
                Console.WriteLine("Starting Transaction Migrator");
                var transactionDataService = new FinanceAPIMongoDataService.DataService.TransactionsDataService(connectionString);
                var clientDataService = new FinanceAPIMongoDataService.DataService.ClientDataService(connectionString);
                foreach (Client client in clientDataService.GetAllClients())
                {
                    List<BsonDocument> oldTransactions = transactionDataService.GetOldTransactions(client.ID);
                    foreach (BsonDocument oldTransaction in oldTransactions)
                    {
                        if(oldTransaction["_id"].BsonType == BsonType.Document)
                            continue;
                        
                        Transaction newTransaction = new Transaction
                        {
                            ID = oldTransaction["_id"].ToString(),
                            Amount = decimal.Parse(oldTransaction["Amount"].ToString()),
                            Category = oldTransaction["Category"].ToString(),
                            Currency = oldTransaction["Currency"].ToString(),
                            Date = DateTime.Parse(oldTransaction["Date"].ToString()),
                            Logo = null,
                            Merchant = oldTransaction["Merchant"].ToString(),
                            Note = oldTransaction["Note"].ToString(),
                            Owner = oldTransaction["Owner"].ToString(),
                            Status = Status.SETTLED,
                            Type = oldTransaction["Type"].ToString(),
                            Vendor = oldTransaction["Vendor"].ToString(),
                            AccountID = oldTransaction["AccountID"].ToString(),
                            ClientID = oldTransaction["ClientID"].ToString()
                        };

                        transactionDataService.DeleteOldTransaction(oldTransaction["_id"].ToString(), client.ID);
                        transactionDataService.InsertTransaction(newTransaction);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Finished Transaction Migrator");
        }
    }
}