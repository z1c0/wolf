using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Wolf
{
  internal class Index
  {
    private Log _log;
    private Config _config;
    private readonly List<string> _tags = new();
    private readonly List<Post> _posts = new();

    internal Index(Log log, Config config)
    {
      _log = log;
      _config = config;
    }
    
    internal void Add(string slug, string featuredImage, IEnumerable<string> lines)
    {
      var post = new Post 
      { 
        Slug = slug,
        Title = slug,
        FeaturedImage = featuredImage
      };
      if (lines != null)
      {
        foreach (var l in lines)
        {
          var i = l.IndexOf(':');
          if (i >= 0)
          {
            var key = l.Substring(0, i).Trim().ToLower();
            var value = l.Substring(i + 1).Trim();
            switch (key)
            {
              case "title":
                post.Title = value;
                break;

              case "published":
                post.Published = DateTime.Parse(value, CultureInfo.InvariantCulture);
                break;

              case "image":
                post.FeaturedImage = _config.ImagePrefix + slug + '/' + value;
                break;

              case "tags":
              case "categories":
                post.Tags = value.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var tag in post.Tags)
                {
                  if (_tags.Find(t => string.Equals(t, tag, StringComparison.CurrentCultureIgnoreCase)) == null)
                  {
                    _tags.Add(tag);
                  }
                }
                break;

              default:
                _log.Warning($"Unrecognized YAML header '{l}' in '{slug}'");
                break;
            }
          }
        }
      }
      _posts.Add(post);
    }
    
    internal void Save()
    {
      _posts.Sort();  // Post class implements "IComparable" on the publish date.
      var settings = new JsonSerializerSettings 
      { 
        ContractResolver = new CamelCasePropertyNamesContractResolver() 
      };
      File.WriteAllText(PostsFileName, JsonConvert.SerializeObject(_posts, Formatting.Indented, settings));

      if (_config.GenerateTagsFile)
      {
        // Report newly introduced tags in verbose mode.
        if (_config.Verbose)
        {
          if (File.Exists(TagsFileName))
          {
            var oldTags = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(TagsFileName));
            var newTags = _tags.Except(oldTags);
            if (newTags.Any())
            {
              _log.Info("New tags were encountered:");
              foreach (var t in newTags)
              {
                _log.Info($"- {t}");
              }
            }
          }
        }
        _tags.Sort();
        File.WriteAllText(TagsFileName, JsonConvert.SerializeObject(_tags, Formatting.Indented, settings));
      }
    }

    private string PostsFileName => Path.Combine(_config.IndexDirectory, "posts.json");
    private string TagsFileName => Path.Combine(_config.IndexDirectory, "tags.json");
  }
}