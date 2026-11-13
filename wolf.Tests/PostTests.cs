namespace Wolf.Tests;

public class PostTests
{
  private static Post At(string slug, int day) =>
    new() { Slug = slug, Title = slug, Published = new DateTime(2026, 1, day) };

  [Fact]
  public void Newer_posts_sort_first()
  {
    Assert.True(At("a", 2).CompareTo(At("b", 1)) < 0);
    Assert.True(At("a", 1).CompareTo(At("b", 2)) > 0);
  }

  [Fact]
  public void Equal_dates_sort_by_slug()
  {
    Assert.True(At("a", 1).CompareTo(At("b", 1)) < 0);
    Assert.True(At("b", 1).CompareTo(At("a", 1)) > 0);
    Assert.Equal(0, At("a", 1).CompareTo(At("a", 1)));
  }
}
