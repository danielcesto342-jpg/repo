using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Logging;
using ChildrenOfTheGraveEnumNetwork.Content;
using ChildrenOfTheGraveEnumNetwork.Enums;
using INIParser;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

//C:\\Perforce\\LoL\\__RC__\\code\\Eternity\\source\\Riot\\Eternity\\ConfigFile\\File.cpp
namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content.FileSystem;

public class RFile
{
    private readonly ILog _logger = LoggerProvider.GetLogger();

    internal Dictionary<ulong, string> m_SectionAndNameToValue;
    internal string MainFileName;
    internal string DefaultsFileName;
    internal long ModifiedTime;
    internal bool FileHasBeenModified;

    private readonly List<List<ulong>> mHashMaps;
    private readonly List<int> Int32List;
    private readonly List<float> Float32List;
    private readonly List<byte> FixedPointFloatList;
    private readonly List<short> Int16List;
    private readonly List<byte> Int8List;
    private readonly List<bool> BitList;
    private readonly List<ByteListVec3> FixedPointFloatListVec3;
    private readonly List<Vector3> Float32ListVec3;
    private readonly List<ByteListVec2> FixedPointFloatListVec2;
    private readonly List<Vector2> Float32ListVec2;
    private readonly List<ByteListVec4> FixedPointFloatListVec4;
    private readonly List<Vector4> Float32ListVec4;
    private readonly List<ushort> StringOffsetList;
    private readonly List<BinaryINIEntryHeader> entryHeaders;
    private List<char> EntryData;

    public bool BinaryCached { get; private set; }
    public bool BinaryIsDefault { get; private set; }
    public bool TextFileExists { get; private set; }
    string mRelativePath;
    string mRelativeDefaultPath;
    uint mFlags;

    internal RFile(string fileName, string path, string defaultsPath)
    {
        //?
        MainFileName = fileName;
        DefaultsFileName = path;

        FileHasBeenModified = false;
        m_SectionAndNameToValue = [];
        mHashMaps = [];
        Int32List = [];
        Float32List = [];
        FixedPointFloatList = [];
        Int16List = [];
        Int8List = [];
        BitList = [];
        FixedPointFloatListVec3 = [];
        Float32ListVec3 = [];
        FixedPointFloatListVec2 = [];
        Float32ListVec2 = [];
        FixedPointFloatListVec4 = [];
        Float32ListVec4 = [];
        StringOffsetList = [];
        entryHeaders = [];
        EntryData = [];
        //var_4 = 0x12;
        BinaryIsDefault = false;
        TextFileExists = false;

        for (int i = 0; i < (int)TypeGroupIndex.TYPE_INDEX_NumOf; i++)
        {
            mHashMaps.Add([]);
        }

        BinaryIsDefault &= BinaryCached = TryToLoadBinaryFileV2(path);
        ModifiedTime = new FileInfo(MainFileName).LastWriteTime.Ticks;
        TextFileExists = TryToLoadTextFile(path, 1) || TryToLoadTextFile(defaultsPath, 0);

        mRelativePath = defaultsPath;
        mRelativeDefaultPath = path;
    }

