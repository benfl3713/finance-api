using FinanceAPICore;
using FinanceAPICore.DataService;
using System;

namespace FinanceAPIData
{
	public class ClientProcessor
	{
		IClientDataService _clientDataService = new FinanceAPIMongoDataService.DataService.ClientDataService();
		public string InsertClient(Client client)
		{
			// Force client id to be empty
			client.ID = Guid.NewGuid().ToString();
			client.Password = FinanceAPICore.Utilities.PasswordHasher.Hash(client.Password);
			return _clientDataService.InsertClient(client) ? client.ID : null;
		}

		public Client GetClientById(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
				return null;
			return _clientDataService.GetClientById(clientId);
		}

		public bool UpdateClient(Client client)
		{
			client.Password = FinanceAPICore.Utilities.PasswordHasher.Hash(client.Password);
			return _clientDataService.UpdateClient(client);
		}

		public bool DeleteClient(string clientId)
		{
			if(!string.IsNullOrEmpty(clientId))
				return _clientDataService.DeleteClient(clientId);
			return false;
		}
	}
}
