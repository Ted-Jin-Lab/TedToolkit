using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Octokit;
using Serilog;
using Project = Nuke.Common.ProjectModel.Project;

namespace NukeBuilder;

[GitHubActions("bump_version", GitHubActionsImage.WindowsLatest,
    Submodules = GitHubActionsSubmodules.True,
    OnPullRequestBranches = ["main"],
    InvokedTargets = [nameof(BumpVersion)])]
[GitHubActions("test", GitHubActionsImage.WindowsLatest,
    Submodules = GitHubActionsSubmodules.True,
    OnPushBranchesIgnore = ["main"],
    InvokedTargets = [nameof(Test)])]
[GitHubActions("release", GitHubActionsImage.WindowsLatest,
    Submodules = GitHubActionsSubmodules.True,
    OnPushBranches = ["main"],
    ImportSecrets = [nameof(NuGetApiKey), nameof(GithubToken)],
    InvokedTargets = [nameof(Release)])]
class Build : NukeBuild
{
    [Parameter] readonly string GithubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
    [Parameter] readonly string NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");
    [Parameter] readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

    [GitRepository] readonly GitRepository Repository;
    [Solution(GenerateProjects = true)] readonly Solution Solution;

    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => d => d
        .Before(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetClean(s => s.SetProject(Solution));
        });

    Target Restore => d => d
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s.SetProjectFile(Solution));
        });

    Target Compile => d => d
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration.Release));
        });

    Target Test => d => d
        .DependsOn(Compile)
        .Executes(() =>
        {
            foreach (var project in Solution.AllProjects.Where(p => p.Name.EndsWith(".Tests"))) TestProject(project);

            return;

            static void TestProject(Project project)
            {
                DotNetTasks.DotNetTest(s => s
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.Release)
                    .EnableNoRestore()
                    .EnableNoBuild()
                    .SetLoggers("trx"));
            }
        });

    Target Release => d => d
        .DependsOn(PushNugetPackages, CreateGitHubRelease, UploadGitHubReleasePackages)
        .OnlyWhenStatic(HasNotRelease);

    private bool HasNotRelease()
    {
        var client = new GitHubClient(new ProductHeaderValue("NUKE-Tag"))
        {
            Credentials = new Credentials(GithubToken)
        };
        var tags = client.Repository.GetAllTags(Repository.GetGitHubOwner(), Repository.GetGitHubName()).Result
            .Select(t => t.Name);
        return !tags.Contains(GetVersionTag());
    }

    private string GetVersionTag()
    {
        var propsFile = RootDirectory / "Directory.Build.props";
        var doc = XDocument.Load(propsFile);
        var versionElement = doc.Descendants("Version").FirstOrDefault();
        return $"v{versionElement?.Value ?? throw new Exception("Version not found in Directory.Build.props")}";
    }

    public static int Main() => Execute<Build>(x => x.CopyNuGetPackages);

    #region Nuget Package

    Target CopyNuGetPackages => d => d
        .DependsOn(Test)
        .Executes(() =>
        {
            OutputDirectory.CreateOrCleanDirectory();
            var packageFiles = RootDirectory.GlobFiles("**/*.nupkg");

            foreach (var package in packageFiles)
            {
                package.Copy(OutputDirectory / package.Name, ExistsPolicy.FileOverwriteIfNewer);
                package.DeleteFile();
            }
        });

    private readonly List<string> PushedPackages = [];

    Target PushNugetPackages => d => d
        .DependsOn(CopyNuGetPackages)
        .OnlyWhenStatic(() => !string.IsNullOrEmpty(NuGetApiKey))
        .Executes(() =>
        {
            foreach (var package in OutputDirectory.GlobFiles("*.nupkg"))
            {
                var existVersions =
                    GetPackageVersions(package, out var packageName, out var packageId, out var version);
                foreach (var deletingVersion in VersionsShouldDelete(existVersions))
                    DeletePackage(packageId, deletingVersion);
                if (existVersions.Contains(version)) continue;
                if (!PushPackage(package)) continue;
                PushedPackages.Add($"- [{packageName}](https://www.nuget.org/packages/{packageId}/{version})");
            }
        });

    IEnumerable<Version> VersionsShouldDelete(Version[] versions)
    {
        var canDelete = versions.OrderDescending().Skip(10).ToArray();

        foreach (var version in canDelete.GroupBy(v => new { v.Major, v.Minor })
                     .SelectMany(v => v.OrderDescending().Skip(1)))
            yield return version;
    }

    private void DeletePackage(string packageId, Version version)
    {
        try
        {
            DotNetTasks.DotNetNuGetDelete(s => s
                .SetSource(NuGetSource)
                .SetApiKey(NuGetApiKey)
                .SetPackageId(packageId)
                .SetPackageVersion(version.ToString()));
            Log.Information($"Deleted the nuget package {packageId} {version}");
        }
        catch
        {
            Log.Warning($"Failed to delete the nuget package {packageId} {version}");
        }
    }

    private bool PushPackage(AbsolutePath package)
    {
        try
        {
            DotNetTasks.DotNetNuGetPush(s => s
                .SetTargetPath(package)
                .SetSource(NuGetSource)
                .SetApiKey(NuGetApiKey)
            );
            return true;
        }
        catch
        {
            Log.Warning($"Failed to push the nuget package {package}");
            return false;
        }
    }

    private Version[] GetPackageVersions(AbsolutePath packagePath, out string packageName, out string packageId,
        out Version version)
    {
        var packageNameVersion = packagePath.NameWithoutExtension;
        var parts = packageNameVersion.Split('.');
        if (parts.Length < 2)
        {
            packageName = packageId = string.Empty;
            version = null!;
            return [];
        }

        packageName = string.Join(".", parts.TakeWhile(p => !uint.TryParse(p, out _)));
        packageId = packageName.ToLower();
        if (!Version.TryParse(string.Join(".", parts.SkipWhile(p => !uint.TryParse(p, out _))), out version)) return [];

        var nugetCheckUrl = $"https://api.nuget.org/v3-flatcontainer/{packageId}/index.json";

        try
        {
            using var client = new HttpClient();
            var response = client.GetStringAsync(nugetCheckUrl).Result;

            using var jsonDoc = JsonDocument.Parse(response);
            return jsonDoc.RootElement.GetProperty("versions").Deserialize<Version[]>();
        }
        catch
        {
            Log.Warning($"Failed to check NuGet for {packageId} {version}. Assuming it doesn't exist.");
            return [];
        }
    }

    #endregion

    #region GitHub Release

    private Release NewRelease;

    Target CreateGitHubRelease => d => d
        .DependsOn(Test, CreateReleaseNote)
        .OnlyWhenStatic(HasNotRelease)
        .OnlyWhenStatic(() => !string.IsNullOrEmpty(GithubToken))
        .Executes(async () =>
        {
            var client = new GitHubClient(new ProductHeaderValue("NUKE-Release"))
            {
                Credentials = new Credentials(GithubToken)
            };

            var version = GetVersionTag();
            var release = new NewRelease(version)
            {
                Name = version[1..],
                Body = ReleaseNote.ToString(),
                Draft = false,
                Prerelease = false
            };

            NewRelease = await client.Repository.Release.Create(Repository.GetGitHubOwner(), Repository.GetGitHubName(),
                release);
        });

    Target UploadGitHubReleasePackages => d => d
        .DependsOn(CreateGitHubRelease, CopyNuGetPackages)
        .Executes(async () =>
        {
            var client = new GitHubClient(new ProductHeaderValue("NUKE-Release-Upload"))
            {
                Credentials = new Credentials(GithubToken)
            };

            foreach (var package in OutputDirectory.GlobFiles("*.nupkg"))
            {
                await using var packageStream = File.OpenRead(package);
                var upload = new ReleaseAssetUpload
                {
                    FileName = Path.GetFileName(package),
                    ContentType = "application/octet-stream",
                    RawData = packageStream
                };
                await client.Repository.Release.UploadAsset(NewRelease, upload);
            }
        });


    private readonly StringBuilder ReleaseNote = new();
    private readonly ContributorManager Contributors = new();

    Target CreateReleaseNote => d => d
        .DependsOn(GetCommitNote, GetLastTag, GetPullRequestNote, GetIssuesNote, PushNugetPackages)
        .Executes(async () =>
        {
            var version = GetVersionTag();
            var link = string.IsNullOrEmpty(TagName)
                ? $"https://github.com/{Repository.Identifier}/commits/{version}"
                : Repository.GetGitHubCompareTagsUrl(TagName, version);
            ReleaseNote.AppendLine($"# [{version[1..]}]({link}) ({DateTime.UtcNow.Date.ToString("yyyy-M-d dddd")})");
            var client = new GitHubClient(new ProductHeaderValue("NUKE-GetDescription"))
            {
                Credentials = new Credentials(GithubToken)
            };
            var repository = await client.Repository.Get(Repository.GetGitHubOwner(), Repository.GetGitHubName());
            ReleaseNote.AppendLine(repository.Description);
            if (PushedPackages.Count > 0)
                ReleaseNote.AppendLine($"Nuget Packages:\n{string.Join("\n", PushedPackages)}");

            ReleaseNote.Append(PullRequestNote);
            ReleaseNote.Append(IssuesNote);
            ReleaseNote.Append(CommitsNote);
            ReleaseNote.Append(Contributors.ToStringBuilder());
        });

    private readonly StringBuilder CommitsNote = new();

    Target GetCommitNote => d => d
        .DependsOn(GetGitmoji, GetCommits)
        .Executes(() =>
        {
            Dictionary<string, StringBuilder> commitsBuilders = new();

            foreach (var commit in Commits)
            {
                var message = commit.Commit.Message;

                var type = GetCommitType(message, out var content);
                if (string.IsNullOrEmpty(type)) continue;
                ref var builder = ref CollectionsMarshal.GetValueRefOrAddDefault(commitsBuilders, type, out var exist);
                if (!exist) builder = new StringBuilder();
                if (Contributors.Add(commit))
                    builder.AppendLine($"1. {content} by @{commit.Author.Login} in {commit.Sha}");
                else
                    builder.AppendLine($"1. {content} in {commit.Sha}");
            }

            if (commitsBuilders.Count == 0) return;
            CommitsNote.AppendLine("## Commits");
            foreach (var pair in commitsBuilders)
            {
                CommitsNote.AppendLine($"### {Gitmojis.FirstOrDefault(i => i.Value == pair.Key).Key} {pair.Key}");
                CommitsNote.Append(pair.Value);
            }
        });

    private static string GetCommitType(string message, out string content)
    {
        foreach (var pair in Gitmojis.Where(pair => message.StartsWith(pair.Key)))
        {
            content = message[pair.Key.Length..].Split('\n')[0].Trim();
            return pair.Value;
        }

        return content = string.Empty;
    }


    private readonly StringBuilder PullRequestNote = new();

    Target GetPullRequestNote => d => d
        .Executes(async () =>
        {
            var client = new GitHubClient(new ProductHeaderValue("NUKE-PullRequests"))
            {
                Credentials = new Credentials(GithubToken)
            };
            var pullRequests =
                await client.PullRequest.GetAllForRepository(Repository.GetGitHubOwner(), Repository.GetGitHubName(),
                    new PullRequestRequest
                    {
                        State = ItemStateFilter.Closed
                    });
            var mergedPRs = pullRequests.Where(pr => LastTagCreatedTime is null || pr.MergedAt > LastTagCreatedTime)
                .ToList();

            if (mergedPRs.Count == 0) return;
            PullRequestNote.AppendLine("## Pull Requests");

            foreach (var pr in mergedPRs)
                if (Contributors.Add(pr))
                    PullRequestNote.AppendLine($"1. {pr.Title} by @{pr.User.Login} in #{pr.Number}");
                else
                    PullRequestNote.AppendLine($"1. {pr.Title} #{pr.Number}");
        });

    private readonly StringBuilder IssuesNote = new();

    Target GetIssuesNote => d => d
        .Executes(async () =>
        {
            var client = new GitHubClient(new ProductHeaderValue("NUKE-Issues"))
            {
                Credentials = new Credentials(GithubToken)
            };

            var issues =
                await client.Issue.GetAllForRepository(Repository.GetGitHubOwner(), Repository.GetGitHubName(),
                    new RepositoryIssueRequest
                    {
                        Since = LastTagCreatedTime,
                        State = ItemStateFilter.Closed
                    });

            var closedIssues = issues.Where(i => i.PullRequest is null).ToArray();
            if (closedIssues.Length == 0) return;

            IssuesNote.AppendLine("## Issues");
            foreach (var issue in closedIssues)
            {
                var icon = issue.StateReason?.Value is ItemStateReason.Completed ? "✅" : "🚫";

                if (issue.User is { } user)
                    IssuesNote.AppendLine($"1. {icon}{issue.Title} by @{user.Login} in #{issue.Number}");
                else
                    IssuesNote.AppendLine($"1. {icon}{issue.Title} #{issue.Number}");
            }
        });

    private static readonly Dictionary<string, string> Gitmojis = [];

    Target GetGitmoji => d => d
        .Executes(async () =>
        {
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync("https://gitmoji.dev/api/gitmojis");
            if (JsonConvert.DeserializeObject(json) is not JObject jObject) return;
            if (jObject["gitmojis"] is not JArray emojis) return;
            foreach (var emoji in emojis)
            {
                var description = emoji["description"]?.ToString();

                if (description is null) continue;
                if (emoji["emoji"]?.ToString() is { } emojiKey)
                    Gitmojis[emojiKey] = description;
                if (emoji["code"]?.ToString() is { } codeKey)
                    Gitmojis[codeKey] = description;
            }
        });

    private IReadOnlyList<GitHubCommit> Commits = [];

    Target GetCommits => d => d
        .DependsOn(GetLastTag)
        .OnlyWhenStatic(() => !string.IsNullOrEmpty(GithubToken))
        .Executes(async () =>
        {
            var client = new GitHubClient(new ProductHeaderValue("NUKE-Commits"))
            {
                Credentials = new Credentials(GithubToken)
            };

            Commits = await client.Repository.Commit.GetAll(Repository.GetGitHubOwner(), Repository.GetGitHubName(),
                new CommitRequest
                {
                    Since = LastTagCreatedTime
                });
        });

    private DateTimeOffset? LastTagCreatedTime;
    private string TagName;

    Target GetLastTag => d => d
        .OnlyWhenStatic(() => !string.IsNullOrEmpty(GithubToken))
        .Executes(async () =>
        {
            var client = new GitHubClient(new ProductHeaderValue("NUKE-LastTag"))
            {
                Credentials = new Credentials(GithubToken)
            };

            try
            {
                var release =
                    await client.Repository.Release.GetLatest(Repository.GetGitHubOwner(), Repository.GetGitHubName());
                TagName = release.TagName;
                LastTagCreatedTime = release.CreatedAt;
            }
            catch
            {
                //Ignore
            }
        });

    #endregion

    #region Bump Version

    Target BumpVersion => d => d
        .Executes(() =>
        {
            GitTasks.Git($"fetch origin");
            GitTasks.Git($"checkout -B development origin/development");
            
            if (GitTasks.Git("log -1 --pretty=%B").First().Text.Trim().StartsWith("🔖")) return;

            var propsFile = RootDirectory / "Directory.Build.props";

            if (ChangeVersionTag(propsFile, v =>
                {
                    var today = DateTime.Today;
                    if (v.Major == today.Year && v.Minor == today.Month && v.Build == today.Day)
                        return new Version(today.Year, today.Month, today.Day, Math.Max(0, v.Revision) + 1);

                    return new Version(today.Year, today.Month, today.Day, 0);
                }) is not { } version) return;

            GitTasks.Git($"config user.name \"nuke-bot\"");
            GitTasks.Git($"config user.email \"nuke-bot@users.noreply.github.com\"");
            
            GitTasks.Git($"add Directory.Build.props");
            GitTasks.Git($"commit -m \"🔖 {version} Released!\"");
            GitTasks.Git($"push origin development");
        });

    private static Version ChangeVersionTag(AbsolutePath propsFile, Func<Version, Version> changer)
    {
        var doc = XDocument.Load(propsFile);
        var versionElement = doc.Descendants("Version").FirstOrDefault();
        if (versionElement is null) return null;
        if (!Version.TryParse(versionElement.Value, out var version)) return null;
        var newVersion = changer(version);
        versionElement.Value = newVersion.ToString();
        doc.Save(propsFile);
        return newVersion;
    }

    #endregion
}