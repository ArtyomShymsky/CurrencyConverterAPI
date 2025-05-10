using CurrencyConverterAPI.DTOs;

namespace CurrencyConverterAPI.Intefaces
{
    public interface ICurrencyConversionService
    {
        Task<ExchangeRateResponse> GetRatesAsync(string from, string to);
        Task<decimal?> ConvertCurrency(decimal amount, string from, string to);
    }
}
