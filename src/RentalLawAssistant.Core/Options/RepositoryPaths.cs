namespace RentalLawAssistant.Core.Options;

/// <summary>
/// Resolves configured relative paths (e.g. "data/corpus") against the repository root, so the
/// console apps and the API find the same files no matter which directory they are started from.
/// </summary>
public static class RepositoryPaths
{
    /// <summary>File that marks the repository root.</summary>
    public const string MarkerFileName = "RentalLawAssistant.slnx";

    /// <summary>
    /// Returns <paramref name="path"/> unchanged (normalized) when it is absolute; otherwise combines it
    /// with the first ancestor of <paramref name="startDirectory"/> that contains the marker file,
    /// falling back to the current directory.
    /// </summary>
    public static string Resolve(string path, string? startDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        var root = FindRoot(startDirectory ?? AppContext.BaseDirectory) ?? Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(root, path));
    }

    private static string? FindRoot(string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, MarkerFileName)))
            {
                return directory.FullName;
            }
        }

        return null;
    }
}
