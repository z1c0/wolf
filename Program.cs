using System;
using System.Text;
using Microsoft.Extensions.CommandLineUtils;

namespace Wolf
{
  public class Program
  {
    public static void Main(string[] args)
    {
      Console.OutputEncoding = Encoding.UTF8;
      var config = new Config();
      var showLogo = true;
      var app = new CommandLineApplication(false)
      {
        Name = "wolf",
        Description = "markdown to static blog post generator"
      };
      //app.Command("catapult", cfg => {});
      var input = app.Option(
        "-i | --inputDirectory <directory>",
        "Input directory of blog posts",
        CommandOptionType.SingleValue);
      var output = app.Option(
        "-o | --outputDirectory <directory>",
        "Output directory of blog posts",
        CommandOptionType.SingleValue);
      var index = app.Option(
        "-x | --indexDirectory <directory>",
        "Directory where index (posts.json) file is written",
        CommandOptionType.SingleValue);
      var prefix = app.Option(
        "-p | --imagePrefix <prefix>",
        "Prefix added to image paths.",
        CommandOptionType.SingleValue);
      var tags = app.Option(
        "-t | --tagFile",
        "Generate 'tags.json' file.",
        CommandOptionType.NoValue);
      var verbose = app.Option(
        "-v | --verbose",
        "Show detailed output.",
        CommandOptionType.NoValue);
      var logo = app.Option(
        "-n | --nologo",
        "Do not display logo.",
        CommandOptionType.NoValue);
      app.HelpOption("-? | -h | --help");
      app.OnExecute(() =>
      {
        config.InputDirectory = input.HasValue() ? input.Value() : "posts";
        config.OutputDirectory = output.HasValue() ? output.Value() : config.InputDirectory;
        config.IndexDirectory = index.HasValue() ? index.Value() : config.OutputDirectory;
        config.ImagePrefix = prefix.HasValue() ? prefix.Value() : "/" + config.OutputDirectory + "/";
        config.GenerateTagsFile = tags.HasValue();
        config.Verbose = verbose.HasValue();
        showLogo = !logo.HasValue();
        return 1;
      });
      if (app.Execute(args) == 1)
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
          Console.WriteLine("input directory     : " + config.InputDirectory);
          Console.WriteLine("output directory    : " + config.OutputDirectory);
          Console.WriteLine("index directory     : " + config.IndexDirectory);
          Console.WriteLine("image prefix        : " + config.ImagePrefix);
          Console.WriteLine("generate 'tags.json': " + config.GenerateTagsFile);
        }
        var converter = new Converter(config);
        converter.Run();
        Console.WriteLine("DONE");
        Console.WriteLine();
      }
    }
  }
}
