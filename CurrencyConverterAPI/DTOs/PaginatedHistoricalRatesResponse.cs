namespace CurrencyConverterAPI.DTOs
{
    public class PaginatedHistoricalRatesResponse
    {
        public string Base { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<HistoricalRate> Rates { get; set; }
    }
}
