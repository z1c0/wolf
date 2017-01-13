using System;

namespace Wolf
{
  public class Program
  {
    public static void Main(string[] args)
    {
      Console.WriteLine(@"                _  __ ");
      Console.WriteLine(@"               | |/ _|");
      Console.WriteLine(@" __      _____ | | |_ ");
      Console.WriteLine(@" \ \ /\ / / _ \| |  _|");
      Console.WriteLine(@"  \ V  V / (_) | | |  ");
      Console.WriteLine(@"   \_/\_/ \___/|_|_|  ");
      Console.WriteLine(" a markdown to static blog post generator");
      Console.WriteLine();

      var config = new Config
      {
        InputDirectory = "posts",
        OutputDirectory = "wwwroot",
        IndexDirectory = "wwwroot",
        ImagePrefix = "/posts/",
        ForceGeneration = true
      };
      var converter = new Converter(config);
      converter.Run();

      Console.WriteLine("DONE");
      Console.WriteLine();
    }
  }
}
