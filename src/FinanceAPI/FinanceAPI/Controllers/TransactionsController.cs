using System.ComponentModel.DataAnnotations;
using FinanceAPI.Attributes;
using FinanceAPICore;
using FinanceAPIData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FinanceAPI.Controllers
{
	[Route("api/transaction")]
	[ApiController]
	[Authorize]
	public class TransactionsController : Controller
	{
		private TransactionProcessor _transactionProcessor;
		public TransactionsController(TransactionProcessor transactionProcessor)
		{
			_transactionProcessor = transactionProcessor;
		}

		[HttpGet]
		public IActionResult GetTransactions(string accountId = null)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return Json(_transactionProcessor.GetTransactions(clientId, accountId));
		}
		
		[HttpPost]
		public IActionResult InsertTransaction([FromBody] JObject jsonTransaction)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			Transaction transaction = Transaction.CreateFromJson(jsonTransaction, clientId);
			string transactionId = _transactionProcessor.InsertTransaction(transaction);
			if (transactionId != null && !transactionId.StartsWith("ERROR:"))
				return Json(transactionId);
			return BadRequest(transactionId);
		}

		[HttpPut("{transactionId}")]
		public IActionResult UpdateTransaction([Required][FromBody] JObject jsonTransaction)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			Transaction transaction = Transaction.CreateFromJson(jsonTransaction, clientId);
			if (string.IsNullOrEmpty(transaction.ID))
				return BadRequest("Transaction ID is required");

			if (_transactionProcessor.UpdateTransaction(transaction))
				return Json("Transaction Updated");
			return BadRequest();
		}

		[HttpGet("{transactionId}")]
		public IActionResult GetTransactionById([FromRoute(Name = "transactionId")][Required] string transactionId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			Transaction transaction = _transactionProcessor.GetTransactionById(transactionId, clientId);
			if (transaction == null)
				return BadRequest("Could not find Transaction");
			return Json(transaction);
		}

		[HttpDelete("{transactionId}")]
		public IActionResult DeleteTransaction([FromRoute(Name = "transactionId")][Required] string transactionId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			if (_transactionProcessor.DeleteTransaction(transactionId, clientId))
				return Json("Transaction Deleted");
			return BadRequest("Failed to delete Transaction");
		}
	}
}