    //Unfinished
    //I believe I saw a similar function in the WarZ Eternity engine(League's engine) leak
    //Not sure though
    //Probably better to just use a ini-reading library...
    private bool TryToLoadTextFile(string path, uint flags)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        IniFile INI = new(path);
        foreach (string section in INI.Sections)
        {
            foreach (string key in INI.GetKeys(section))
            {
                string? value = INI[section, key];
                if (value is null)
                {
                    continue;
                }
                uint hash = HashFunctions.HashStringSdbm(section, key);
                m_SectionAndNameToValue[hash] = value.Trim('"');
            }
        }
        return true;
    }

    private bool TryToLoadBinaryFileV2(string relativePatha)
    {
        relativePatha += "bin";
        r3dFileImpl file = r3dFileManager.Instance.Open(relativePatha);
        if (file is null)
        {
            return false;
        }

        byte[] schnoz = RConfigFile.fread(3, 1, file);
        if (schnoz[0] != 2)
        {
            _logger.WarnFormat("ConfigFile: Old version needs to be recompiled: {0}", MainFileName);
            return false;
        }

        short flags = BitConverter.ToInt16(RConfigFile.fread(2, 1, file));
        if ((flags & 1) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int32List], Int32List);
        }
        if ((flags & 2) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32List], Float32List);
        }
        if ((flags & 4) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatList], FixedPointFloatList);
        }
        if ((flags & 8) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int16List], Int16List);
        }
        if ((flags & 0x10) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int8List], Int8List);
        }
        if ((flags & 0x20) != 0)
        {
            RConfigFile.ReadListBitSet(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_BitList], BitList);
        }
        if ((flags & 0x40) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec3], FixedPointFloatListVec3);
        }
        if ((flags & 0x80) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec3], Float32ListVec3);
        }
        if ((flags & 0x100) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec2], FixedPointFloatListVec2);
        }
        if ((flags & 0x200) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec2], Float32ListVec2);
        }
        if ((flags & 0x400) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec4], FixedPointFloatListVec4);
        }
        if ((flags & 0x800) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec4], Float32ListVec4);
        }
        if ((flags & 0x1000) != 0)
        {
            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_StringList], StringOffsetList);
        }

        //int16_t eax_15 = *(uint16_t*)((char*)esp_9 + 0x1d);
        //if (eax_15 >= 0)
        //UNKNOWN: CHECK
        short remainingBytes = (short)(file.stream.Length - file.stream.Position);
        if (remainingBytes >= 0)
        {
            EntryData = [.. RConfigFile.fread(1, (uint)remainingBytes, file).Select(x => (char)x)];
        }
        else
        {
            EntryData = null;
            _logger.WarnFormat("Corrupt Binary header! {0}", relativePatha);
        }

        RConfigFile.fclose(file);
        return true;
        //ConfigFileR.ReadList()
    }

    #region CustomStuff
    [Obsolete("HACK to not break a ton of code")]
    public bool HasMentionOf(string section, string name)
    {
        return GetValue(section, name, null!) is not null;
    }
    public string GetValue(string section, string name, string defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out string returnValue, section, name, hash, defaultValue);
        return returnValue;
    }
    public Vector4 GetValue(string section, string name, Vector4 defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out Vector4 returnValue, section, name, hash, defaultValue);
        return returnValue;
    }
    public Vector3 GetValue(string section, string name, Vector3 defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out Vector3 returnValue, section, name, hash, defaultValue);
        return returnValue;
    }
    public Vector2 GetValue(string section, string name, Vector2 defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out Vector2 returnValue, section, name, hash, defaultValue);
        return returnValue;
    }
    public bool GetValue(string section, string name, bool defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out bool returnValue, section, name, hash, defaultValue);
        return returnValue;
    }
    public float GetValue(string section, string name, float defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out float returnValue, section, name, hash, defaultValue);
        return returnValue;
    }
    public int GetValue(string section, string name, int defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out int returnValue, section, name, hash, defaultValue);
        return returnValue;
    }
    #endregion

    public void GetValue(string section, string name, out string returnValue, string defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out returnValue, section, name, hash, defaultValue);
    }
    public void GetValue(string section, string name, out Vector4 returnValue, Vector4 defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out returnValue, section, name, hash, defaultValue);
    }
    public void GetValue(string section, string name, out Vector3 returnValue, Vector3 defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out returnValue, section, name, hash, defaultValue);
    }
    public void GetValue(string section, string name, out Vector2 returnValue, Vector2 defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out returnValue, section, name, hash, defaultValue);
    }
    public void GetValue(string section, string name, out bool returnValue, bool defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out returnValue, section, name, hash, defaultValue);
    }
    public void GetValue(string section, string name, out float returnValue, float defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out returnValue, section, name, hash, defaultValue);
    }
    public void GetValue(string section, string name, out int returnValue, int defaultValue)
    {
        uint hash = HashFunctions.HashStringSdbm(section, name);
        GetValue(out returnValue, section, name, hash, defaultValue);
    }

    internal void GetValue(out string returnValue, string section, string name, uint hashedSectionAndName, string defaultValue)
    {
        if (TextFileExists && GetValueFromText(section, name, out returnValue, false))
        {
            return;
        }

        if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
        {
            returnValue = defaultValue;
        }
    }
    internal bool GetValueFromText(string section, string name, out string returnValue, bool skipCache)
    {
        if (m_SectionAndNameToValue.TryGetValue(HashFunctions.HashStringSdbm(section, name), out returnValue!))
        {
            return true;
        }
        return false;
    }
    internal void GetValue(out Vector4 returnValue, string section, string name, uint hashedSectionAndName, Vector4 defaultValue)
    {
        if (TextFileExists && GetValueFromText(section, name, out string strVal, false))
        {
            string[] parts = strVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length is 4 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z) &&
                float.TryParse(parts[3], out float w))
            {
                returnValue = new Vector4(x, y, z, w);
                return;
            }
            _logger.ErrorFormat("Error parsing Vector2 value from text file!: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
        }

        if (!BinaryCached || !LookupRawFloat4(hashedSectionAndName, out returnValue))
        {
            returnValue = defaultValue;
        }
    }
    internal void GetValue(out Vector3 returnValue, string section, string name, uint hashedSectionAndName, Vector3 defaultValue)
    {
        if (TextFileExists && GetValueFromText(section, name, out string strVal, false))
        {
            string[] parts = strVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length is 3 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
            {
                returnValue = new Vector3(x, y, z);
                return;
            }
            _logger.ErrorFormat("Error parsing Vector2 value from text file!: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
        }

        if (!BinaryCached || !LookupRawFloat3(hashedSectionAndName, out returnValue))
        {
            returnValue = defaultValue;
        }
    }

    internal void GetValue(out Vector2 returnValue, string section, string name, uint hashedSectionAndName, Vector2 defaultValue)
    {
        if (TextFileExists && GetValueFromText(section, name, out string strVal, false))
        {
            string[] parts = strVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length is 3 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y))
            {
                returnValue = new Vector2(x, y);
                return;
            }
            _logger.ErrorFormat("Error parsing Vector2 value from text file!: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
        }

        if (!BinaryCached || !LookupRawFloat2(hashedSectionAndName, out returnValue))
        {
            returnValue = defaultValue;
        }
    }
    internal void GetValue(out bool returnValue, string section, string name, uint hashedSectionAndName, bool defaultValue)
    {
        if (TextFileExists && GetValueFromText(section, name, out string strVal, false))
        {
            returnValue = strVal is "1" or "true" or "yes" or "on";
            return;
        }

        if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
        {
            returnValue = defaultValue;
        }
    }
    internal void GetValue(out float returnValue, string section, string name, uint hashedSectionAndName, float defaultValue)
    {
        if (TextFileExists && GetValueFromText(section, name, out string strVal, false))
        {
            if (float.TryParse(strVal, out float val))
            {
                returnValue = val;
                return;
            }
            _logger.ErrorFormat("Error parsing float value from text file: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
        }

        if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
        {
            returnValue = defaultValue;
        }
    }
    internal void GetValue(out int returnValue, string section, string name, uint hashedSectionAndName, int defaultValue)
    {
        if (TextFileExists && GetValueFromText(section, name, out string strVal, false))
        {
            if (int.TryParse(strVal, out int val))
            {
                returnValue = val;
                return;
            }
            _logger.ErrorFormat("Error parsing int value from text file: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
        }

        if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
        {
            returnValue = defaultValue;
            return;
        }
    }

    private bool GetValueFromBinary(uint hash, out float returnValue)
    {
        if (LookupRawFloat(hash, out returnValue))
        {
            return true;
        }
        if (LookupRawInt(hash, out int intVal))
        {
            returnValue = intVal;
            return true;
        }
        if (LookupRawBool(hash, out bool boolVal))
        {
            returnValue = boolVal ? 1 : 0; //0x3f800000 : 0 ???
            return true;
        }
        if (LookupString(hash, out string strVal))
        {
            if (float.TryParse(strVal, out returnValue))
            {
                return true;
            }
        }
        returnValue = 0;
        return false;
    }

    private bool GetValueFromBinary(uint hash, out Vector4 returnValue)
    {
        //This function does't see to exist in the original code
        throw new NotImplementedException();
    }

    private bool GetValueFromBinary(uint hash, out Vector3 returnValue)
    {
        if (LookupRawFloat3(hash, out returnValue))
        {
            return true;
        }
        if (LookupString(hash, out string strVal))
        {
            string[] split = strVal.Split(" ");
            if (split.Length is 3)
            {
                if (float.TryParse(split[0], out float x) && float.TryParse(split[1], out float y) && float.TryParse(split[2], out float z))
                {
                    returnValue = new Vector3(x, y, z);
                    return true;
                }
            }
        }
        returnValue = Vector3.Zero;
        return false;
    }
    internal bool GetValueFromBinary(uint hash, out int returnValue)
    {
        if (LookupRawInt(hash, out returnValue))
        {
            return true;
        }
        if (LookupRawFloat(hash, out float floatVal))
        {
            returnValue = (int)floatVal;
            return true;
        }
        if (LookupRawBool(hash, out bool boolVal))
        {
            returnValue = boolVal ? 1 : 0;
            return true;
        }
        if (LookupString(hash, out string strVal))
        {
            if (float.TryParse(strVal, out float f))
            {
                returnValue = (int)f;
                return true;
            }
        }
        returnValue = 0;
        return false;
    }
    internal bool GetValueFromBinary(uint hash, out bool returnValue)
    {
        if (LookupRawBool(hash, out returnValue))
        {
            return true;
        }
        if (LookupString(hash, out string strVal))
        {
            returnValue = strVal.FirstOrDefault() is '1';
        }
        returnValue = false;
        return false;
    }
    internal bool GetValueFromBinary(uint hash, out Vector2 returnValue)
    {
        if (LookupRawFloat2(hash, out returnValue))
        {
            return true;
        }
        if (LookupString(hash, out string strVal))
        {
            string[] split = strVal.Split(" ");
            if (split.Length is 2)
            {
                if (float.TryParse(split.First(), out float x) && float.TryParse(split.Last(), out float y))
                {
                    returnValue = new Vector2(x, y);
                    return true;
                }
            }
        }
        returnValue = Vector2.Zero;
        return false;
    }
    internal bool GetValueFromBinary(uint hash, out string returnValue)
    {
        if (LookupString(hash, out returnValue))
        {
            return true;
        }
        if (LookupRawInt(hash, out int intVal))
        {
            returnValue = $"{intVal}";
            return true;
        }
        if (LookupRawFloat(hash, out float floatVal))
        {
            returnValue = $"{floatVal}";
            return true;
        }
        if (LookupRawBool(hash, out bool boolVal))
        {
            returnValue = boolVal ? "1" : "0";
            return true;
        }
        if (LookupRawFloat2(hash, out Vector2 vec2Val))
        {
            returnValue = $"{vec2Val.X} {vec2Val.Y}";
            return true;
        }
        if (LookupRawFloat3(hash, out Vector3 vec3Val))
        {
            returnValue = $"{vec3Val.X} {vec3Val.Y} {vec3Val.Z}";
            return true;
        }
        if (LookupRawFloat4(hash, out Vector4 vec4Val))
        {
            returnValue = $"{vec4Val.X} {vec4Val.Y} {vec4Val.Z} {vec4Val.W}";
            return true;
        }
        returnValue = "";
        return false;
    }
    internal bool LookupString(uint hash, out string returnValue)
    {
        if (!Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_StringList], StringOffsetList, out ushort offset))
        {
            if (m_SectionAndNameToValue.TryGetValue(hash, out returnValue!)) //TODO: Check this
            {
                return true;
            }
            returnValue = string.Empty;
            return false;
        }

        List<char> buffer = EntryData[offset..];
        returnValue = new string([.. buffer[..buffer.IndexOf('\0')]]); //Check
        return true;
    }
    internal bool LookupRawBool(uint hash, out bool returnValue)
    {
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_BitList], BitList, out returnValue))
        {
            return true;
        }

        returnValue = false;
        return false;
    }
    internal bool LookupRawInt(uint hash, out int returnValue)
    {
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int32List], Int32List, out returnValue))
        {
            return true;
        }
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int16List], Int16List, out short shortVal))
        {
            returnValue = shortVal;
            return true;
        }
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int8List], Int8List, out byte byteVal))
        {
            returnValue = byteVal;
            return true;
        }
        returnValue = 0;
        return false;
    }
    internal bool LookupRawFloat(uint hash, out float returnValue)
    {
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32List], Float32List, out returnValue))
        {
            return true;
        }
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatList], FixedPointFloatList, out byte b))
        {
            returnValue = b / 10.0f;
            return true;
        }
        returnValue = 0;
        return false;
    }
    internal bool LookupRawFloat2(uint hash, out Vector2 returnValue)
    {
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec2], Float32ListVec2, out returnValue))
        {
            return true;
        }
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec2], FixedPointFloatListVec2, out ByteListVec2 b))
        {
            returnValue = new Vector2(b.x / 10.0f, b.y / 10.0f);
            return true;
        }
        returnValue = Vector2.Zero;
        return false;
    }
    internal bool LookupRawFloat3(uint hash, out Vector3 returnValue)
    {
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec3], Float32ListVec3, out returnValue))
        {
            return true;
        }
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec3], FixedPointFloatListVec3, out ByteListVec3 b))
        {
            returnValue = new Vector3(b.x / 10.0f, b.y / 10.0f, b.z / 10.0f);
            return true;
        }
        returnValue = Vector3.Zero;
        return false;
    }
    internal bool LookupRawFloat4(uint hash, out Vector4 returnValue)
    {
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec4], Float32ListVec4, out returnValue))
        {
            return true;
        }
        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec4], FixedPointFloatListVec4, out ByteListVec4 b))
        {
            returnValue = new Vector4(b.x / 10.0f, b.y / 10.0f, b.z / 10.0f, b.w / 10.0f);
            return true;
        }
        returnValue = Vector4.Zero;
        return false;
    }
    internal bool Lookup<T>(uint hash, List<ulong> hashMap, List<T> list, out T returnValue)
    {
        returnValue = default!;

        int index = hashMap.IndexOf(hash);
        if (index is -1)
        {
            return false;
        }

        returnValue = list[index];
        return true;
    }

    internal struct ByteListVec2
    {
        internal byte x;
        internal byte y;
    }
    internal struct ByteListVec3
    {
        internal byte x;
        internal byte y;
        internal byte z;
    }
    internal struct ByteListVec4
    {
        internal byte x;
        internal byte y;
        internal byte z;
        internal byte w;
    }
    internal struct BinaryINIEntryHeader
    {
        uint hashKey;
        uint offset;
    };
    internal struct BinaryINIHeaderV2
    {
        byte version;
        //__offset(0x1);
        short charEntries;
        //__padding char _3[1];
    };
}

