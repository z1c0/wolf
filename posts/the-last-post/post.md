---
Title: The last example post
Published: 2017-01-24
Tags: blog fun
Image: pic0.png
---

This post contains source code:

```csharp
internal static void Error(string msg)
{
  var color = Console.ForegroundColor;
  Console.ForegroundColor = ConsoleColor.Red;
  Console.Write("ERROR: ");
  Console.WriteLine(msg);
  Console.ForegroundColor = color;
}
```

