namespace Grapevine;

public static class IContentFolderExtensions
{
    /// <summary>
    /// Adds a new <see cref="ContentFolder"/> to the collection.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="folderPath"></param>
    public static void Add(this ICollection<IContentFolder> collection, string folderPath)
    {
        collection.Add(new ContentFolder(folderPath));
    }

    /// <summary>
    /// Adds a range of new <see cref="ContentFolder"/>s to the collection.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="folderPaths"></param>
    public static void AddRange(this ICollection<IContentFolder> collection, params string[] folderPaths)
    {
        foreach (var path in folderPaths)
        {
            collection.Add(new ContentFolder(path));
        }
    }
}
