using Markdig;
using System.IO;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace mywebsite;

internal class Program
{
    const string ReplaceThisWithBlogContent = "<!-- Blog Content Here -->";
    const string ReplaceThisWithBlogLinks = "<!-- Blog Links Here -->";
    const string DefaultTemplate = "Content/Templates/Default.html";
    const string PublishPath = "public";

    static List<Blog> Blogs = new List<Blog>();

    static void Main()
    {
        var root = GetProjectRoot();
        Directory.SetCurrentDirectory(root);

        if (Directory.Exists(PublishPath))
            Directory.Delete(PublishPath, true);
        Directory.CreateDirectory(PublishPath);

        foreach (var directory in Directory.GetDirectories("Content/Blogs", "", SearchOption.TopDirectoryOnly))
        {
            WriteBlog(directory);
        }

        HomeSite();
        CopyContent();
    }

    static void CopyContent()
    {
        if (!Directory.Exists(Path.Combine(PublishPath, "Content")))
        {
            Directory.CreateDirectory(Path.Combine(PublishPath, "Content"));
        }

        foreach(var file in Directory.GetFiles("Content", "", SearchOption.AllDirectories))
        {
            if(Path.GetExtension(file) != ".html" && Path.GetExtension(file) != ".md")
            {
                Console.WriteLine("copying content: " + file);
                File.Copy(file, Path.Combine(PublishPath, file));
            }
        }
    }

    static void WriteBlog(string BlogDirectory)
    {
        string unmodifiedBlogName = Path.GetFileName(BlogDirectory);
        string blogName = unmodifiedBlogName.Replace("_", " ");
        blogName = blogName.Remove(0, 4); // remove numbers and spacing
        Console.WriteLine("current blog: " + blogName);

        string content = File.ReadAllText(Path.Combine(BlogDirectory, "index.md"));
        content = Markdown.ToHtml(content);
        string template = File.ReadAllText(DefaultTemplate);

        string html = template.Replace(ReplaceThisWithBlogContent, content);
        string outputPath = Path.Combine(PublishPath, Path.Combine("Blogs", unmodifiedBlogName, "index.html"));
        Directory.CreateDirectory(Path.Combine(PublishPath, "Blogs", unmodifiedBlogName));
        File.WriteAllText(outputPath, html);

        Blogs.Add(new Blog()
        {
            Name = blogName,
            Description = unmodifiedBlogName,
            UnmodifiedName = unmodifiedBlogName,
        });
    }

    static void HomeSite() 
    {
        string html = File.ReadAllText("Content/index.html");
        string htmlWithBlogLinks = "";

        foreach(var blog in Blogs)
        {
            htmlWithBlogLinks += $"<li>\r\n<a href=\"Blogs/{blog.UnmodifiedName}/index.html\" class=\"title\">{blog.Name}</a>\r\n<p>{blog.Description}</p>\r\n</li>";
        }

        html = html.Replace(ReplaceThisWithBlogLinks, htmlWithBlogLinks);

        File.WriteAllText(Path.Combine(PublishPath, "index.html"), html);
    }

    static string GetProjectRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(dir, "mywebsite.csproj")))
            dir = Path.GetFullPath(Path.Combine(dir, ".."));
        return dir;
    }
}
