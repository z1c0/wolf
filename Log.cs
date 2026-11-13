namespace Wolf;

internal enum LogType
{
  Info,
  Warning,
  Error
}

internal class Log(Config config)
{
  private readonly List<(LogType Type, string Message)> _messages = [];

  internal int ErrorCount => _messages.Count(m => m.Type == LogType.Error);

  internal IEnumerable<string> GetMessagesOfType(LogType type) =>
    _messages.Where(m => m.Type == type).Select(m => m.Message).ToList();

  internal void Error(string msg)
  {
    if (config.ThrowOnError)
    {
      throw new InvalidOperationException(msg);
    }
    _messages.Add((LogType.Error, msg));
    PrintToConsole("ERROR: ", msg, ConsoleColor.Red);
  }

  internal void Warning(string msg)
  {
    _messages.Add((LogType.Warning, msg));
    PrintToConsole("WARNING: ", msg, ConsoleColor.Yellow);
  }

  internal void Info(string msg)
  {
    _messages.Add((LogType.Info, msg));
    PrintToConsole(null, msg, ConsoleColor.Cyan);
  }

  private static void PrintToConsole(string? prefix, string msg, ConsoleColor color)
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
