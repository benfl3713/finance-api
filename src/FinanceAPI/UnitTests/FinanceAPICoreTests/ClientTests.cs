using System.Collections.Generic;
using FinanceAPICore;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FinanceAPICoreTests
{
    public class ClientTests
    {
        [TestCaseSource(typeof(TestCases), nameof(TestCases.CreateFromJsonTestData))]
        public void TestCreateFromJson(string jsonClient, Client expectedClient)
        {
            Client actualClient = Client.CreateFromJson(JObject.Parse(jsonClient));
            Assert.That(actualClient.ID, Is.EqualTo(expectedClient.ID));
            Assert.That(actualClient.FirstName, Is.EqualTo(expectedClient.FirstName));
            Assert.That(actualClient.LastName, Is.EqualTo(expectedClient.LastName));
            Assert.That(actualClient.Username, Is.EqualTo(expectedClient.Username));
        }

        private class TestCases
        {

            public static IEnumerable<TestCaseData> CreateFromJsonTestData
            {
                get
                {
                    yield return new TestCaseData(
                        "{\"ID\":\"TESTCLIENT1\",\"FirstName\":\"Test\",\"LastName\":\"Tester\",\"Username\":\"testers\"}",
                        new Client
                        {
                            ID = "TESTCLIENT1",
                            FirstName = "Test",
                            LastName = "Tester",
                            Username = "testers"
                        });
                    yield return new TestCaseData(
                        "{\"ID\":\"TESTCLIENT2\",\"Username\":\"testers\"}",
                        new Client
                        {
                            ID = "TESTCLIENT2",
                            Username = "testers"
                        });
                    yield return new TestCaseData(
                        "{\"ID\":\"TESTCLIENT3\"}",
                        new Client
                        {
                            ID = "TESTCLIENT3",
                        });
                    yield return new TestCaseData(
                        "{\"Username\":\"testers\"}",
                        new Client
                        {
                            Username = "testers",
                        });
                }
            }
        }
    }
}