//For the purposes of the project, I changed a couple behaviors regarding text files being prioritized over binary ones
#region Original/Correct Implementation
//public class RFile
//{
//    private readonly ILog _logger = LoggerProvider.GetLogger();

//    internal Dictionary<ulong, string> m_SectionAndNameToValue;
//    internal string MainFileName;
//    internal string DefaultsFileName;
//    internal long ModifiedTime;
//    internal bool FileHasBeenModified;

//    private readonly List<List<ulong>> mHashMaps;
//    private readonly List<int> Int32List;
//    private readonly List<float> Float32List;
//    private readonly List<byte> FixedPointFloatList;
//    private readonly List<short> Int16List;
//    private readonly List<byte> Int8List;
//    private readonly List<bool> BitList;
//    private readonly List<ByteListVec3> FixedPointFloatListVec3;
//    private readonly List<Vector3> Float32ListVec3;
//    private readonly List<ByteListVec2> FixedPointFloatListVec2;
//    private readonly List<Vector2> Float32ListVec2;
//    private readonly List<ByteListVec4> FixedPointFloatListVec4;
//    private readonly List<Vector4> Float32ListVec4;
//    private readonly List<ushort> StringOffsetList;
//    private readonly List<BinaryINIEntryHeader> entryHeaders;
//    private List<char> EntryData;

