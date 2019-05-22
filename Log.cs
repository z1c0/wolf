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
      var color = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write("ERROR: ");
      Console.WriteLine(msg);
      Console.ForegroundColor = color;
    }

    internal void Warning(string msg)
    {
      _messages.Add(Tuple.Create(LogType.Warning, msg));
      var color = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Write("WARNING: ");
      Console.WriteLine(msg);
      Console.ForegroundColor = color;
    }

    internal void Info(string msg)
    {
      _messages.Add(Tuple.Create(LogType.Info, msg));
      Console.WriteLine(msg);
    }
  }
}