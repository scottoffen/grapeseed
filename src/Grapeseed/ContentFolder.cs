using Microsoft.Extensions.Logging;

namespace Grapevine;

public class ContentFolder : ContentFolderBase, IContentFolder, IDisposable
{
    private string _indexFileName = DefaultIndexFileName;
    private string? _prefix = string.Empty;
    private string _path = string.Empty;

    public ILogger<IContentFolder> Logger { get; protected set; }

    public ContentFolder(string path) : this(path, null, null) { }

    public ContentFolder(string path, string prefix) : this(path, prefix, null) { }

    public ContentFolder(string path, Func<IHttpContext, Task> handler) : this(path, null, handler) { }

    public ContentFolder(string path, string? prefix, Func<IHttpContext, Task>? handler)
    {
        Logger = DefaultLogger.GetInstance<IContentFolder>();
        FolderPath = path;
        Prefix = prefix;
        FileNotFoundHandler = handler ?? DefaultFileNotFoundHandler;
    }

    public override string FolderPath
    {
        get => _path;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var path = Path.GetFullPath(value);
            if (_path == path) return;

            if (!Directory.Exists(path)) path = Directory.CreateDirectory(path).FullName;
            _path = path;
            DirectoryMapping?.Clear();

            Watcher?.Dispose();
            Watcher = new FileSystemWatcher
            {
                Path = _path,
                Filter = "*",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
            };

            Watcher.Created += (sender, args) => { AddToDirectoryListing(args.FullPath); };
            Watcher.Deleted += (sender, args) => { RemoveFromDirectoryListing(args.FullPath); };
            Watcher.Renamed += (sender, args) => { RenameInDirectoryListing(args.OldFullPath, args.FullPath); };
        }
    }

    public override string IndexFileName
    {
        get { return _indexFileName; }
        set
        {
            if (string.IsNullOrWhiteSpace(value) || _indexFileName.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return;
            _indexFileName = value;
            DirectoryMapping?.Clear();
        }
    }

    public override string? Prefix
    {
        get { return _prefix; }
        set
        {
            var prefix = string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : $"/{value.Trim().TrimStart('/').TrimEnd('/').Trim()}";

            if (_prefix?.Equals(prefix, StringComparison.CurrentCultureIgnoreCase) ?? false) return;

            _prefix = prefix;
            DirectoryMapping?.Clear();
        }
    }

    public void Dispose()
    {
        Watcher?.Dispose();
    }

    public override async Task SendFileAsync(IHttpContext context)
    {
        await SendFileAsync(context, null);
    }

    public override async Task SendFileAsync(IHttpContext context, string? filename)
    {
        PopulateDirectoryListing();

        if (DirectoryMapping.ContainsKey(context.Request.Endpoint))
        {
            var filepath = DirectoryMapping[context.Request.Endpoint];
            context.Response.StatusCode = HttpStatusCode.Ok;

            var lastModified = File.GetLastWriteTimeUtc(filepath).ToString("R");
            context.Response.AddHeader("Last-Modified", lastModified);

            if (context.Request.Headers.AllKeys.Contains("If-Modified-Since"))
            {
                var header = context.Request.Headers["If-Modified-Since"];
                if (!string.IsNullOrEmpty(header) && header.Equals(lastModified))
                {
                    await context.Response.SendResponseAsync(HttpStatusCode.NotModified).ConfigureAwait(false);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(filename))
                context.Response.AddHeader("Content-Disposition", $"attachment; filename=\"{filename}\"");

            context.Response.ContentType = ContentType.FindKey(Path.GetExtension(filepath).TrimStart('.').ToLower());

            using (var stream = new FileStream(filepath, FileMode.Open))
            {
                await context.Response.SendResponseAsync(stream);
            }
        }

        // File not found, but should have been based on the path info
        else if (!string.IsNullOrEmpty(Prefix) && context.Request.Endpoint.StartsWith(Prefix, StringComparison.CurrentCultureIgnoreCase))
        {
            context.Response.StatusCode = HttpStatusCode.NotFound;
        }
    }
}
