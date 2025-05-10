using Asp.Versioning;
using CurrencyConverterAPI.Intefaces;
using CurrencyConverterAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace CurrencyConverterAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class CurrencyConversionController : ControllerBase
    {
        private readonly ICurrencyConversionService _exchangeService;
        private readonly List<string> _excludedCurrencys = new List<string>() { "TRY", "PLN", "THB", "MXN" };
        public CurrencyConversionController(ICurrencyConversionService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        [HttpGet("getexchangerate")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> GetExchangeRate([FromQuery] string from = "USD", [FromQuery] string to = "EUR")
        {
            if(_excludedCurrencys.Contains(from) || _excludedCurrencys.Contains(to))
            {
                return BadRequest();
            }

            var result = await _exchangeService.GetRatesAsync(from, to);
            if (result != null)
            {
                return Ok(result);

            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpGet("convertcurrency")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Convert(decimal amount, string from, string to)
        {
            var username = User.Identity?.Name;


            var result = await _exchangeService.ConvertCurrency(amount, from.ToUpper(), to.ToUpper());

            if (result.HasValue)
                return Ok(new { Amount = amount, From = from.ToUpper(), To = to.ToUpper(), Converted = result.Value });
            else
                return BadRequest("Currency conversion failed. Check parameters.");
        }


    }
}
