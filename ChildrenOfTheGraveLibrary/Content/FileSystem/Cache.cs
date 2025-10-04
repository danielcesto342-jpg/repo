using ChildrenOfTheGraveEnumNetwork.Content;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content.FileSystem;

internal static class Cache
{
    private static readonly Dictionary<string, RFile> FileNameToFile = [];
    private static RFile LastAccessedFile = null!;
    private static string LastAccessedFileName = "";

    //Check
    private static string CreateFullPath(string filename)
    {
        string path = "";
        if (FileNameToFile is not null) //?
        {
            path = Directory.GetCurrentDirectory();
            FileSysHelper.RelativeToAbsolutePath(path, out path);
        }
        FileSysHelper.FormatPath(ref path);
        return $"{path}/{filename}";
    }

    internal static void PreloadFile(string fileName)
    {
        GetFile(fileName, false);
    }

    internal static RFile? GetFile(string fileName, bool skipCache /*unused*/ = false)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        RFile? file;

        if (!skipCache) //Custom? Maybe I overlooked this check in the disassembly
        {
            if (fileName == LastAccessedFileName)
            {
                file = LastAccessedFile;
            }
            else if (FileNameToFile.TryGetValue(fileName, out file))
            {
                LastAccessedFile = file;
                LastAccessedFileName = fileName;
            }

            if (file is not null)
            {
                if (!file.FileHasBeenModified)
                {
                    return file;
                }
            }
        }

        string fullPath = fileName;
        //fullPath = CreateFullPath(fileName);
        FileSysHelper.ParseFileSpecification(fullPath, out string folder, out string name, out string extension);
        string keyVal = $"{folder}/defaults/{name}.{extension}";
        file = new(Path.GetFileName(fileName), fullPath, keyVal);

        lock (FileNameToFile)
        {
            FileNameToFile[fileName] = file;
        }

        LastAccessedFile = file;
        LastAccessedFileName = fileName;
        return file;
    }

    internal static void GetValue(out string returnValue, string pFileName, string pSection, string pName, string pDefault = "", bool skipCache = false)
    {
        RFile? eax = GetFile(pFileName, skipCache);
        if (eax is null)
        {
            returnValue = pDefault;
            return;
        }
        eax.GetValue(out returnValue, pSection, pName, HashFunctions.HashStringSdbm(pSection, pName), pDefault);
    }

    internal static void GetValue(out bool returnValue, string pFileName, string pSection, string pName, bool pDefault = false, bool skipCache = false)
    {
        RFile? eax = GetFile(pFileName, skipCache);
        if (eax is null)
        {
            returnValue = pDefault;
            return;
        }
        eax.GetValue(out returnValue, pSection, pName, HashFunctions.HashStringSdbm(pSection, pName), pDefault);
    }

    internal static void GetValue(out Vector4 returnValue, string pFileName, string pSection, string pName, Vector4 pDefault = default, bool skipCache = false)
    {
        RFile? eax = GetFile(pFileName, skipCache);
        if (eax is null)
        {
            returnValue = pDefault;
            return;
        }
        eax.GetValue(out returnValue, pSection, pName, HashFunctions.HashStringSdbm(pSection, pName), pDefault);
    }

    internal static void GetValue(out Vector3 returnValue, string pFileName, string pSection, string pName, Vector3 pDefault = default, bool skipCache = false)
    {
        RFile? eax = GetFile(pFileName, skipCache);
        if (eax is null)
        {
            returnValue = pDefault;
            return;
        }
        eax.GetValue(out returnValue, pSection, pName, HashFunctions.HashStringSdbm(pSection, pName), pDefault);
    }

    internal static void GetValue(out Vector2 returnValue, string pFileName, string pSection, string pName, Vector2 pDefault = default, bool skipCache = false)
    {
        RFile? eax = GetFile(pFileName, skipCache);
        if (eax is null)
        {
            returnValue = pDefault;
            return;
        }
        eax.GetValue(out returnValue, pSection, pName, HashFunctions.HashStringSdbm(pSection, pName), pDefault);
    }

    internal static void GetValue(out float returnValue, string pFileName, string pSection, string pName, float pDefault = 0, bool skipCache = default)
    {
        RFile? eax = GetFile(pFileName, skipCache);
        if (eax is null)
        {
            returnValue = pDefault;
            return;
        }
        eax.GetValue(out returnValue, pSection, pName, HashFunctions.HashStringSdbm(pSection, pName), pDefault);
    }

    internal static void GetValue(out int returnValue, string pFileName, string pSection, string pName, int pDefault = 0, bool skipCache = false)
    {
        RFile? eax = GetFile(pFileName, skipCache);
        if (eax is null)
        {
            returnValue = pDefault;
            return;
        }
        eax.GetValue(out returnValue, pSection, pName, HashFunctions.HashStringSdbm(pSection, pName), pDefault);
    }
}