//    public bool BinaryCached { get; private set; }
//    public bool BinaryIsDefault { get; private set; }
//    public bool TextFileExists { get; private set; }
//    string mRelativePath;
//    string mRelativeDefaultPath;
//    uint mFlags;

//    //Too lazy to properly RE the ini text reading
//    private IniFile? INI;

//    internal RFile(string fileName, string path, string? defaultsPath)
//    {
//        //?
//        MainFileName = fileName;
//        DefaultsFileName = path;

//        FileHasBeenModified = false;
//        m_SectionAndNameToValue = [];
//        mHashMaps = [];
//        Int32List = [];
//        Float32List = [];
//        FixedPointFloatList = [];
//        Int16List = [];
//        Int8List = [];
//        BitList = [];
//        FixedPointFloatListVec3 = [];
//        Float32ListVec3 = [];
//        FixedPointFloatListVec2 = [];
//        Float32ListVec2 = [];
//        FixedPointFloatListVec4 = [];
//        Float32ListVec4 = [];
//        StringOffsetList = [];
//        entryHeaders = [];
//        EntryData = [];
//        //var_4 = 0x12;
//        BinaryIsDefault = false;
//        TextFileExists = false;

//        for (int i = 0; i < (int)TypeGroupIndex.TYPE_INDEX_NumOf; i++)
//        {
//            mHashMaps.Add([]);
//        }

