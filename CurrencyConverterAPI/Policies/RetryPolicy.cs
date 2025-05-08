using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace CurrencyConverterAPI.Policies
{
    public static class RetryPolicy
    {
       public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // 5xx, 408, network errors
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) // 429 rate limit
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s due to {outcome?.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    });
        }

    }
}
