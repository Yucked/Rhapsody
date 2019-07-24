using System.Text.Json.Serialization;

namespace Frostbyte.Entities.Results
{
    public sealed class GitHubResult
    {
        public GitHubCommit Commit { get; set; }
        public GitHubRepo Repo { get; set; }
    }

    public sealed class GitHubCommit
    {
        [JsonPropertyName("sha")]
        public string RawSha { get; set; }

        [JsonIgnore]
        public string Sha
            => RawSha.Sub(0, 34);
    }

    public sealed class GitHubRepo
    {
        [JsonPropertyName("open_issues")]
        public int OpenIssues { get; set; }

        [JsonPropertyName("license")]
        public RepoLicense License { get; set; }
    }

    public sealed class RepoLicense
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}