using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Utilities;

namespace FinanceAPIData
{
	public class AuthenticationProcessor
	{
		private readonly IClientDataService _clientDataService;

		public AuthenticationProcessor(IClientDataService clientDataService)
		{
			_clientDataService = clientDataService;
		}
		public Client AuthenticateClient(string username, string password)
		{
			Client client = _clientDataService.GetClientByUsername(username);
			if (client != null && PasswordHasher.Verify(password, client.Password))
				return client;

			return null;
		}
	}
}
