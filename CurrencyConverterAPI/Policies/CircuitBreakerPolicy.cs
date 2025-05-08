using Polly.Extensions.Http;
using Polly;

namespace CurrencyConverterAPI.Policies
{
    public static class CircuitBreakerPolicy
    {
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3, // After 3 failures
                    durationOfBreak: TimeSpan.FromSeconds(30), // Stay open for 30 seconds
                    onBreak: (outcome, timespan) =>
                    {
                        Console.WriteLine($"Circuit opened for {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    },
                    onReset: () => Console.WriteLine("Circuit closed - API is healthy again."),
                    onHalfOpen: () => Console.WriteLine("Circuit half-open - testing API again.")
                );
        }

    }
}
