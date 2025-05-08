using CurrencyConverterAPI.DTOs;
using CurrencyConverterAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverterAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrencyRatesController : ControllerBase
    {
        private readonly CurrencyRateService _service;
        public CurrencyRatesController(CurrencyRateService service)
        {
            _service = service;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRates([FromQuery] string from = "USD")
        {

            var result = await _service.GetLatestRatesAsync(from);
            if (result == null)
                return BadRequest("Invalid base currency or no data found.");


            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistoricalRates(
        [FromQuery] string from = "USD",
        [FromQuery] DateOnly? start = null,
        [FromQuery] DateOnly? end = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            if (!start.HasValue || !end.HasValue)
                return BadRequest("Start and end dates are required.");

            if (start > end)
                return BadRequest("Start date must be before end date.");

            var data = await _service.GetHistoricalRatesAsync(from, start.Value, end.Value);
            if (data == null || data.Count == 0)
                return NotFound("No data available for the selected period.");

            int totalItems = data.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedRates = data
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginatedHistoricalRatesResponse
            {
                Base = from.ToUpper(),
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Rates = pagedRates
            };

            return Ok(response);
        }






    }
}
