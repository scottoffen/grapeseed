using System.Collections.Concurrent;

namespace Grapeseed;

public abstract class ContentFolderBase : IContentFolder
{
    public static string DefaultIndexFileName { get; } = "index.html";

    public static Func<IHttpContext, Task> DefaultFileNotFoundHandler { get; set; } = async (context) =>
    {
        context.Response.StatusCode = HttpStatusCode.NotFound;
        var content = $"File Not Found: {context.Request.Endpoint}";
        await context.Response.SendResponseAsync(content);
    };

    public ConcurrentDictionary<string, string> DirectoryMapping { get; protected set; } = new ConcurrentDictionary<string, string>();

    protected FileSystemWatcher? Watcher;

    public abstract string IndexFileName { get; set; }

    public abstract string? Prefix { get; set; }

    public abstract string FolderPath { get; set; }

    public Func<IHttpContext, Task> FileNotFoundHandler { get; set; } = DefaultFileNotFoundHandler;

    public IList<string> DirectoryListing => DirectoryMapping.Values.ToList();

    public virtual void AddToDirectoryListing(string fullPath)
    {
        if (DirectoryMapping == null)
            DirectoryMapping = new ConcurrentDictionary<string, string>();

        DirectoryMapping[CreateDirectoryListingKey(fullPath)] = fullPath;

        if (fullPath.EndsWith($"{Path.DirectorySeparatorChar}{IndexFileName}", StringComparison.CurrentCultureIgnoreCase))
            DirectoryMapping[CreateDirectoryListingKey(fullPath.Replace($"{Path.DirectorySeparatorChar}{IndexFileName}", ""))] = fullPath;
    }

    public virtual string CreateDirectoryListingKey(string item)
    {
        return $"{Prefix}{item.Replace(FolderPath, string.Empty).Replace(@"\", "/")}";
    }

    public virtual void PopulateDirectoryListing()
    {
        if (DirectoryMapping?.Count > 0) return;

        Directory.GetFiles(FolderPath, "*", SearchOption.AllDirectories)
            .ToList()
            .ForEach(AddToDirectoryListing);
    }

    public virtual void RemoveFromDirectoryListing(string fullPath)
    {
        if (DirectoryMapping == null) return;

        DirectoryMapping.Where(x => x.Value == fullPath)
            .ToList()
            .ForEach(pair => DirectoryMapping.TryRemove(pair.Key, out var key));
    }

    public virtual void RenameInDirectoryListing(string oldFullPath, string newFullPath)
    {
        RemoveFromDirectoryListing(oldFullPath);
        AddToDirectoryListing(newFullPath);
    }

    public abstract Task SendFileAsync(IHttpContext context);

    public abstract Task SendFileAsync(IHttpContext context, string? filename);
}

