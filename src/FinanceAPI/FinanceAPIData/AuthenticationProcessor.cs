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
			string hashedPassword = PasswordHasher.Hash(password);
			return _clientDataService.LoginClient(username, hashedPassword);
		}
	}
}
