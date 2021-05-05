using System;
using System.Collections.Generic;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPIData;
using Moq;
using NUnit.Framework;

namespace FinanceAPIDataTests
{
    public class TransactionLogoCalculatorTests
    {
        [TestCaseSource(typeof(TestCases), nameof(TestCases.RunTestData))]
        public void RunTest(Transaction originalTransaction, string expectedLogo)
        {
            TransactionLogoCalculatorMock mock = new TransactionLogoCalculatorMock();
            Dictionary<string, string> transactionLogos = new Dictionary<string, string>();
            // Setup mocks
            Mock<ITransactionsDataService> mockITransactionsDataService = new Mock<ITransactionsDataService>();
            mockITransactionsDataService.Setup(db => db.GetTransactions(It.IsAny<string>())).Returns(new List<Transaction>{originalTransaction});
            mockITransactionsDataService.Setup(db => db.UpdateTransactionLogo(It.IsAny<string>(), It.IsAny<string>())).Callback((string id, string logo) => transactionLogos.Add(id, logo)).Returns(true);
            mock._transactionsDataService = mockITransactionsDataService.Object;
            
            Mock<IClientDataService> mockIClientDataService = new Mock<IClientDataService>();
            mockIClientDataService.Setup(db => db.GetAllClients()).Returns(new List<Client>{new Client{ID = "TEST"}});
            mock._clientDataService = mockIClientDataService.Object;
            
            // Run Test
            mock.Run();
            Assert.That(transactionLogos[originalTransaction.ID], Is.EqualTo(expectedLogo));
        }

        private class TestCases
        {

            public static IEnumerable<TestCaseData> RunTestData
            {
                get
                {
                    yield return new TestCaseData(new Transaction("TEST1", DateTime.Today, "TESTACCOUNT1", "Test", 5, "Test", type: "Adjust"), "/assets/logo_square.png");
                    yield return new TestCaseData(new Transaction("TEST2", DateTime.Today, "TESTACCOUNT1", "Test", 5, "Adjust", type: "Test"), "/assets/logo_square.png");
                    yield return new TestCaseData(new Transaction("TEST3", DateTime.Today, "TESTACCOUNT1", "Test", 5, "Test", "Adjust"), "/assets/logo_square.png");
                    yield return new TestCaseData(new Transaction("TEST4", DateTime.Today, "TESTACCOUNT1", "Test", 5, "Test", "Test"), "https://logo.clearbit.com/Test.com");
                }
            }
        }
        
        private class TransactionLogoCalculatorMock : TransactionLogoCalculator
        {
            public TransactionLogoCalculatorMock(Dictionary<string, Logo> logoOverrides = null) : base(null, null, logoOverrides)
            {
                _logoOverrides = logoOverrides ?? new Dictionary<string, Logo>
                {
                    {
                        "Adjust", new Logo("/assets/logo_square.png", true, new List<string> {"Type", "Vendor", "Merchant"})
                    }
                };
            }

            public new ITransactionsDataService _transactionsDataService
            {
                set => base._transactionsDataService = value;
            }

            public new IClientDataService _clientDataService
            {
                set => base._clientDataService = value;
            }

            protected override bool DoesImageExit(string url)
            {
                return true;
            }
        }
    }
}