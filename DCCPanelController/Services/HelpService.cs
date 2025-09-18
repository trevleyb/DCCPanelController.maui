using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Markdig;

namespace DCCPanelController.Services;

// App/Services/HelpService.cs

public sealed class HelpService {
    // You can tweak these:
    public const string DefaultTopicId      = "getting-started";
    public const string UndefinedTopicTitle = "Help Topic Not Found";
    
    private static readonly Lazy<HelpService> _current            = new(() => new HelpService());
    public static readonly string PackedRoot    = "help/en"; // localize later
    public static readonly string InstalledRoot = Path.Combine(FileSystem.AppDataDirectory, PackedRoot);

    private readonly MarkdownPipeline _md = new MarkdownPipelineBuilder()
                                           .UseAdvancedExtensions()
                                           .UseAutoIdentifiers()
                                           .Build();

    private Dictionary<string, HelpTopicMeta> _index = new();
    public static HelpService Current => _current.Value;

    public async Task InitializeAsync(bool force = false) {
        #if DEBUG
        force = true;
        #endif

        await EnsureInstalledAsync(force);
        var indexPath = Path.Combine(InstalledRoot, "index.json");
        _index = JsonSerializer.Deserialize<Dictionary<string, HelpTopicMeta>>(
                     await File.ReadAllTextAsync(indexPath))
              ?? new Dictionary<string, HelpTopicMeta>();
    }

    public IEnumerable<HelpTopicMeta> GetAllTopics() => _index.Values.OrderBy(t => t.Title);

    public async Task<HelpDocument> LoadTopicAsync(string? id, string? referrerId = null, string? anchor = null) {
        id = string.IsNullOrWhiteSpace(id) ? DefaultTopicId : id;

        // Known topic in index?
        if (_index.TryGetValue(id, out var meta)) {
            var mdPath = Path.Combine(InstalledRoot, $"{id}.md");
            if (File.Exists(mdPath)) {
                var md = await File.ReadAllTextAsync(mdPath);
                var htmlBody = Markdown.ToHtml(md, _md);

                var baseUrl = new Uri(InstalledRoot + Path.DirectorySeparatorChar).AbsoluteUri;
                htmlBody = RewriteRelativeImageSrcToAbsolute(htmlBody, InstalledRoot);
                if (!string.IsNullOrEmpty(anchor)) {
                    htmlBody += $"<script>location.hash='#{Uri.EscapeDataString(anchor)}';</script>";
                }

                var imgPath = Path.Combine(InstalledRoot, "images", "turnouts.png");
                var baseDir = InstalledRoot.EndsWith(Path.DirectorySeparatorChar) ? InstalledRoot : InstalledRoot + Path.DirectorySeparatorChar;
                var baseFileUri = new Uri(baseDir).AbsoluteUri;
                var fullHtml = WrapHtml(meta.Title, htmlBody, baseFileUri);

                string? filePath = null;
                #if MACCATALYST
                var renderedDir = Path.Combine(InstalledRoot, "_rendered");
                Directory.CreateDirectory(renderedDir);
                filePath = Path.Combine(renderedDir, $"{id}.html");
                await File.WriteAllTextAsync(filePath, fullHtml);
                #endif
                return new HelpDocument(meta.Title, fullHtml, baseFileUri, filePath);
            }
        }

        // Fallback undefined page
        var baseDirFallback = InstalledRoot.EndsWith(Path.DirectorySeparatorChar)
            ? InstalledRoot
            : InstalledRoot + Path.DirectorySeparatorChar;
        var baseFileUriFallback = new Uri(baseDirFallback).AbsoluteUri;
        return BuildUndefinedDocument(id!, referrerId, baseFileUriFallback);
    }

    public Task NavigateAsync(string id) => Shell.Current.GoToAsync($"help?topicId={Uri.EscapeDataString(id)}");

