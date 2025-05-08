using CurrencyConverterAPI.DTOs;

namespace CurrencyConverterAPI.Intefaces
{
    public interface ICurrencyRateService
    {
        Task<CurrencyRatesResponse?> GetLatestRatesAsync(string baseCurrency = "USD");
    }
}