//        bool binaryFileV2 = TryToLoadBinaryFileV2(defaultsPath);
//        BinaryCached = binaryFileV2;
//        if (!binaryFileV2)
//        {
//            bool v10 = TryToLoadBinaryFileV2(path);
//            BinaryIsDefault = v10;
//            BinaryCached = v10;

//            ModifiedTime = new FileInfo(MainFileName).LastWriteTime.Ticks;

//            bool textFile = TryToLoadTextFile(defaultsPath, 0);
//            bool v14 = TryToLoadTextFile(path, 1);
//            TextFileExists = textFile || v14;
//            mRelativePath = defaultsPath;
//            mRelativeDefaultPath = path;
//        }
//    }

//    //Unfinished
//    //I believe I saw a similar function in the WarZ Eternity engine(League's engine) leak
//    //Not sure though
//    //Probably better to just use a ini-reading library...
//    private bool TryToLoadTextFile(string path, uint flags)
//    {
//        if (!File.Exists(path))
//        {
//            return false;
//        }

//        INI = new IniFile(path);
//        return true;
//    }

//    private bool TryToLoadBinaryFileV2(string relativePatha)
//    {
//        relativePatha += "bin";
//        r3dFileImpl file = r3dFileManager.Instance.Open(relativePatha);
//        if (file is null)
//        {
//            return false;
//        }

//        byte[] schnoz = RConfigFile.fread(3, 1, file);
//        if (schnoz[0] != 2)
//        {
//            _logger.WarnFormat("ConfigFile: Old version needs to be recompiled: {0}", MainFileName);
//            return false;
//        }

//        short flags = BitConverter.ToInt16(RConfigFile.fread(2, 1, file));
//        if ((flags & 1) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int32List], Int32List);
//        }
//        if ((flags & 2) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32List], Float32List);
//        }
//        if ((flags & 4) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatList], FixedPointFloatList);
//        }
//        if ((flags & 8) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int16List], Int16List);
//        }
//        if ((flags & 0x10) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int8List], Int8List);
//        }
//        if ((flags & 0x20) != 0)
//        {
//            RConfigFile.ReadListBitSet(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_BitList], BitList);
//        }
//        if ((flags & 0x40) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec3], FixedPointFloatListVec3);
//        }
//        if ((flags & 0x80) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec3], Float32ListVec3);
//        }
//        if ((flags & 0x100) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec2], FixedPointFloatListVec2);
//        }
//        if ((flags & 0x200) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec2], Float32ListVec2);
//        }
//        if ((flags & 0x400) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec4], FixedPointFloatListVec4);
//        }
//        if ((flags & 0x800) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec4], Float32ListVec4);
//        }
//        if ((flags & 0x1000) != 0)
//        {
//            RConfigFile.ReadList(file, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_StringList], StringOffsetList);
//        }

//        //int16_t eax_15 = *(uint16_t*)((char*)esp_9 + 0x1d);
//        //if (eax_15 >= 0)
//        //UNKNOWN: CHECK
//        short remainingBytes = (short)(file.stream.Length - file.stream.Position);
//        if (remainingBytes >= 0)
//        {
//            EntryData = [.. RConfigFile.fread(1, (uint)remainingBytes, file).Select(x => (char)x)];
//        }
//        else
//        {
//            EntryData = null;
//            _logger.WarnFormat("Corrupt Binary header! {0}", relativePatha);
//        }

//        RConfigFile.fclose(file);
//        return true;
//        //ConfigFileR.ReadList()
//    }

//    #region CustomStuff
//    [Obsolete("HACK to not break a ton of code")]
//    public bool HasMentionOf(string section, string name)
//    {
//        return GetValue(section, name, null!) is not null;
//    }
//    public string GetValue(string section, string name, string defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out string returnValue, section, name, hash, defaultValue);
//        return returnValue;
//    }
//    public Vector4 GetValue(string section, string name, Vector4 defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out Vector4 returnValue, section, name, hash, defaultValue);
//        return returnValue;
//    }
//    public Vector3 GetValue(string section, string name, Vector3 defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out Vector3 returnValue, section, name, hash, defaultValue);
//        return returnValue;
//    }
//    public Vector2 GetValue(string section, string name, Vector2 defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out Vector2 returnValue, section, name, hash, defaultValue);
//        return returnValue;
//    }
//    public bool GetValue(string section, string name, bool defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out bool returnValue, section, name, hash, defaultValue);
//        return returnValue;
//    }
//    public float GetValue(string section, string name, float defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out float returnValue, section, name, hash, defaultValue);
//        return returnValue;
//    }
//    public int GetValue(string section, string name, int defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out int returnValue, section, name, hash, defaultValue);
//        return returnValue;
//    }
//    #endregion

