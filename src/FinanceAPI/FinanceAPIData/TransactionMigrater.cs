using System;
using System.Collections.Generic;
using FinanceAPICore;

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
                    List<OldTransaction> oldTransactions = transactionDataService.GetOldTransactions(client.ID);
                    foreach (OldTransaction oldTransaction in oldTransactions)
                    {
                        Transaction newTransaction = new Transaction
                        {
                            ID = oldTransaction.ID,
                            Amount = oldTransaction.Amount,
                            Category = oldTransaction.Category,
                            Currency = oldTransaction.Currency,
                            Date = oldTransaction.Date,
                            Logo = oldTransaction.Logo,
                            Merchant = oldTransaction.Merchant,
                            Note = oldTransaction.Note,
                            Owner = oldTransaction.Owner,
                            Status = oldTransaction.Status,
                            Type = oldTransaction.Type,
                            Vendor = oldTransaction.Vendor,
                            AccountID = oldTransaction.AccountID,
                            ClientID = oldTransaction.ClientID
                        };

                        transactionDataService.DeleteOldTransaction(oldTransaction.ID, client.ID);
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