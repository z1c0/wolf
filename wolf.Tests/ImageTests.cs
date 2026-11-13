using static Wolf.Tests.TempBlog;

namespace Wolf.Tests;

public class ImageTests
{
  [Fact]
  public void Prefixes_local_images()
  {
    using var blog = new TempBlog()
      .Post("trip", "![a](pic.png)")
      .File("trip/pic.png");

    blog.Run();

    Assert.Contains("src=\"/posts/trip/pic.png\"", blog.Html("trip"));
  }

  [Fact]
  public void Leaves_missing_and_absolute_images_alone()
  {
    using var blog = new TempBlog()
      .Post("trip", "![a](missing.png) ![b](https://example.com/b.png) ![c](/static/c.png)");

    var result = blog.Run();

    var html = blog.Html("trip");
    Assert.Empty(result.Errors);
    Assert.Contains("src=\"missing.png\"", html);
    Assert.Contains("src=\"https://example.com/b.png\"", html);
    Assert.Contains("src=\"/static/c.png\"", html);
  }

  [Fact]
  public void Finds_url_encoded_file_names()
  {
    using var blog = new TempBlog()
      .Post("trip", "![a](my%20pic.png)")
      .File("trip/my pic.png");

    blog.Run(blog.Config(separateOutput: true));

    Assert.Contains("src=\"/posts/trip/my%20pic.png\"", blog.Html("trip", separateOutput: true));
    Assert.True(File.Exists(Path.Combine(blog.Output, "trip", "my pic.png")));
  }

  [Fact]
  public void Copies_images_including_subfolders_to_a_separate_output()
  {
    using var blog = new TempBlog()
      .Post("trip", "![a](pic.png) ![b](more/deep.png)")
      .File("trip/pic.png")
      .File("trip/more/deep.png");

    var result = blog.Run(blog.Config(separateOutput: true));

    Assert.Empty(result.Errors);
    Assert.True(File.Exists(Path.Combine(blog.Output, "trip", "pic.png")));
    Assert.True(File.Exists(Path.Combine(blog.Output, "trip", "more", "deep.png")));
  }

  [Fact]
  public void Copies_a_featured_image_that_only_the_front_matter_names()
  {
    using var blog = new TempBlog()
      .Post("trip", FrontMatter("image: cover.png") + "no images in the text")
      .File("trip/cover.png");

    blog.Run(blog.Config(separateOutput: true));

    Assert.True(File.Exists(Path.Combine(blog.Output, "trip", "cover.png")));
  }

  [Fact]
  public void First_image_is_the_featured_image()
  {
    using var blog = new TempBlog()
      .Post("trip", "![a](one.png) ![b](two.png)")
      .File("trip/one.png")
      .File("trip/two.png");

    blog.Run();

    Assert.Equal("/posts/trip/one.png", (string?)blog.Index()[0]["featuredImage"]);
  }

  [Fact]
  public void Front_matter_image_overrides_the_first_image()
  {
    using var blog = new TempBlog()
      .Post("trip", FrontMatter("image: cover.png") + "![a](one.png)")
      .File("trip/one.png")
      .File("trip/cover.png");

    blog.Run();

    Assert.Equal("/posts/trip/cover.png", (string?)blog.Index()[0]["featuredImage"]);
  }
}