//    public void GetValue(string section, string name, out string returnValue, string defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out returnValue, section, name, hash, defaultValue);
//    }
//    public void GetValue(string section, string name, out Vector4 returnValue, Vector4 defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out returnValue, section, name, hash, defaultValue);
//    }
//    public void GetValue(string section, string name, out Vector3 returnValue, Vector3 defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out returnValue, section, name, hash, defaultValue);
//    }
//    public void GetValue(string section, string name, out Vector2 returnValue, Vector2 defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out returnValue, section, name, hash, defaultValue);
//    }
//    public void GetValue(string section, string name, out bool returnValue, bool defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out returnValue, section, name, hash, defaultValue);
//    }
//    public void GetValue(string section, string name, out float returnValue, float defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out returnValue, section, name, hash, defaultValue);
//    }
//    public void GetValue(string section, string name, out int returnValue, int defaultValue)
//    {
//        uint hash = HashFunctions.HashStringSdbm(section, name);
//        GetValue(out returnValue, section, name, hash, defaultValue);
//    }

//    internal void GetValue(out string returnValue, string section, string name, uint hashedSectionAndName, string defaultValue)
//    {
//        if (!TextFileExists)
//        {
//            if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
//            {
//                returnValue = defaultValue;
//                return;
//            }
//            return;
//        }

//        if (BinaryIsDefault && GetValueFromBinary(hashedSectionAndName, out returnValue))
//        {
//            return;
//        }

//        GetValueFromText(section, name, defaultValue, out returnValue, false);
//        return;
//    }
//    internal void GetValueFromText(string section, string name, string defaultVal, out string returnValue, bool skipCache)
//    {
//        if (INI is not null)
//        {
//            returnValue = INI[section, name]?.Trim('"') ?? defaultVal;
//            return;
//        }
//        returnValue = defaultVal;
//    }
//    internal void GetValue(out Vector4 returnValue, string section, string name, uint hashedSectionAndName, Vector4 defaultValue)
//    {
//        if (!TextFileExists)
//        {
//            if (!BinaryCached || !LookupRawFloat4(hashedSectionAndName, out returnValue))
//            {
//                returnValue = defaultValue;
//                return;
//            }
//            return;
//        }

//        if (BinaryIsDefault && LookupRawFloat4(hashedSectionAndName, out returnValue))
//        {
//            return;
//        }
//        GetValueFromText(section, name, null, out string strVal, false);
//        if (strVal != null)
//        {
//            string[] parts = strVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);
//            if (parts.Length is 4 &&
//                float.TryParse(parts[0], out float x) &&
//                float.TryParse(parts[1], out float y) &&
//                float.TryParse(parts[2], out float z) &&
//                float.TryParse(parts[3], out float w))
//            {
//                returnValue = new Vector4(x, y, z, w);
//                return;
//            }
//            _logger.ErrorFormat("Error parsing Vector2 value from text file!: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
//        }
//        returnValue = defaultValue;
//    }
//    internal void GetValue(out Vector3 returnValue, string section, string name, uint hashedSectionAndName, Vector3 defaultValue)
//    {
//        if (!TextFileExists)
//        {
//            if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
//            {
//                returnValue = defaultValue;
//                return;
//            }
//            return;
//        }

//        if (BinaryIsDefault && GetValueFromBinary(hashedSectionAndName, out returnValue))
//        {
//            return;
//        }
//        GetValueFromText(section, name, null!, out string strVal, false);
//        if (strVal is not null)
//        {
//            string[] parts = strVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);
//            if (parts.Length is 3 &&
//                float.TryParse(parts[0], out float x) &&
//                float.TryParse(parts[1], out float y) &&
//                float.TryParse(parts[2], out float z))
//            {
//                returnValue = new Vector3(x, y, z);
//                return;
//            }
//            _logger.ErrorFormat("Error parsing Vector2 value from text file!: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
//        }
//        returnValue = defaultValue;
//    }
//    internal void GetValue(out Vector2 returnValue, string section, string name, uint hashedSectionAndName, Vector2 defaultValue)
//    {
//        if (!TextFileExists)
//        {
//            if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
//            {
//                returnValue = defaultValue;
//                return;
//            }
//            return;
//        }

//        if (BinaryIsDefault && GetValueFromBinary(hashedSectionAndName, out returnValue))
//        {
//            return;
//        }
//        GetValueFromText(section, name, null!, out string strVal, false);
//        if (strVal is not null)
//        {
//            string[] parts = strVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);
//            if (parts.Length is 2 &&
//                float.TryParse(parts[0], out float x) &&
//                float.TryParse(parts[1], out float y))
//            {
//                returnValue = new Vector2(x, y);
//                return;
//            }
//            _logger.ErrorFormat("Error parsing Vector2 value from text file: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
//        }
//        returnValue = defaultValue;
//    }
//    internal void GetValue(out bool returnValue, string section, string name, uint hashedSectionAndName, bool defaultValue)
//    {
//        if (!TextFileExists)
//        {
//            if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
//            {
//                returnValue = defaultValue;
//                return;
//            }
//            return;
//        }

