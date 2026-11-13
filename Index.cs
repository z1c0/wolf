using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Wolf;

internal class Index(Log log, Config config)
{
  private readonly List<string> _tags = [];
  private readonly List<Post> _posts = [];

  internal Post Add(string slug, string featuredImage, IEnumerable<string>? lines)
  {
    var post = new Post
    {
      Slug = slug,
      Title = slug,
      FeaturedImage = featuredImage
    };
    foreach (var l in lines ?? [])
    {
      var i = l.IndexOf(':');
      if (i < 0)
      {
        if (!string.IsNullOrWhiteSpace(l))
        {
          log.Warning($"Ignored YAML line '{l}' in '{slug}' (only 'key: value' lines are supported)");
        }
        continue;
      }
      var key = l[..i].Trim().ToLowerInvariant();
      var value = l[(i + 1)..].Trim();
      switch (key)
      {
        case "title":
          post.Title = value;
          break;

        case "published":
          if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var published))
          {
            post.Published = published;
          }
          else
          {
            log.Error($"Invalid 'published' date '{value}' in '{slug}'");
          }
          break;

        case "image":
          post.FeaturedImageFile = value;
          post.FeaturedImage = config.ImagePrefix + slug + '/' + value;
          break;

        case "tags":
        case "categories":
          post.Tags = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
          if (post.Tags.Length == 0)
          {
            log.Warning($"No tags in '{slug}' (tags must be space-separated on the same line)");
          }
          foreach (var tag in post.Tags)
          {
            if (!_tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
              _tags.Add(tag);
            }
          }
          break;

        default:
          log.Warning($"Unrecognized YAML header '{l}' in '{slug}'");
          break;
      }
    }
    _posts.Add(post);
    return post;
  }

  internal void Save()
  {
    _posts.Sort();  // Newest first, see Post.CompareTo.
    var settings = new JsonSerializerSettings
    {
      ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
    Directory.CreateDirectory(config.IndexDirectory);
    File.WriteAllText(PostsFileName, JsonConvert.SerializeObject(_posts, Formatting.Indented, settings));

    if (config.GenerateTagsFile)
    {
      // Report newly introduced tags in verbose mode.
      if (config.Verbose && File.Exists(TagsFileName))
      {
        var oldTags = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(TagsFileName)) ?? [];
        var newTags = _tags.Except(oldTags, StringComparer.OrdinalIgnoreCase).ToList();
        if (newTags.Count > 0)
        {
          log.Info("New tags were encountered:");
          foreach (var t in newTags)
          {
            log.Info($"- {t}");
          }
        }
      }
      _tags.Sort(StringComparer.OrdinalIgnoreCase);
      File.WriteAllText(TagsFileName, JsonConvert.SerializeObject(_tags, Formatting.Indented, settings));
    }
  }

  private string PostsFileName => Path.Combine(config.IndexDirectory, "posts.json");
  private string TagsFileName => Path.Combine(config.IndexDirectory, "tags.json");
}
