namespace Wolf
{
  internal class Config
  {
    internal bool GenerateTagsFile { get; set; }
    internal string InputDirectory { get; set; }
    internal string OutputDirectory { get; set; }
    internal string IndexDirectory { get; set; }
    internal string ImagePrefix { get; set; }
    internal bool Verbose { get; set; }
  }
}