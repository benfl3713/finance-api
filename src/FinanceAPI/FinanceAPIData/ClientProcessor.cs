using System;
using FinanceAPICore;
using FinanceAPICore.DataService;

namespace FinanceAPIData
{
	public class ClientProcessor
	{
		IClientDataService _clientDataService;

		public ClientProcessor(IClientDataService clientDataService)
		{
			_clientDataService = clientDataService;
		}
		public string InsertClient(Client client)
		{
			if (_clientDataService.GetClientByUsername(client.Username) != null)
				throw new ArgumentException("Username is already taken");
			
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
