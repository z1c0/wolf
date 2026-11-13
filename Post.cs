namespace Wolf;

internal class Post : IComparable<Post>
{
  public required string Slug { get; set; }
  public required string Title { get; set; }
  public DateTime Published { get; set; }
  public string[]? Tags { get; set; }
  public string FeaturedImage { get; set; } = string.Empty;

  // The "image:" value from the front matter, relative to the post folder.
  [Newtonsoft.Json.JsonIgnore]
  public string? FeaturedImageFile { get; set; }

  public override string ToString() => Title;

  // Newest first; posts published at the same time are ordered by slug so
  // that posts.json is the same on every run.
  public int CompareTo(Post? other)
  {
    if (other is null)
    {
      return -1;
    }
    var byDate = other.Published.CompareTo(Published);
    return byDate != 0 ? byDate : string.CompareOrdinal(Slug, other.Slug);
  }
}
