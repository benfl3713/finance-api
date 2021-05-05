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
        public Dictionary<string, StatisticsProcessor.AccountBalanceHistory> GetBalanceHistory(string accountId = null, DateTime? dateFrom = null)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _statisticsProcessor.GetBalanceHistoryV2(clientId, accountId, dateFrom);
        }

        [HttpGet("[action]")]
        public Dictionary<string, decimal> GetSpentAmountPerCategory()
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _statisticsProcessor.GetSpentAmountPerCategory(clientId);
        }
    }
}