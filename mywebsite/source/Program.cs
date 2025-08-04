using Markdig;
using System.Text.RegularExpressions;

namespace mywebsite;

internal class Program
{
    const string ReplaceThisWithBlog = "<!-- Blog Content Here -->";
    const string DefaultTemplate = "Content/Templates/Default.html";
    const string PublishPath = "public";

    static void Main()
    {
        var root = GetProjectRoot();
        Directory.SetCurrentDirectory(root);

        if (Directory.Exists(PublishPath))
            Directory.Delete(PublishPath, true);
        Directory.CreateDirectory(PublishPath);

        foreach (var file in Directory.GetFiles("Content/Blogs", "index.md", SearchOption.AllDirectories))
        {
            string dirName = Path.GetFileName(Path.GetDirectoryName(file)!);
            string blogName = Regex.Replace(dirName, @"^\d+_", "").Replace('_', ' ');

            WriteBlog(blogName, file);
        }
    }

    static string GetProjectRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(dir, "mywebsite.csproj")))
            dir = Path.GetFullPath(Path.Combine(dir, ".."));
        return dir;
    }

    static void WriteBlog(string title, string filePath)
    {
        string content = File.ReadAllText(filePath);
        content = Markdown.ToHtml(content);
        string template = File.ReadAllText(DefaultTemplate);

        string html = template.Replace(ReplaceThisWithBlog, content);
        string outputPath = Path.Combine(PublishPath, Path.GetFileName(Path.GetDirectoryName(filePath)!) + ".html");

        File.WriteAllText(outputPath, html);
    }
}
