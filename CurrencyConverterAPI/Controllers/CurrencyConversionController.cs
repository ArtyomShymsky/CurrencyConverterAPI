using CurrencyConverterAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace CurrencyConverterAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrencyConversionController : ControllerBase
    {
        private readonly ExchangeService _exchangeService;
        private readonly List<string> _excludedCurrencys = new List<string>() { "TRY", "PLN", "THB", "MXN" };
        public CurrencyConversionController(IMemoryCache cache, ExchangeService exchangeService)
        {
            _exchangeService = exchangeService;
        }


        [HttpGet("Exchange")]
        public async Task<IActionResult> GetExchangeRate([FromQuery] string from = "USD", [FromQuery] string to = "EUR")
        {
            if(_excludedCurrencys.Contains(from) || _excludedCurrencys.Contains(to))
            {
                return BadRequest();
            }

            var result = await _exchangeService.GetRatesAsync(from, to);
            return Ok(result);
        }

    }
}
