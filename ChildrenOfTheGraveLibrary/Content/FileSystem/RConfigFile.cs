using ChildrenOfTheGraveEnumNetwork.Content;
using ChildrenOfTheGraveLibrary.Content.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content.FileSystem;

internal static class RConfigFile
{
    internal static void ReadList<T>(r3dFile arg2, List<ulong> hashMap, List<T> values) where T : struct
    {
        if (arg2 is not r3dFileImpl file)
        {
            return;
        }

        short count = BitConverter.ToInt16(fread(2, 1, file));
        byte[] data;

        for (int i = 0; i < count; i++)
        {
            data = fread(4, 1, file);
            if (data.Length != 4)
            {
                Array.Resize(ref data, 4);
            }
            hashMap.Add(BitConverter.ToUInt32(data));
        }

        int typeSize = Marshal.SizeOf(typeof(T));
        for (int i = 0; i < count; i++)
        {
            data = fread((uint)typeSize, 1, file);
            values.Add(Unsafe.As<byte, T>(ref data[0]));
        }
    }

    internal static void ReadListBitSet(r3dFile arg2, List<ulong> hashMap, List<bool> values)
    {
        if (arg2 is not r3dFileImpl file)
        {
            return;
        }

        short count = BitConverter.ToInt16(fread(2, 1, file));
        int totalBytes = (int)MathF.Ceiling(count / 8f);

        for (int i = 0; i < count; i++)
        {
            byte[] data = fread(4, 1, file);
            ulong hash = BitConverter.ToUInt32(data);
            hashMap.Add(hash);
        }

        int totalBitsRead = 0;
        for (int i = 0; i < totalBytes; i++)
        {
            byte b = fread(1, 1, file)[0];
            for (byte bitCount = 0; bitCount < 8 && totalBitsRead < count; bitCount++, totalBitsRead++)
            {
                byte mask = (byte)(1 << bitCount);
                values.Add((b & mask) != 0);
            }
        }
    }

    //uint32_t fread(void* ptr, uint32_t size, uint32_t n, class r3dFileImpl* f)
    internal static byte[] fread(uint size, uint n, r3dFileImpl f)
    {
        if (f.stream != null)
        {
            using BinaryReader reader = new(f.stream, Encoding.ASCII, true);
            byte[] bytes = reader.ReadBytes((int)(size * n));
            return bytes;
        }

        int size_1 = f.size;
        int totalSize = (int)(size * n);
        int pos = f.pos;

        if (pos + totalSize >= size_1)
        {
            totalSize = size_1 - pos;
        }

        if (totalSize != 0)
        {
            return f.data[pos..(pos + totalSize)];
        }

        return [];
    }

    //Check
    internal static void fclose(r3dFile f)
    {
        if (f is r3dFileImpl impl)
        {
            impl.Dispose();
            return;
        }
        else
        {
            (f as r3dFileRADS)!.Close();
        }
    }

    internal static Vector3 r3dReadCFG_V(string fileName, string section, string name, Vector3 defaultValue)
    {
        RFile? file = Cache.GetFile(fileName);
        if (file is null)
        {
            return defaultValue;
        }
        uint hash = HashFunctions.HashStringSdbm(section, name);
        file.GetValue(out Vector3 value, section, name, hash, defaultValue);
        return value;
    }

    internal static bool r3dWriteCFG_V(string fileName, string group, string name, Vector3 value)
    {
        string toWrite = $"{value.X:F4} {value.Y:F4} {value.Z:F4}";
        return r3dWriteCFG_S(fileName, group, name, toWrite);
    }

    internal static bool r3dWriteCFG_S(string FileName, string group, string name, string str)
    {
        //TODO
        throw new NotImplementedException();
    }
}
