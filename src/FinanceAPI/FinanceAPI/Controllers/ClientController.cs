using System.ComponentModel.DataAnnotations;
using FinanceAPI.Attributes;
using FinanceAPICore;
using FinanceAPIData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FinanceAPI.Controllers
{
	[Route("api/client")]
	[ApiController]
	public class ClientController : Controller
	{
		private ClientProcessor _clientProcessor;
		public ClientController(ClientProcessor clientProcessor)
		{
			_clientProcessor = clientProcessor;
		}
		[HttpPost]
		public IActionResult InsertClient([FromBody] JObject jsonClient)
		{
			Client client = Client.CreateFromJson(jsonClient);
			client.Password = jsonClient["Password"]?.ToString();
			if (string.IsNullOrEmpty(client.Username) || string.IsNullOrEmpty(client.Password))
				return BadRequest("Username and Password Required");

			string clientId = _clientProcessor.InsertClient(client);
			if (clientId != null)
				return Json(clientId);
			return BadRequest();
		}

		[HttpPut]
		[Authorize]
		public IActionResult UpdateClient([FromBody] JObject jsonClient)
		{
			Client client = Client.CreateFromJson(jsonClient);
			if (string.IsNullOrEmpty(client.ID))
				return BadRequest("Client ID is required");

			if (_clientProcessor.UpdateClient(client))
				return Json("Client Updated");
			return BadRequest();
		}

		[HttpGet("{clientId}")]
		[Authorize]
		public IActionResult GetClientById([FromRoute(Name = "clientId")][Required] string clientId)
		{
			Client client = _clientProcessor.GetClientById(clientId);
			if (client == null)
				return BadRequest("Could not find client");
			return Json(client);
		}

		[HttpDelete("{clientId}")]
		[Authorize]
		public IActionResult DeleteClient([FromRoute(Name = "clientId")][Required] string clientId)
		{
			if (_clientProcessor.DeleteClient(clientId))
				return Json("Client Deleted");
			return BadRequest("Failed to delete client");
		}
	}
}
