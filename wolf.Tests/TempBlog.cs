using Newtonsoft.Json.Linq;

namespace Wolf.Tests;

// A throwaway input/output folder pair for one test.
internal sealed class TempBlog : IDisposable
{
  public TempBlog()
  {
    Root = Path.Combine(Path.GetTempPath(), "wolf-tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(Input);
  }

  public string Root { get; }
  public string Input => Path.Combine(Root, "in");
  public string Output => Path.Combine(Root, "out");

  public TempBlog Post(string slug, string markdown, string fileName = "post.md")
  {
    var dir = Path.Combine(Input, slug);
    Directory.CreateDirectory(dir);
    System.IO.File.WriteAllText(Path.Combine(dir, fileName), markdown);
    return this;
  }

  public TempBlog File(string relativePath, string content = "x")
  {
    var path = Path.Combine(Input, relativePath);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    System.IO.File.WriteAllText(path, content);
    return this;
  }

  public Config Config(bool separateOutput = false) => new()
  {
    InputDirectory = Input,
    OutputDirectory = separateOutput ? Output : Input,
    IndexDirectory = separateOutput ? Output : Input,
    ImagePrefix = "/posts/",
  };

  public Result Run(Config? config = null) => new Converter(config ?? Config()).Run();

  public string Html(string slug, bool separateOutput = false) =>
    System.IO.File.ReadAllText(Path.Combine(separateOutput ? Output : Input, slug, slug + ".html"));

  public JArray Index(bool separateOutput = false) =>
    JArray.Parse(System.IO.File.ReadAllText(Path.Combine(separateOutput ? Output : Input, "posts.json")));

  public static string FrontMatter(params string[] lines) =>
    "---\n" + string.Join("\n", lines) + "\n---\n";

  public void Dispose()
  {
    try
    {
      Directory.Delete(Root, recursive: true);
    }
    catch (IOException)
    {
    }
  }
}
