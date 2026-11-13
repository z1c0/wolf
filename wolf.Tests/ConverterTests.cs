using static Wolf.Tests.TempBlog;

namespace Wolf.Tests;

public class ConverterTests
{
  [Fact]
  public void Writes_html_named_after_the_post_folder()
  {
    using var blog = new TempBlog().Post("hello", "# Hi\n\nSome *text*.");

    var result = blog.Run();

    Assert.Empty(result.Errors);
    Assert.Equal("<h1 id=\"hi\">Hi</h1>\n<p>Some <em>text</em>.</p>\n", blog.Html("hello"));
  }

  [Theory]
  [InlineData("hello.md")]
  [InlineData("index.md")]
  [InlineData("default.md")]
  [InlineData("post.md")]
  public void Finds_the_markdown_file_by_its_known_names(string fileName)
  {
    using var blog = new TempBlog().Post("hello", "text", fileName);

    blog.Run();

    Assert.Contains("text", blog.Html("hello"));
  }

  [Fact]
  public void Prefers_the_file_named_after_the_folder()
  {
    using var blog = new TempBlog()
      .Post("hello", "from folder name", "hello.md")
      .Post("hello", "from post.md", "post.md");

    blog.Run();

    Assert.Contains("from folder name", blog.Html("hello"));
  }

  [Fact]
  public void Warns_about_a_folder_without_markdown()
  {
    using var blog = new TempBlog().File("empty/readme.txt");

    var result = blog.Run();

    Assert.Empty(result.Errors);
    Assert.Contains(result.Warnings, w => w.Contains("No Markdown"));
  }

  [Fact]
  public void Reports_a_missing_input_directory()
  {
    using var blog = new TempBlog();
    var config = blog.Config();
    config.InputDirectory = Path.Combine(blog.Root, "nope");

    var result = blog.Run(config);

    Assert.Contains(result.Errors, e => e.Contains("does not exist"));
  }

  [Fact]
  public void A_broken_post_does_not_stop_the_others()
  {
    using var blog = new TempBlog()
      .Post("a-good", FrontMatter("published: 2026-01-01") + "first")
      .Post("b-bad", FrontMatter("published: not-a-date") + "broken")
      .Post("c-good", FrontMatter("published: 2026-01-02") + "last");

    var result = blog.Run();

    Assert.Contains(result.Errors, e => e.Contains("not-a-date"));
    Assert.Contains("first", blog.Html("a-good"));
    Assert.Contains("last", blog.Html("c-good"));
  }

  [Fact]
  public void Keeps_the_previous_index_when_a_post_fails()
  {
    using var blog = new TempBlog()
      .Post("good", "fine")
      .Post("bad", FrontMatter("published: 31.31.2026") + "broken");
    var postsJson = Path.Combine(blog.Input, "posts.json");
    File.WriteAllText(postsJson, "previous");

    var result = blog.Run();

    Assert.Contains(result.Errors, e => e.Contains("not written"));
    Assert.Equal("previous", File.ReadAllText(postsJson));
  }

  [Fact]
  public void Writes_no_index_when_disabled()
  {
    using var blog = new TempBlog().Post("hello", "text");
    var config = blog.Config();
    config.GenerateIndex = false;

    blog.Run(config);

    Assert.False(File.Exists(Path.Combine(blog.Input, "posts.json")));
  }
}
