namespace Wolf.Tests;

// Converts the sample posts in the repo's posts/ folder and compares the
// result with wolf.Tests/Expected. Catches unintended output changes, e.g.
// from a Markdig update. After an intended change, regenerate Expected with
// WOLF_UPDATE_EXPECTED=1 dotnet test --solution wolf.sln
public class SamplePostsTests
{
  private static readonly string[] OutputFiles =
  [
    "posts.json",
    "first-post/first-post.html",
    "post-2/post-2.html",
    "the-last-post/the-last-post.html",
  ];

  [Fact]
  public void Output_matches_the_expected_files()
  {
    using var blog = new TempBlog();
    CopyDirectory(Path.Combine(AppContext.BaseDirectory, "SamplePosts"), blog.Input);
    var config = blog.Config(separateOutput: true);
    config.ImagePrefix = "/posts/";

    var result = blog.Run(config);
    Assert.Empty(result.Errors);

    if (Environment.GetEnvironmentVariable("WOLF_UPDATE_EXPECTED") == "1")
    {
      var target = Path.Combine(FindRepoRoot(), "wolf.Tests", "Expected");
      foreach (var f in OutputFiles)
      {
        var dest = Path.Combine(target, f);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(Path.Combine(blog.Output, f), dest, overwrite: true);
      }
      return;
    }

    foreach (var f in OutputFiles)
    {
      var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Expected", f));
      Assert.Equal(expected, File.ReadAllText(Path.Combine(blog.Output, f)));
    }
  }

  private static void CopyDirectory(string from, string to)
  {
    foreach (var file in Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories))
    {
      var dest = Path.Combine(to, Path.GetRelativePath(from, file));
      Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
      File.Copy(file, dest);
    }
  }

  private static string FindRepoRoot()
  {
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null && !File.Exists(Path.Combine(dir.FullName, "wolf.sln")))
    {
      dir = dir.Parent;
    }
    return dir?.FullName ?? throw new InvalidOperationException("wolf.sln not found");
  }
}
