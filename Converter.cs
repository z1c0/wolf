using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Wolf;

public class Converter
{
  private static readonly string[] MarkdownNames = ["index", "default", "post"];

  private readonly Config _config;
  private readonly Index _index;
  private readonly Log _log;
  private readonly MarkdownPipeline _pipeline =
    new MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter().Build();

  public Converter(Config config)
  {
    _config = config;
    _log = new Log(_config);
    _index = new Index(_log, _config);
  }

  public Result Run()
  {
    if (!Directory.Exists(_config.InputDirectory))
    {
      _log.Error($"Input directory '{_config.InputDirectory}' does not exist");
    }
    else
    {
      var dirs = new DirectoryInfo(_config.InputDirectory)
        .GetDirectories()
        .OrderBy(d => d.Name, StringComparer.Ordinal);
      foreach (var d in dirs)
      {
        var mdFile = GetMarkdownFile(d);
        if (mdFile == null)
        {
          _log.Warning($"No Markdown (.md) document found in directory '{d.FullName}'");
          continue;
        }
        // One broken post must not stop the others from being converted.
        try
        {
          Convert(mdFile);
        }
        catch (Exception e)
        {
          _log.Error($"Converting '{mdFile.FullName}' failed: {e}");
        }
      }
      if (_config.GenerateIndex)
      {
        // A posts.json written after an error would silently drop the
        // failed post from the site, so keep the previous one instead.
        if (_log.ErrorCount > 0)
        {
          _log.Error("Index (posts.json) not written because of the errors above");
        }
        else
        {
          try
          {
            _index.Save();
          }
          catch (Exception e)
          {
            _log.Error($"Writing the index failed: {e}");
          }
        }
      }
    }
    return new Result(
      _log.GetMessagesOfType(LogType.Error),
      _log.GetMessagesOfType(LogType.Warning),
      _log.GetMessagesOfType(LogType.Info)
    );
  }

  private static FileInfo? GetMarkdownFile(DirectoryInfo dir) =>
    MarkdownNames.Prepend(dir.Name)
      .Select(n => new FileInfo(Path.Combine(dir.FullName, n + ".md")))
      .FirstOrDefault(fi => fi.Exists);

  private bool CopiesFiles => !string.Equals(
    Path.GetFullPath(_config.InputDirectory),
    Path.GetFullPath(_config.OutputDirectory),
    StringComparison.Ordinal);

  private void Convert(FileInfo mdFile)
  {
    var name = mdFile.Directory!.Name;
    var htmlDir = Path.Combine(_config.OutputDirectory, name);
    Directory.CreateDirectory(htmlDir);

    var doc = Markdown.Parse(File.ReadAllText(mdFile.FullName), _pipeline);
    var htmlFile = Path.Combine(htmlDir, name + ".html");
    var featuredImage = string.Empty;
    _log.Info($"Processing '{htmlFile}'");
    foreach (var l in doc.Descendants().OfType<LinkInline>().Where(l => l.IsImage))
    {
      if (!TryResolveLocalFile(mdFile.DirectoryName!, l.Url, out var img))
      {
        continue;
      }
      CopyToOutput(img, mdFile.DirectoryName!, htmlDir);
      l.Url = _config.ImagePrefix + name + '/' + l.Url;
      if (string.IsNullOrEmpty(featuredImage))
      {
        featuredImage = l.Url;
      }
    }
    using (var writer = new StreamWriter(htmlFile))
    {
      var renderer = new HtmlRenderer(writer);
      _pipeline.Setup(renderer);
      renderer.Render(doc);
    }
    var yaml = doc.Descendants().OfType<YamlFrontMatterBlock>().FirstOrDefault();
    var post = _index.Add(name, featuredImage, yaml?.Lines.Lines.Select(l => l.ToString()));

    // An "image:" from the front matter that the post body doesn't show
    // still has to end up next to the HTML.
    if (post.FeaturedImageFile is { } featured &&
        TryResolveLocalFile(mdFile.DirectoryName!, featured, out var featuredFile))
    {
      CopyToOutput(featuredFile, mdFile.DirectoryName!, htmlDir);
    }
  }

  // Only relative paths to files that exist next to the post count as local
  // images; absolute URLs ("https://…", "/img/…") are left alone.
  private static bool TryResolveLocalFile(string postDir, string? url, out FileInfo file)
  {
    file = null!;
    if (string.IsNullOrWhiteSpace(url) ||
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && !uri.IsFile ||
        url.StartsWith('/') || url.StartsWith('\\'))
    {
      return false;
    }
    var candidate = new FileInfo(Path.Combine(postDir, url));
    if (!candidate.Exists)
    {
      // Markdown image paths may be URL-encoded ("my%20pic.png").
      candidate = new FileInfo(Path.Combine(postDir, Uri.UnescapeDataString(url)));
    }
    file = candidate;
    return candidate.Exists;
  }

  private void CopyToOutput(FileInfo file, string postDir, string htmlDir)
  {
    if (!CopiesFiles)
    {
      return;
    }
    // Keep the image's path relative to the post, subfolders included.
    var target = Path.Combine(htmlDir, Path.GetRelativePath(postDir, file.FullName));
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    file.CopyTo(target, true);
  }
}
