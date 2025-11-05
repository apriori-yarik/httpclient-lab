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

builder.Services.AddGitHubClient(refitSettings);

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
