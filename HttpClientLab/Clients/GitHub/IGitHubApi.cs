using Refit;

namespace HttpClientLab.Clients.GitHub
{
    public interface IGitHubApi
    {
        [Get("/users/{user}")]
        Task<GitHubUser> GetUserAsync(string user, CancellationToken ct = default);
    }

    public sealed record GitHubUser(
        string login,
        long id,
        string? name,
        int public_repos,
        int followers,
        int following);
}
