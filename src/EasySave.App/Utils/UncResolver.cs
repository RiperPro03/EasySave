using System.Runtime.InteropServices;
using System.Text;

namespace EasySave.App.Utils;

internal static class UncResolver
{
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

    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetGetConnection(
        string localName,
        StringBuilder remoteName,
        ref int length);

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

    private static bool IsUnc(string p) => p.Length >= 2 && p[0] == '\\' && p[1] == '\\';

    private static bool IsDrivePath(string p) =>
        p.Length >= 3 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/');
}