//        if (BinaryIsDefault && GetValueFromBinary(hashedSectionAndName, out returnValue))
//        {
//            return;
//        }
//        GetValueFromText(section, name, null!, out string strVal, false);
//        if (strVal is not null)
//        {
//            returnValue = strVal is "1" or "true" or "yes" or "on";
//            return;
//        }
//        returnValue = defaultValue;
//    }
//    internal void GetValue(out float returnValue, string section, string name, uint hashedSectionAndName, float defaultValue)
//    {
//        if (!TextFileExists)
//        {
//            if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
//            {
//                returnValue = defaultValue;
//                return;
//            }
//            return;
//        }

//        if (BinaryIsDefault && GetValueFromBinary(hashedSectionAndName, out returnValue))
//        {
//            return;
//        }
//        GetValueFromText(section, name, null!, out string strVal, false);
//        if (strVal is not null)
//        {
//            if(!float.TryParse(strVal, out float val))
//            {
//                _logger.ErrorFormat("Error parsing float value from text file: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
//                returnValue = defaultValue;
//            }
//            returnValue = val;
//            return;
//        }
//        returnValue = defaultValue;
//    }
//    internal void GetValue(out int returnValue, string section, string name, uint hashedSectionAndName, int defaultValue)
//    {
//        if (!TextFileExists)
//        {
//            if (!BinaryCached || !GetValueFromBinary(hashedSectionAndName, out returnValue))
//            {
//                returnValue = defaultValue;
//                return;
//            }
//            return;
//        }

//        if (BinaryIsDefault && GetValueFromBinary(hashedSectionAndName, out returnValue))
//        {
//            return;
//        }
//        GetValueFromText(section, name, null!, out string strVal, false);
//        if (strVal is not null)
//        {
//            if (!int.TryParse(strVal, out int val))
//            {
//                _logger.ErrorFormat("Error parsing float value from text file: [{0}][{1}][{2}] = {3}", MainFileName, section, name, strVal);
//                returnValue = defaultValue;
//            }
//            returnValue = val;
//            return;
//        }
//        returnValue = defaultValue;
//    }

//    private bool GetValueFromBinary(uint hash, out float returnValue)
//    {
//        if (LookupRawFloat(hash, out returnValue))
//        {
//            return true;
//        }
//        if (LookupRawInt(hash, out int intVal))
//        {
//            returnValue = intVal;
//            return true;
//        }
//        if (LookupRawBool(hash, out bool boolVal))
//        {
//            returnValue = boolVal ? 1 : 0; //0x3f800000 : 0 ???
//            return true;
//        }
//        if (LookupString(hash, out string strVal))
//        {
//            if (float.TryParse(strVal, out returnValue))
//            {
//                return true;
//            }
//        }
//        returnValue = 0;
//        return false;
//    }

//    private bool GetValueFromBinary(uint hash, out Vector4 returnValue)
//    {
//        //This function does't see to exist in the original code
//        throw new NotImplementedException();
//    }

//    private bool GetValueFromBinary(uint hash, out Vector3 returnValue)
//    {
//        if (LookupRawFloat3(hash, out returnValue))
//        {
//            return true;
//        }
//        if (LookupString(hash, out string strVal))
//        {
//            string[] split = strVal.Split(" ");
//            if (split.Length is 3)
//            {
//                if (float.TryParse(split[0], out float x) && float.TryParse(split[1], out float y) && float.TryParse(split[2], out float z))
//                {
//                    returnValue = new Vector3(x, y, z);
//                    return true;
//                }
//            }
//        }
//        returnValue = Vector3.Zero;
//        return false;
//    }
//    internal bool GetValueFromBinary(uint hash, out int returnValue)
//    {
//        if (LookupRawInt(hash, out returnValue))
//        {
//            return true;
//        }
//        if (LookupRawFloat(hash, out float floatVal))
//        {
//            returnValue = (int)floatVal;
//            return true;
//        }
//        if (LookupRawBool(hash, out bool boolVal))
//        {
//            returnValue = boolVal ? 1 : 0;
//            return true;
//        }
//        if (LookupString(hash, out string strVal))
//        {
//            if (float.TryParse(strVal, out float f))
//            {
//                returnValue = (int)f;
//                return true;
//            }
//        }
//        returnValue = 0;
//        return false;
//    }
//    internal bool GetValueFromBinary(uint hash, out bool returnValue)
//    {
//        if (LookupRawBool(hash, out returnValue))
//        {
//            return true;
//        }
//        if (LookupString(hash, out string strVal))
//        {
//            returnValue = strVal.FirstOrDefault() is '1';
//        }
//        returnValue = false;
//        return false;
//    }
//    internal bool GetValueFromBinary(uint hash, out Vector2 returnValue)
//    {
//        if (LookupRawFloat2(hash, out returnValue))
//        {
//            return true;
//        }
//        if (LookupString(hash, out string strVal))
//        {
//            string[] split = strVal.Split(" ");
//            if (split.Length is 2)
//            {
//                if (float.TryParse(split.First(), out float x) && float.TryParse(split.Last(), out float y))
//                {
//                    returnValue = new Vector2(x, y);
//                    return true;
//                }
//            }
//        }
//        returnValue = Vector2.Zero;
//        return false;
//    }
//    internal bool GetValueFromBinary(uint hash, out string returnValue)
//    {
//        if (LookupString(hash, out returnValue))
//        {
//            return true;
//        }
//        if (LookupRawInt(hash, out int intVal))
//        {
//            returnValue = $"{intVal}";
//            return true;
//        }
//        if (LookupRawFloat(hash, out float floatVal))
//        {
//            returnValue = $"{floatVal}";
//            return true;
//        }
//        if (LookupRawBool(hash, out bool boolVal))
//        {
//            returnValue = boolVal ? "1" : "0";
//            return true;
//        }
//        if (LookupRawFloat2(hash, out Vector2 vec2Val))
//        {
//            returnValue = $"{vec2Val.X} {vec2Val.Y}";
//            return true;
//        }
//        if (LookupRawFloat3(hash, out Vector3 vec3Val))
//        {
//            returnValue = $"{vec3Val.X} {vec3Val.Y} {vec3Val.Z}";
//            return true;
//        }
//        if (LookupRawFloat4(hash, out Vector4 vec4Val))
//        {
//            returnValue = $"{vec4Val.X} {vec4Val.Y} {vec4Val.Z} {vec4Val.W}";
//            return true;
//        }
//        returnValue = "";
//        return false;
//    }
//    internal bool LookupString(uint hash, out string returnValue)
//    {
//        if (!Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_StringList], StringOffsetList, out ushort offset))
//        {
//            if (m_SectionAndNameToValue.TryGetValue(hash, out returnValue!)) //TODO: Check this
//            {
//                return true;
//            }
//            returnValue = string.Empty;
//            return false;
//        }

