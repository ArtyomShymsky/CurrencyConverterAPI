using Asp.Versioning;
using CurrencyConverterAPI.DTOs;
using CurrencyConverterAPI.Intefaces;
using CurrencyConverterAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Polly.CircuitBreaker;

namespace CurrencyConverterAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class CurrencyRatesController : ControllerBase
    {
        private readonly ICurrencyRateService _service;
        public CurrencyRatesController(ICurrencyRateService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet("latest")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetLatestRates([FromQuery] string from = "USD")
        {
            var username = User.Identity?.Name;

            try
            {
                var result = await _service.GetLatestRatesAsync(from);
                if (result == null)
                    return NotFound("Unable to retrieve latest currency rates.");

                return Ok(result);
            }
            catch (BrokenCircuitException)
            {
                return StatusCode(503, "Service temporarily unavailable (circuit breaker open).");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("history")]
        [MapToApiVersion("1.0")]
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
