using CurrencyConverterAPI.DTOs;
using CurrencyConverterAPI.Intefaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace CurrencyConverterAPI.Services
{
    public class ExchangeService: ICurrencyConversionService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public ExchangeService(IHttpClientFactory factory, IMemoryCache cache)
        {
            _httpClient = factory.CreateClient("Frankfurter");
            _cache = cache;
        }

        public async Task<ExchangeRateResponse> GetRatesAsync(string from, string to)
        {
            string cacheKey = $"exchange_{from}_{to}";

            if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse cachedRate))
            {
                return cachedRate;
            }

            var response = await _httpClient.GetAsync($"latest?from={from}&to={to}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var rate = JsonSerializer.Deserialize<ExchangeRateResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Cache for 10 minutes
            _cache.Set(cacheKey, rate, TimeSpan.FromMinutes(10));

            return rate;
        }
    }

}
