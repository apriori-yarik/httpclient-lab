using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace HttpClientLab.Clients.GitHub
{
    public static class Policies
    {
        public static IAsyncPolicy<HttpResponseMessage> Retry() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == (HttpStatusCode)429)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                    {
                        var baseDelay = TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryAttempt - 1));
                        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 200));
                        return baseDelay + jitter;
                    });

        public static IAsyncPolicy<HttpResponseMessage> Timeout(TimeSpan? perTry = null) =>
            Policy.TimeoutAsync<HttpResponseMessage>(perTry ?? TimeSpan.FromSeconds(10));

        public static IAsyncPolicy<HttpResponseMessage> CircuitBreaker() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
