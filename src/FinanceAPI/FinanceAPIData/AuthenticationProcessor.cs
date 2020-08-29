using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData
{
	public class AuthenticationProcessor
	{
		IClientDataService _clientDataService = new FinanceAPIMongoDataService.DataService.ClientDataService();
		public Client AuthenticateClient(string username, string password)
		{
			Client client = _clientDataService.GetClientByUsername(username);
			if (client != null && PasswordHasher.Verify(password, client.Password))
				return client;

			return null;
		}
	}
}
