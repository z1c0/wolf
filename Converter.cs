using System;
using System.IO;
using System.Linq;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Wolf
{
  public class Converter
  {
    private Config _config;
    private Index _index;
    private Log _log;

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
        try
        {
          var dirs = Directory.GetDirectories(_config.InputDirectory).Select(d => new DirectoryInfo(d));
          foreach (DirectoryInfo d in dirs)
          {
            var mdFile = GetMarkdownFileName(d);
            if (mdFile == null)
            {
              _log.Warning($"No Markdown (.md) document found in directory '{d.FullName}'");
            }
            else
            {
              Convert(mdFile);
            }
          }
          if (_config.GenerateIndex)
          {
            _index.Save();
          }
        }
        catch (Exception e)
        {
          _log.Error(e.ToString());
        }
      }
      return new Result(
        _log.GetMessagesOfType(LogType.Error),
        _log.GetMessagesOfType(LogType.Warning),
        _log.GetMessagesOfType(LogType.Info)
      );
    }

    private static FileInfo GetMarkdownFileName(DirectoryInfo dir)
    {
      var names = new[] { dir.Name, "index", "default", "post" };
      foreach (var n in names)
      {
        var fi = new FileInfo(Path.ChangeExtension(Path.Combine(dir.FullName, n), "md"));
        if (fi.Exists)
        {
          return fi;
        }
      }
      return null;
    }

    private void Convert(FileInfo mdFile)
    {
      var name =  mdFile.Directory.Name;
      var htmlDir = Path.Combine(_config.OutputDirectory, name);
      if (!Directory.Exists(htmlDir))
      {
        Directory.CreateDirectory(htmlDir);
      }
      using (var reader = mdFile.OpenText())
      {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter().Build();
        var doc = Markdown.Parse(reader.ReadToEnd(), pipeline);
        // Actual HTML conversion now?
        var htmlFile = Path.ChangeExtension(Path.Combine(htmlDir, name), "html");
        var featuredImage = string.Empty;
        _log.Info($"Processing '{htmlFile}'");
        foreach (var l in doc.Descendants().OfType<LinkInline>().Where(l => l.IsImage))
        {
          var img = new FileInfo(Path.Combine(mdFile.DirectoryName, l.Url));
          if (img.Exists)
          {
            if (_config.InputDirectory != _config.OutputDirectory)
            {
              img.CopyTo(Path.Combine(htmlDir, l.Url), true);
            }
            l.Url = _config.ImagePrefix + name + '/' + l.Url;
            if (string.IsNullOrEmpty(featuredImage))
            {
              featuredImage = l.Url;
            }
          }
        }
        using (var writer = new StreamWriter(new FileStream(htmlFile, FileMode.Create)))
        {
            var renderer = new HtmlRenderer(writer);
            pipeline.Setup(renderer);
            renderer.Render(doc);
            writer.Flush();
        }
        var yaml = doc.Descendants().OfType<YamlFrontMatterBlock>().FirstOrDefault();
        _index.Add(name, featuredImage, yaml?.Lines.Lines.Select(l => l.ToString()));
      }
    }
  }
}