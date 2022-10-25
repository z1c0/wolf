using System;
using System.Linq;
using System.Collections.Generic;

namespace Wolf
{
  internal enum LogType
  {
    Info,
    Warning,
    Error
  }

  internal class Log
  {
    private Config _config;
    private List<Tuple<LogType, string>> _messages;

    internal Log(Config config)
    {
      _config = config;
      _messages = new List<Tuple<LogType, string>>();
    }

    internal IEnumerable<string> GetMessagesOfType(LogType type)
    {
      return _messages.Where(m => m.Item1 == type).Select(m => m.Item2).ToList();
    }

    internal void Error(string msg)
    {
      if (_config.ThrowOnError)
      {
        throw new InvalidOperationException(msg);
      }
      _messages.Add(Tuple.Create(LogType.Error, msg));
      PrintToConsole("ERROR: ", msg, ConsoleColor.Red);
    }

    internal void Warning(string msg)
    {
      _messages.Add(Tuple.Create(LogType.Warning, msg));
      PrintToConsole("WARNING: ", msg, ConsoleColor.Yellow);
    }

    internal void Info(string msg)
    {
      _messages.Add(Tuple.Create(LogType.Info, msg));
      PrintToConsole(null, msg, ConsoleColor.Cyan);
    }

    private static void PrintToConsole(string prefix, string msg, ConsoleColor color)
    {
      var saveColor = Console.ForegroundColor;
      Console.ForegroundColor = color;
      if (!string.IsNullOrEmpty(prefix))
      {
        Console.Write(prefix);
      }
      Console.WriteLine(msg);
      Console.ForegroundColor = saveColor;
    }
  }
}