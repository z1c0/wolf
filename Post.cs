using System;

namespace Wolf
{
  internal class Post
  {
    public string Slug  { get; set; }
    public string Title  { get; set; }
    public DateTime Published { get; set; }
    public string[] Tags { get; set; }

    public override string ToString()
    {
      return Title;
    }
  }
}