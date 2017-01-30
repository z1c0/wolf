using System.Collections.Generic;

namespace Wolf
{
  public class Result
  {
    internal Result(IEnumerable<string> errors, IEnumerable<string> warnings, IEnumerable<string> infos)
    {
      Errors = errors;
      Warnings = warnings;
      Information = infos;
    }

    public IEnumerable<string> Errors { get; private set; }
    public IEnumerable<string> Warnings { get; private set; }
    public IEnumerable<string> Information { get; private set; }
  }
}