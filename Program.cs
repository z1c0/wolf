using System;

namespace Wolf
{
  public class Program
  {
    public static void Main(string[] args)
    {      
      Console.WriteLine();
      Console.WriteLine(@" ^_^  wolf");
      Console.WriteLine(@"(` ´)");
      Console.WriteLine(@" \°/  markdown to static blog post generator");
      Console.WriteLine();

      var config = new Config
      {
        InputDirectory = "posts",
        OutputDirectory = "posts",
        IndexDirectory = "posts",
        ImagePrefix = "/posts/",
        ForceGeneration = true,
        GenerateTagsFile = false
      };
      var converter = new Converter(config);
      converter.Run();

      Console.WriteLine("DONE");
      Console.WriteLine();
    }
  }
}
