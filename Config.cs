namespace Wolf
{
  public class Config
  {
    public bool GenerateTagsFile { get; set; }
    public string InputDirectory { get; set; }
    public string OutputDirectory { get; set; }
    public string IndexDirectory { get; set; }
    public string ImagePrefix { get; set; }
    public bool Verbose { get; set; }
    public bool ThrowOnError { get; set; }
  }
}