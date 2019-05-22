using System;

namespace Wolf
{
  internal class Post : IComparable
  {
    public string Slug  { get; set; }
    public string Title  { get; set; }
    public DateTime Published { get; set; }
    public string[] Tags { get; set; }
    public string FeaturedImage { get; set; }

    public override string ToString()
    {
      return Title;
    }

    public int CompareTo(object obj)
    {
      return ((Post)obj).Published.CompareTo(Published);
    }    
  }
}