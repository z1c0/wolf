using static Wolf.Tests.TempBlog;

namespace Wolf.Tests;

public class IndexTests
{
  [Fact]
  public void Reads_the_front_matter()
  {
    using var blog = new TempBlog().Post("hello", FrontMatter(
      "Title: Hello: World",
      "Published: 2026-03-15 11:00",
      "Categories: Dev  AI") + "text");

    var result = blog.Run();

    Assert.Empty(result.Errors);
    var post = blog.Index()[0];
    Assert.Equal("hello", (string?)post["slug"]);
    Assert.Equal("Hello: World", (string?)post["title"]);
    Assert.Equal(new DateTime(2026, 3, 15, 11, 0, 0), (DateTime?)post["published"]);
    Assert.Equal(["Dev", "AI"], post["tags"]!.Select(t => (string)t!));
  }

  [Fact]
  public void Uses_the_slug_as_title_without_front_matter()
  {
    using var blog = new TempBlog().Post("plain-post", "text");

    blog.Run();

    Assert.Equal("plain-post", (string?)blog.Index()[0]["title"]);
  }

  [Fact]
  public void Sorts_newest_first_and_by_slug_on_equal_dates()
  {
    using var blog = new TempBlog()
      .Post("old", FrontMatter("published: 2020-01-01") + "x")
      .Post("same-b", FrontMatter("published: 2026-01-01") + "x")
      .Post("same-a", FrontMatter("published: 2026-01-01") + "x")
      .Post("new", FrontMatter("published: 2026-06-01") + "x");

    blog.Run();

    Assert.Equal(["new", "same-a", "same-b", "old"], blog.Index().Select(p => (string)p["slug"]!));
  }

  [Fact]
  public void Warns_about_unknown_keys_and_yaml_lists()
  {
    using var blog = new TempBlog().Post("hello", FrontMatter(
      "author: me",
      "tags:",
      "  - a") + "text");

    var result = blog.Run();

    Assert.Empty(result.Errors);
    Assert.Contains(result.Warnings, w => w.Contains("Unrecognized YAML header 'author: me'"));
    Assert.Contains(result.Warnings, w => w.Contains("No tags"));
    Assert.Contains(result.Warnings, w => w.Contains("'  - a'"));
  }

  [Fact]
  public void Writes_a_sorted_tag_file_without_case_duplicates()
  {
    using var blog = new TempBlog()
      .Post("one", FrontMatter("tags: ruby Dev") + "x")
      .Post("two", FrontMatter("tags: dev AI") + "x");
    var config = blog.Config();
    config.GenerateTagsFile = true;

    blog.Run(config);

    var tags = Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText(Path.Combine(blog.Input, "tags.json")));
    Assert.Equal(["AI", "Dev", "ruby"], tags.Select(t => (string)t!));
  }
}
