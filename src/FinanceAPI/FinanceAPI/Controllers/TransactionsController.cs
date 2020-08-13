using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceAPICore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
	[Route("api/{clientId}/transactions")]
	[ApiController]
	public class TransactionsController : Controller
	{
		[HttpGet]
		public IActionResult GetTransactions([FromRoute] string clientId)
		{
			return Json(new Transaction("Testid", DateTime.Today, "fsfsdfsdf", "Test Account", "Transfer", 59.48m, "Test"));
		}
	}
}
