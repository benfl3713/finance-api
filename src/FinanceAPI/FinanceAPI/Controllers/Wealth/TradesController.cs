using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FinanceAPI.Attributes;
using FinanceAPICore.Wealth;
using FinanceAPIData.Wealth;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers.Wealth
{
    [ApiController]
    [Route("api/wealth/trades")]
    [Authorize]
    [Produces("application/json")]
    public class TradesController : Controller
    {
        private readonly TradeRepository _tradeRepository;

        public TradesController(TradeRepository tradeRepository)
        {
            _tradeRepository = tradeRepository;
        }

        [HttpGet]
        public List<Trade> GetTrades()
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _tradeRepository.GetTrades(clientId)?.OrderByDescending(t => t.TradeDateTime).ToList();
        }

        [HttpGet("{tradeId}")]
        public Trade GetTradeById([FromRoute(Name = "tradeId")] [Required] string tradeId)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _tradeRepository.GetTradeById(tradeId, clientId);
        }

        [HttpPost]
        public string InsertTrade([Required] Trade trade)
        {
            trade.ClientID = Request.HttpContext.Items["ClientId"]?.ToString();
            trade.Id = Guid.NewGuid().ToString();
            return _tradeRepository.InsertTrade(trade) ? trade.Id : null;
        }

        [HttpPut]
        [HttpPut("{tradeId}")]
        public bool UpdateTrade([Required] Trade trade)
        {
            trade.ClientID = Request.HttpContext.Items["ClientId"]?.ToString();
            return _tradeRepository.UpdateTrade(trade);
        }

        [HttpDelete("{tradeId}")]
        public bool DeleteTrad([FromRoute(Name = "tradeId")] [Required] string tradeId)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _tradeRepository.DeleteTrade(tradeId, clientId);
        }
    }
}