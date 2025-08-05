using Markdig;
using System.IO;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace mywebsite;

internal class Program
{
    const string ReplaceThisWithBlogContent = "<!-- Blog Content Here -->";
    const string ReplaceThisWithBlogLinks = "<!-- Blog Links Here -->";

    const string ReplaceThisWithProjectLinks = "<!-- Project Links Here -->";

    const string ReplaceThisWithBlogTitle = "<!-- Blog Title Here -->";


    const string DefaultTemplate = "Content/Templates/Default.html";
    const string PublishPath = "public";

    static List<Blog> Blogs = new List<Blog>();
    static List<Project> Projects = new List<Project>();

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
        if (!Directory.Exists("Content/Projects"))
        {
            Console.WriteLine("ERROR: 'Content/Projects' does not exist.");
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

        foreach (var directory in Directory.GetFiles("Content/Projects"))
        {
            WriteProject(directory);
        }

        HomeSite();
        CopyContent();
    }

    static void WriteProject(string projectDirectory)
    {
        string unmodifiedProjectName = Path.GetFileName(projectDirectory);
        string projectName = unmodifiedProjectName.Remove(0, 3).Replace(".md", ""); // remove number

        Console.WriteLine("current project: " + projectName);

        string[] splitContent = File.ReadAllText(projectDirectory).Split("\n");

        string url = splitContent[0];
        string description = string.Empty;
        if(splitContent.Length > 1)
        {
            description = splitContent[1];
        }


        Projects.Add(new Project
        {
            Name = projectName,
            Description = description,
            Url = url,
        });
    }

    static void WriteBlog(string BlogDirectory)
    {
        string unmodifiedBlogName = Path.GetFileName(BlogDirectory);
        string blogName = unmodifiedBlogName.Replace("_", " ");
        blogName = blogName.Remove(0, 4); // remove numbers and spacing
        Console.WriteLine("current blog: " + blogName);

        string mdPath = Path.Combine(BlogDirectory, "index.md");
        string markdown = File.ReadAllText(mdPath);

        string description = unmodifiedBlogName;
        var match = Regex.Match(markdown, @"^DESCRIPTION:\s*(.+)$", RegexOptions.Multiline);
        if (match.Success)
        {
            description = match.Groups[1].Value.Trim();

            // remove the description
            markdown = Regex.Replace(markdown, @"^DESCRIPTION:\s*.+\r?\n?", "", RegexOptions.Multiline);
        }

        string title = blogName;

        var titleMatch = Regex.Match(markdown, @"^TITLE:\s*(.+)$", RegexOptions.Multiline);
        if (titleMatch.Success)
        {
            title = titleMatch.Groups[1].Value.Trim();
            markdown = Regex.Replace(markdown, @"^TITLE:\s*.+\r?\n?", "", RegexOptions.Multiline);
        }

        string content = Markdown.ToHtml(markdown);
        string template = File.ReadAllText(DefaultTemplate);

        string html = template
            .Replace(ReplaceThisWithBlogTitle, $"<div class=\"blog-title\"><h1>{title}</h1></div>")
            .Replace(ReplaceThisWithBlogContent, content);

        string outputPath = Path.Combine(PublishPath, "Blogs", unmodifiedBlogName, "index.html");
        Directory.CreateDirectory(Path.Combine(PublishPath, "Blogs", unmodifiedBlogName));
        File.WriteAllText(outputPath, html);

        Blogs.Add(new Blog()
        {
            Name = blogName,
            Description = description,
            UnmodifiedName = unmodifiedBlogName,
        });
    }

    static void CopyContent()
    {
        if (!Directory.Exists(Path.Combine(PublishPath, "Content")))
        {
            Directory.CreateDirectory(Path.Combine(PublishPath, "Content"));
        }

        foreach (var file in Directory.GetFiles("Content", "", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(file) != ".html" && Path.GetExtension(file) != ".md")
            {
                Console.WriteLine("copying content: " + file);
                File.Copy(file, Path.Combine(PublishPath, file));
            }
        }
    }

    static void HomeSite()
    {
        string html = File.ReadAllText("Content/index.html");
        string htmlWithBlogLinks = @"<div class=""blog-grid"">";

        // Blogs

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

        // Projects
        string htmlWithProjectLinks = @"<div class=""blog-grid"">";

        foreach (var project in Projects)
        {
            htmlWithProjectLinks += $@"
<a href=""{project.Url}"" class=""blog-card-link"" target=""_blank"">
    <div class=""blog-card"">
        <span class=""title"">{project.Name}</span>
        <p>{project.Description}</p>
    </div>
</a>";
        }

        htmlWithProjectLinks += "</div>";

        html = html.Replace(ReplaceThisWithProjectLinks, htmlWithProjectLinks);



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
