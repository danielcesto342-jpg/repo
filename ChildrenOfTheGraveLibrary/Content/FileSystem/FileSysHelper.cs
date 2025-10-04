using System.IO;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content.FileSystem;

internal static class FileSysHelper
{
    internal static void ParseFileSpecification(string pIn, out string path, out string name, out string extension)
    {
        path = Path.GetDirectoryName(pIn) ?? "";
        name = Path.GetFileNameWithoutExtension(pIn);
        extension = Path.GetExtension(pIn);
        //Remove '.'
        if (extension.Length > 0)
        {
            extension = extension[1..];
        }
    }

    internal static void RelativeToAbsolutePath(string relativePath, out string fullPath)
    {
        fullPath = Path.GetFullPath(relativePath);
        FormatPath(ref fullPath);
    }

    internal static void FormatPath(ref string path)
    {
        path = path.Replace('\\', '/');
    }

    internal static void ReplaceCharacter(ref string path, char from, char to)
    {
        path = path.Replace(from, to);
    }

    internal static void CreatePathDirectories(string path)
    {
        FormatPath(ref path);
        Directory.CreateDirectory(path);
    }
}
