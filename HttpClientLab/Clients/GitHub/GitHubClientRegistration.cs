using Refit;
using System.Net.Http.Headers;

namespace HttpClientLab.Clients.GitHub
{
    public static class GitHubClientRegistration
    {
        public static IServiceCollection AddGitHubClient(this IServiceCollection services, RefitSettings? settings = null)
        {
            settings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            };

            services
                .AddRefitClient<IGitHubApi>(settings)
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri("https://api.github.com");
                    c.DefaultRequestHeaders.UserAgent.ParseAdd("HttpClientLab/1.0 (+https://github.com/)");
                    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                    c.Timeout = TimeSpan.FromSeconds(30);
                })
                .AddPolicyHandler(Policies.Timeout(TimeSpan.FromSeconds(10)))
                .AddPolicyHandler(Policies.Retry())
                .AddPolicyHandler(Policies.CircuitBreaker());

            return services;
        }
    }
}
