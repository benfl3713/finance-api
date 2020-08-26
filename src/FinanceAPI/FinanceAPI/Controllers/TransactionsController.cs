using System;
using FinanceAPICore;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
	[Route("api/{clientId}/transactions")]
	[ApiController]
	public class TransactionsController : Controller
	{
		[HttpGet]
		public IActionResult GetTransactions([FromRoute(Name = "clientId")] string clientId)
		{
			return Json(new Transaction("Testid", DateTime.Today, "fsfsdfsdf", "Test Account", "Transfer", 59.48m, "Test"));
		}
	}
}