    private string WrapHtml(string title, string body, string baseHrefFileUri) => $@"
<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<title>{WebUtility.HtmlEncode(title)}</title>
<base href=""{baseHrefFileUri}"">  <!-- ensures ./images/... resolves -->
<style>
  :root {{
    --fg:#111; --sub:#666; --accent:#0a84ff; --bg:#fff; --chip:#f3f4f6;
  }}
  @media (prefers-color-scheme: dark) {{
    :root {{ --fg:#e8e8e8; --sub:#9aa0a6; --accent:#4da3ff; --bg:#121212; --chip:#1f2937; }}
  }}
  html,body {{ background:var(--bg); color:var(--fg); font-family:-apple-system, Segoe UI, Roboto, Helvetica, Arial, sans-serif; }}
  body {{ padding:12px; line-height:1.5; }}
  a {{ color:var(--accent); text-decoration:none; }}
  code, pre {{ font-family: ui-monospace, Menlo, Consolas, monospace; }}
  img {{ max-width:100%; height:auto; border-radius:6px; }}
  h1,h2,h3 {{ scroll-margin-top:80px; }}
  .chips a {{ display:inline-block; margin:.25rem .4rem .25rem 0; padding:.25rem .6rem; background:var(--chip); border-radius:999px; font-size:.9em; }}
</style>
</head>
<body>{body}</body>
</html>";

    private HelpDocument BuildUndefinedDocument(string badId, string? referrerId, string baseFileUri) {
        // Suggest closest matches (simple ranking)
        var suggestions = _index.Values
                                .OrderByDescending(t => Score(t, badId))
                                .Take(6)
                                .ToList();

        var list = suggestions.Count == 0
            ? "<p>No similar topics found.</p>"
            : string.Join("", suggestions.Select(t =>
                $@"<li><a href=""help://topic/{Uri.EscapeDataString(t.Id)}"">
                 {WebUtility.HtmlEncode(t.Title)}
               </a></li>"));

        var chips = string.Join("", _index.Values
                                          .OrderBy(t => t.Title)
                                          .Take(10)
                                          .Select(t => $@"<a href=""help://topic/{Uri.EscapeDataString(t.Id)}"">
                         {WebUtility.HtmlEncode(t.Title)}
                       </a>"));

        var refHtml = string.IsNullOrWhiteSpace(referrerId)
            ? ""
            : $@"<p class=""sub"">Linked from:
             <a href=""help://topic/{Uri.EscapeDataString(referrerId)}"">
               {WebUtility.HtmlEncode(referrerId)}
             </a>
           </p>";

        var body = $@"
<h1>{UndefinedTopicTitle}</h1>
<p>The requested topic <code>{WebUtility.HtmlEncode(badId)}</code> was not found.</p>
{refHtml}
<p><a href=""help://topic/{Uri.EscapeDataString(DefaultTopicId)}"">Go to Help home</a></p>

<h3>Try one of these:</h3>
<ul>{list}</ul>

<div class=""chips"">{chips}</div>";

        // NEW: wrap with base href (3-arg WrapHtml)
        var fullHtml = WrapHtml(UndefinedTopicTitle, body, baseFileUri);

        // NEW: on Mac Catalyst, write a real file and return its path
        string? filePath = null;
        #if MACCATALYST
        var renderedDir = Path.Combine(InstalledRoot, "_rendered");
        Directory.CreateDirectory(renderedDir);
        filePath = Path.Combine(renderedDir, $"__undefined_{SanitizeFileName(badId)}.html");
        File.WriteAllText(filePath, fullHtml);
        #endif

        return new HelpDocument(UndefinedTopicTitle, fullHtml, baseFileUri, filePath);

        static int Score(HelpTopicMeta t, string needle) {
            var title = t.Title ?? "";
            var id = t.Id ?? "";
            var k = t.Keywords is null ? "" : string.Join(' ', t.Keywords);

            var s = 0;
            if (id.Equals(needle, StringComparison.OrdinalIgnoreCase)) s += 100;
            if (title.Equals(needle, StringComparison.OrdinalIgnoreCase)) s += 90;
            if (id.StartsWith(needle, StringComparison.OrdinalIgnoreCase)) s += 50;
            if (title.StartsWith(needle, StringComparison.OrdinalIgnoreCase)) s += 45;
            if (id.Contains(needle, StringComparison.OrdinalIgnoreCase)) s += 30;
            if (title.Contains(needle, StringComparison.OrdinalIgnoreCase)) s += 25;
            if (!string.IsNullOrEmpty(k) && k.Contains(needle, StringComparison.OrdinalIgnoreCase)) s += 10;
            return s;
        }
        #if MACCATALYST
        static string SanitizeFileName(string value) {
            foreach (var c in Path.GetInvalidFileNameChars()) {
                value = value.Replace(c, '_');
            }
            return value.Length == 0 ? "topic" : value;
        }
        #endif
    }

    // ------- existing install/update helpers (from your latest version) -------
    // ------------ Install / Update logic ------------

    private async Task EnsureInstalledAsync(bool force) {
        var pkgManifest = await ReadPackageManifestAsync();

        if (force || !Directory.Exists(InstalledRoot)) {
            await ReinstallFromPackageAsync(pkgManifest);
            return;
        }

        var installedManifestPath = Path.Combine(InstalledRoot, "manifest.json");
        if (!File.Exists(installedManifestPath)) {
            // No installed manifest: treat as stale
            await ReinstallFromPackageAsync(pkgManifest);
            return;
        }

        var installedManifest = JsonSerializer.Deserialize<Manifest>(
            await File.ReadAllTextAsync(installedManifestPath));

        // Refresh if version changed or any file missing
        var missingAny = pkgManifest.Files.Any(rel => !File.Exists(Path.Combine(InstalledRoot, rel)));
        if (installedManifest is null
         || !string.Equals(installedManifest.Version, pkgManifest.Version, StringComparison.Ordinal)
         || missingAny) {
            await ReinstallFromPackageAsync(pkgManifest);
        }
    }

    private async Task<Manifest> ReadPackageManifestAsync() {
        using var s = await FileSystem.OpenAppPackageFileAsync($"{PackedRoot}/manifest.json");
        using var r = new StreamReader(s);
        var json = await r.ReadToEndAsync();
        var m = JsonSerializer.Deserialize<Manifest>(json)
             ?? throw new InvalidOperationException("Invalid help manifest.");
        return m;
    }

    private async Task ReinstallFromPackageAsync(Manifest manifest) {
        if (Directory.Exists(InstalledRoot)) {
            Directory.Delete(InstalledRoot, true);
        }
        Directory.CreateDirectory(InstalledRoot);

        // Copy all listed files
        foreach (var rel in manifest.Files) {
            var destPath = Path.Combine(InstalledRoot, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            await CopyRawAsync($"{PackedRoot}/{rel}", destPath);
        }

        // Write manifest we installed
        var installedManifestPath = Path.Combine(InstalledRoot, "manifest.json");
        await File.WriteAllTextAsync(installedManifestPath, JsonSerializer.Serialize(manifest));
    }

    private async Task CopyRawAsync(string rawPath, string dest) {
        using var s = await FileSystem.OpenAppPackageFileAsync(rawPath);
        using var fs = File.Create(dest);
        await s.CopyToAsync(fs);
    }

    private static string RewriteRelativeImageSrcToAbsolute(string html, string installedRoot) {
        // Ensure trailing slash and file:// URL
        var baseUri = new Uri(installedRoot.EndsWith(Path.DirectorySeparatorChar)
            ? installedRoot
            : installedRoot + Path.DirectorySeparatorChar);
        var baseUrl = baseUri.AbsoluteUri; // e.g., file:///var/mobile/Containers/.../help/en/

        // Replace src="images/foo.png" or src="./images/foo.png" (but leave http/https/help/file alone)
        string Rewriter(Match m) {
            var path = m.Groups["path"].Value;
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("help:", StringComparison.OrdinalIgnoreCase)) {
                return m.Value;
            }

            // Normalize ./ and join to base
            var rel = path.StartsWith("./") ? path[2..] : path;
            var abs = new Uri(new Uri(baseUrl), rel).AbsoluteUri;
            return$"src=\"{abs}\"";
        }

        var rx = new Regex(
            "src\\s*=\\s*\"(?<path>[^\"]+)\"",
            RegexOptions.IgnoreCase);
        return rx.Replace(html, Rewriter);
    }
}

public record HelpTopicMeta(string Id, string Title, string[]? Keywords);

public record HelpDocument(string Title, string Html, string BaseUrl, string? FilePath = null);

public record Manifest(string Version, string[] Files);