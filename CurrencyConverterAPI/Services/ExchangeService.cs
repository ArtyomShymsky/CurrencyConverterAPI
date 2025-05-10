using CurrencyConverterAPI.DTOs;
using CurrencyConverterAPI.Intefaces;
using Microsoft.Extensions.Caching.Distributed;
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

        public async Task<decimal?> ConvertCurrency(decimal amount, string from, string to)
        {
            string cacheKey = $"{amount}-{from}-{to}";

            if (_cache.TryGetValue(cacheKey, out decimal cachedResult))
            {
                return cachedResult;
            }

            try
            {
                string url = $"https://api.frankfurter.app/latest?amount={amount}&from={from}&to={to}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement rates = doc.RootElement.GetProperty("rates");

                if (rates.TryGetProperty(to, out JsonElement rateElement))
                {
                    decimal result = rateElement.GetDecimal();

                    // Cache the result for 10 minutes
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

                    return result;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


    }

}
