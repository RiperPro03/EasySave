namespace EasySave.Core.Interfaces;

/// <summary>
/// Abstraction du système de fichiers.
/// </summary>
public interface IFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);

    IEnumerable<string> EnumerateFiles(string path, bool recursive);

    bool FileExists(string path);
    long GetFileSize(string path);
    DateTime GetLastWriteTimeUtc(string path);

    void CopyFile(string sourcePath, string targetPath, bool overwrite);
}