using CurrencyConverterAPI.DTOs;

namespace CurrencyConverterAPI.Intefaces
{
    public interface ICurrencyConversionService
    {
        Task<ExchangeRateResponse> GetRatesAsync(string from, string to);
    }
}
