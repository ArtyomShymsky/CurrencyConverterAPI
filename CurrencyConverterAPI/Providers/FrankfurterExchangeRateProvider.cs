using System.Text.Json;

namespace CurrencyConverterAPI.Providers
{
    public class FrankfurterExchangeRateProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://api.frankfurter.app/";

        public FrankfurterExchangeRateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            var url = $"{ApiBaseUrl}{DateTime.UtcNow:yyyy-MM-dd}?from={fromCurrency}&to={toCurrency}";

            var response = await _httpClient.GetStringAsync(url);
            var exchangeRateData = JsonSerializer.Deserialize<FrankfurterExchangeRateResponse>(response);

            if (exchangeRateData?.Rates == null || !exchangeRateData.Rates.ContainsKey(toCurrency))
                throw new Exception("Exchange rate data is unavailable.");

            return exchangeRateData.Rates[toCurrency];
        }

        public class FrankfurterExchangeRateResponse
        {
            public Dictionary<string, decimal> Rates { get; set; }
        }
    }

}
