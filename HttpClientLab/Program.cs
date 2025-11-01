using HttpClientLab.Clients.GitHub;
using Polly;
using Polly.Extensions.Http;
using Refit;

var builder = WebApplication.CreateBuilder(args);

var refitSettings = new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    })
};

static IAsyncPolicy<HttpResponseMessage> Timeout() =>
    Policy.TimeoutAsync<HttpResponseMessage>(10);

static IAsyncPolicy<HttpResponseMessage> Retry() =>
    HttpPolicyExtensions.HandleTransientHttpError()
    .OrResult(r => (int)r.StatusCode == 429)
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * attempt));

builder.Services
    .AddRefitClient<IGitHubApi>(refitSettings)
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://api.github.com");
        c.DefaultRequestHeaders.UserAgent.ParseAdd("HttpClientLab/1.0");
        c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        c.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(Timeout())
    .AddPolicyHandler(Retry());

var app = builder.Build();

app.MapGet("/github/{user}", async (IGitHubApi gh, string user, CancellationToken ct) =>
{
    try
    {
        var profile = await gh.GetUserAsync(user, ct);

        return Results.Ok(profile);
    }
    catch (ApiException ex)
    {
        return Results.Problem(
            title: $"GitHub API error {(int)ex.StatusCode} {ex.StatusCode}",
            detail: ex.Content,
            statusCode: (int)ex.StatusCode
        ); 
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();
