namespace Wolf.Tests;

// Program.Main writes to the console, so these run one at a time.
[Collection(nameof(ProgramTests))]
public class ProgramTests
{
  [Fact]
  public void Exits_with_0_on_success()
  {
    using var blog = new TempBlog().Post("hello", "text");

    Assert.Equal(0, Program.Main(["-n", "-i", blog.Input, "-p", "/posts/"]));
    Assert.True(File.Exists(Path.Combine(blog.Input, "posts.json")));
  }

  [Fact]
  public void Exits_with_1_when_a_post_fails()
  {
    using var blog = new TempBlog().Post("bad", TempBlog.FrontMatter("published: never") + "x");

    Assert.Equal(1, Program.Main(["-n", "-i", blog.Input]));
  }

  [Fact]
  public void Exits_non_zero_on_an_unknown_option()
  {
    Assert.NotEqual(0, Program.Main(["--bogus"]));
  }

  [Fact]
  public void Short_and_long_options_are_equivalent()
  {
    using var blog = new TempBlog().Post("hello", "text");

    Program.Main(["--nologo", "--inputDirectory", blog.Input, "--outputDirectory", blog.Output, "-nx"]);

    Assert.True(File.Exists(Path.Combine(blog.Output, "hello", "hello.html")));
    Assert.False(File.Exists(Path.Combine(blog.Output, "posts.json")));
  }
}
