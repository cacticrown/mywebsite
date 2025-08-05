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

        if (!Directory.Exists("Content"))
        {
            Console.WriteLine("ERROR: 'Content' directory does not exist.");
            return;
        }
        if (!File.Exists(DefaultTemplate))
        {
            Console.WriteLine($"ERROR: Template file '{DefaultTemplate}' does not exist.");
            return;
        }
        if (!Directory.Exists("Content/Blogs"))
        {
            Console.WriteLine("ERROR: 'Content/Blogs' directory does not exist.");
            return;
        }
        if (!File.Exists("Content/index.html"))
        {
            Console.WriteLine("ERROR: 'Content/index.html' does not exist.");
            return;
        }
        Console.WriteLine("all tests passed");

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
        string htmlWithBlogLinks = @"<div class=""blog-grid"">";

        foreach (var blog in Blogs)
        {
            htmlWithBlogLinks += $@"
<a href=""Blogs/{blog.UnmodifiedName}/index.html"" class=""blog-card-link"">
    <div class=""blog-card"">
        <span class=""title"">{blog.Name}</span>
        <p>{blog.Description}</p>
    </div>
</a>";
        }

        htmlWithBlogLinks += "</div>";

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
