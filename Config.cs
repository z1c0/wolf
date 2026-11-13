namespace Wolf;

public class Config
{
  public bool GenerateIndex { get; set; } = true;
  public bool GenerateTagsFile { get; set; }
  public required string InputDirectory { get; set; }
  public required string OutputDirectory { get; set; }
  public required string IndexDirectory { get; set; }
  public required string ImagePrefix { get; set; }
  public bool Verbose { get; set; }
  public bool ThrowOnError { get; set; }
}