//        List<char> buffer = EntryData[offset..];
//        returnValue = new string([.. buffer[..buffer.IndexOf('\0')]]); //Check
//        return true;
//    }
//    internal bool LookupRawBool(uint hash, out bool returnValue)
//    {
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_BitList], BitList, out returnValue))
//        {
//            return true;
//        }

//        returnValue = false;
//        return false;
//    }
//    internal bool LookupRawInt(uint hash, out int returnValue)
//    {
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int32List], Int32List, out returnValue))
//        {
//            return true;
//        }
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int16List], Int16List, out short shortVal))
//        {
//            returnValue = shortVal;
//            return true;
//        }
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Int8List], Int8List, out byte byteVal))
//        {
//            returnValue = byteVal;
//            return true;
//        }
//        returnValue = 0;
//        return false;
//    }
//    internal bool LookupRawFloat(uint hash, out float returnValue)
//    {
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32List], Float32List, out returnValue))
//        {
//            return true;
//        }
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatList], FixedPointFloatList, out byte b))
//        {
//            returnValue = b / 10.0f;
//            return true;
//        }
//        returnValue = 0;
//        return false;
//    }
//    internal bool LookupRawFloat2(uint hash, out Vector2 returnValue)
//    {
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec2], Float32ListVec2, out returnValue))
//        {
//            return true;
//        }
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec2], FixedPointFloatListVec2, out ByteListVec2 b))
//        {
//            returnValue = new Vector2(b.x / 10.0f, b.y / 10.0f);
//            return true;
//        }
//        returnValue = Vector2.Zero;
//        return false;
//    }
//    internal bool LookupRawFloat3(uint hash, out Vector3 returnValue)
//    {
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec3], Float32ListVec3, out returnValue))
//        {
//            return true;
//        }
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec3], FixedPointFloatListVec3, out ByteListVec3 b))
//        {
//            returnValue = new Vector3(b.x / 10.0f, b.y / 10.0f, b.z / 10.0f);
//            return true;
//        }
//        returnValue = Vector3.Zero;
//        return false;
//    }
//    internal bool LookupRawFloat4(uint hash, out Vector4 returnValue)
//    {
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_Float32ListVec4], Float32ListVec4, out returnValue))
//        {
//            return true;
//        }
//        if (Lookup(hash, mHashMaps[(int)TypeGroupIndex.TYPE_INDEX_FixedPointFloatListVec4], FixedPointFloatListVec4, out ByteListVec4 b))
//        {
//            returnValue = new Vector4(b.x / 10.0f, b.y / 10.0f, b.z / 10.0f, b.w / 10.0f);
//            return true;
//        }
//        returnValue = Vector4.Zero;
//        return false;
//    }
//    internal bool Lookup<T>(uint hash, List<ulong> hashMap, List<T> list, out T returnValue)
//    {
//        returnValue = default!;

//        int index = hashMap.IndexOf(hash);
//        if (index is -1)
//        {
//            return false;
//        }

//        returnValue = list[index];
//        return true;
//    }

//    internal struct ByteListVec2
//    {
//        internal byte x;
//        internal byte y;
//    }
//    internal struct ByteListVec3
//    {
//        internal byte x;
//        internal byte y;
//        internal byte z;
//    }
//    internal struct ByteListVec4
//    {
//        internal byte x;
//        internal byte y;
//        internal byte z;
//        internal byte w;
//    }
//    internal struct BinaryINIEntryHeader
//    {
//        uint hashKey;
//        uint offset;
//    };
//    internal struct BinaryINIHeaderV2
//    {
//        byte version;
//        //__offset(0x1);
//        short charEntries;
//        //__padding char _3[1];
//    };
//}
#endregion