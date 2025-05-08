using CurrencyConverterAPI.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace CurrencyConverterAPI.Services
{
    public class CurrencyRateService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public CurrencyRateService(IHttpClientFactory factory, IMemoryCache cache)
        {
            _httpClient = factory.CreateClient("Frankfurter");
            _cache = cache;
        }

        public async Task<CurrencyRatesResponse?> GetLatestRatesAsync(string baseCurrency = "USD")
        {
            string cacheKey = $"all_rates_{baseCurrency.ToUpper()}";

            if (_cache.TryGetValue(cacheKey, out CurrencyRatesResponse cached))
                return cached;

            var response = await _httpClient.GetAsync($"latest?from={baseCurrency}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CurrencyRatesResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null)
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));

            return result;
        }
        public async Task<List<HistoricalRate>> GetHistoricalRatesAsync(string baseCurrency, DateOnly start, DateOnly end)
        {
            string cacheKey = $"history_{baseCurrency}_{start}_{end}";

            if (_cache.TryGetValue(cacheKey, out List<HistoricalRate> cachedRates))
                return cachedRates;

            string endpoint = $"{start:yyyy-MM-dd}..{end:yyyy-MM-dd}?from={baseCurrency}";

            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var rawData = JsonDocument.Parse(json);

            var ratesNode = rawData.RootElement.GetProperty("rates");

            var result = new List<HistoricalRate>();

            foreach (var dateEntry in ratesNode.EnumerateObject())
            {
                string date = dateEntry.Name;
                foreach (var rateEntry in dateEntry.Value.EnumerateObject())
                {
                    result.Add(new HistoricalRate
                    {
                        Date = date,
                        Currency = rateEntry.Name,
                        Rate = rateEntry.Value.GetDecimal()
                    });
                }
            }

            // Cache the result for 30 minutes
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));

            return result;
        }



    }

}
