using System.Collections.Generic;

namespace FinanceAPICore.DataService
{
	public interface IClientDataService
	{
		bool InsertClient(Client client);
		Client GetClientById(string clientId);
		bool UpdateClient(Client client);
		bool DeleteClient(string clientId);
		Client GetClientByUsername(string username);
		List<Client> GetAllClients();
	}
}
