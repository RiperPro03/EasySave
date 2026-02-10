using System.Runtime.InteropServices;
using System.Text;

namespace EasySave.App.Utils;

/// <summary>
/// Resolves local paths to UNC paths for logging.
/// </summary>
internal static class UncResolver
{
    /// <summary>
    /// Resolves a path to a UNC path when possible.
    /// </summary>
    /// <param name="path">The input path.</param>
    /// <returns>A UNC path or the original absolute path.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
    public static string ResolveToUncForLog(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is null or empty.", nameof(path));

        var fullPath = Path.GetFullPath(path);

        if (!OperatingSystem.IsWindows())
            return fullPath;

        if (IsUnc(fullPath))
            return fullPath;

        if (!IsDrivePath(fullPath))
            return fullPath;

        var driveRoot = fullPath.Substring(0, 2);
        var rest = fullPath.Substring(2);

        if (TryGetUncFromMappedDrive(driveRoot, out var uncRoot))
            return uncRoot + rest;

        return ToAdministrativeUnc(fullPath);
    }

    /// <summary>
    /// Attempts to resolve a mapped drive (for example, "Z:") to a UNC root.
    /// </summary>
    /// <param name="driveLetter">The drive letter with colon.</param>
    /// <param name="uncRoot">The resolved UNC root when successful.</param>
    /// <returns><c>true</c> if a UNC root was found; otherwise <c>false</c>.</returns>
    public static bool TryGetUncFromMappedDrive(string driveLetter, out string uncRoot)
    {
        uncRoot = string.Empty;

        if (!OperatingSystem.IsWindows())
            return false;

        if (string.IsNullOrWhiteSpace(driveLetter) || driveLetter.Length != 2 || driveLetter[1] != ':')
            return false;

        var sb = new StringBuilder(1024);
        int length = sb.Capacity;

        int result = WNetGetConnection(driveLetter, sb, ref length);
        if (result == 0)
        {
            uncRoot = sb.ToString();

            if (uncRoot.EndsWith("\\", StringComparison.Ordinal))
                uncRoot = uncRoot.TrimEnd('\\');

            return IsUnc(uncRoot);
        }

        return false;
    }

    /// <summary>
    /// P/Invoke for querying a mapped drive connection.
    /// </summary>
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetGetConnection(
        string localName,
        StringBuilder remoteName,
        ref int length);

    /// <summary>
    /// Converts a local drive path to an administrative UNC path.
    /// </summary>
    /// <param name="fullPath">A fully-qualified local path.</param>
    /// <returns>The administrative UNC path.</returns>
    private static string ToAdministrativeUnc(string fullPath)
    {
        if (!IsDrivePath(fullPath))
            return fullPath;

        string machine = Environment.MachineName;
        string drive = fullPath.Substring(0, 2);
        string rest = fullPath.Substring(2);
        string adminShare = drive[0] + "$";

        return $@"\\{machine}\{adminShare}{rest}";
    }

    /// <summary>
    /// Determines whether a path is already a UNC path.
    /// </summary>
    /// <param name="p">The path to check.</param>
    /// <returns><c>true</c> if the path is UNC; otherwise <c>false</c>.</returns>
    private static bool IsUnc(string p) => p.Length >= 2 && p[0] == '\\' && p[1] == '\\';

    /// <summary>
    /// Determines whether a path starts with a drive letter.
    /// </summary>
    /// <param name="p">The path to check.</param>
    /// <returns><c>true</c> if the path has a drive prefix; otherwise <c>false</c>.</returns>
    private static bool IsDrivePath(string p) =>
        p.Length >= 3 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/');
}
