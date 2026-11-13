namespace Wolf;

public class Result(IEnumerable<string> errors, IEnumerable<string> warnings, IEnumerable<string> infos)
{
  public IEnumerable<string> Errors { get; } = errors;
  public IEnumerable<string> Warnings { get; } = warnings;
  public IEnumerable<string> Information { get; } = infos;
}
