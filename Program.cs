using System.CommandLine;
using System.Text;

namespace Wolf;

public static class Program
{
  public static int Main(string[] args)
  {
    Console.OutputEncoding = Encoding.UTF8;

    var input = new Option<string>("--inputDirectory", "-i")
    {
      Description = "Input directory of blog posts",
      HelpName = "directory",
      DefaultValueFactory = _ => "posts",
    };
    var output = new Option<string?>("--outputDirectory", "-o")
    {
      Description = "Output directory of blog posts (default: input directory)",
      HelpName = "directory",
    };
    var noIndex = new Option<bool>("--noIndex", "-nx")
    {
      Description = "Do not generate index (posts.json)",
    };
    var index = new Option<string?>("--indexDirectory", "-x")
    {
      Description = "Directory where index (posts.json) file is written (default: output directory)",
      HelpName = "directory",
    };
    var prefix = new Option<string?>("--imagePrefix", "-p")
    {
      Description = "Prefix added to image paths (default: /<output directory>/)",
      HelpName = "prefix",
    };
    var tags = new Option<bool>("--tagFile", "-t")
    {
      Description = "Generate 'tags.json' file",
    };
    var verbose = new Option<bool>("--verbose", "-v")
    {
      Description = "Show detailed output",
    };
    var noLogo = new Option<bool>("--nologo", "-n")
    {
      Description = "Do not display logo",
    };

    var root = new RootCommand("markdown to static blog post generator")
    {
      input, output, noIndex, index, prefix, tags, verbose, noLogo,
    };
    root.SetAction(parseResult =>
    {
      var inputDirectory = parseResult.GetValue(input)!;
      var outputDirectory = parseResult.GetValue(output) ?? inputDirectory;
      var config = new Config
      {
        InputDirectory = inputDirectory,
        OutputDirectory = outputDirectory,
        IndexDirectory = parseResult.GetValue(index) ?? outputDirectory,
        ImagePrefix = parseResult.GetValue(prefix) ?? "/" + outputDirectory + "/",
        GenerateIndex = !parseResult.GetValue(noIndex),
        GenerateTagsFile = parseResult.GetValue(tags),
        Verbose = parseResult.GetValue(verbose),
      };
      return Run(config, showLogo: !parseResult.GetValue(noLogo));
    });
    return root.Parse(args).Invoke();
  }

  private static int Run(Config config, bool showLogo)
  {
    if (showLogo)
    {
      Console.WriteLine();
      Console.WriteLine(@"/\_/\  wolf");
      Console.WriteLine(@"|` ´|");
      Console.WriteLine(@" \-/   markdown to static blog post generator");
      Console.WriteLine();
    }
    if (config.Verbose)
    {
      Console.WriteLine("input directory      : " + config.InputDirectory);
      Console.WriteLine("output directory     : " + config.OutputDirectory);
      Console.WriteLine("index directory      : " + config.IndexDirectory);
      Console.WriteLine("image prefix         : " + config.ImagePrefix);
      Console.WriteLine("generate 'posts.json': " + config.GenerateIndex);
      Console.WriteLine("generate 'tags.json' : " + config.GenerateTagsFile);
    }
    var result = new Converter(config).Run();
    var errors = result.Errors.Count();
    Console.WriteLine(errors == 0 ? "DONE" : $"FAILED ({errors} error(s))");
    Console.WriteLine();
    return errors == 0 ? 0 : 1;
  }
}
