using System;

namespace Wolf
{
  internal static class Log
  {
    internal static void Error(string msg)
    {
      var color = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write("ERROR: ");
      Console.WriteLine(msg);
      Console.ForegroundColor = color;
    }

    internal static void Warning(string msg)
    {
      var color = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Write("WARNING: ");
      Console.WriteLine(msg);
      Console.ForegroundColor = color;
    }

    internal static void Info(string msg)
    {
      Console.WriteLine(msg);
    }
  }
}