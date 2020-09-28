using System;
using System.Collections.Generic;
using FinanceAPI.Attributes;
using FinanceAPIData;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    [Authorize]
    public class StatisticsController : Controller
    {
        private StatisticsProcessor _statisticsProcessor;
        public StatisticsController(StatisticsProcessor statisticsProcessor)
        {
            _statisticsProcessor = statisticsProcessor;
        }
        
        [HttpGet("[action]")]
        public Dictionary<string, StatisticsProcessor.AccountBalanceHistory> GetBalanceHistory(string accountId = null)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _statisticsProcessor.GetBalanceHistory(clientId, accountId);
        }
    }
}