using ChildrenOfTheGraveLibrary.Content.Interfaces;
using System;
using System.IO;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content.FileSystem;

internal class r3dFileImpl : r3dFile, IDisposable
{
    //struct r3dFileImpl::VTable* vtable;
    internal r3dFileLoc Location;
    internal Stream stream;
    internal bool bFileOwned; //int_32
    internal byte[] data;
    internal int pos;
    internal int size;

    internal r3dFileImpl(int size)
    {
        stream = null;
        data = null;
        bFileOwned = false;
        pos = 0;
        Location.FileName = "";
        Location.Where = 0;
        Location.Offset = 0;
        this.size = size;
        data = new byte[size];
        pos = 0;
        stream = null;
    }

    internal void r3dFileImpl_Init()
    {
        stream = null;
        data = null;
        bFileOwned = false;
        size = 0;
        pos = 0;
        Location.FileName = "";
        Location.Where = 0;
        Location.Offset = 0;
    }

    internal void AllocMemory(int _size)
    {
        size = _size;
        data = new byte[_size];
        pos = 0;
        stream = null;
    }

    public void Dispose()
    {
        stream.Dispose();
    }

    public int GetSize()
    {
        return size;
    }

    public bool IsValid()
    {
        if (data is not null || (data is null && stream is not null))
        {
            return true;
        }
        return false;
    }

    public string GetFileName()
    {
        return Location.FileName;
    }

    public byte[] GetUnpackedData()
    {
        return data;
    }

    public byte[] GetData()
    {
        byte[] eax_1 = GetUnpackedData();
        if (eax_1 is null && stream is not null)
        {
            data = new byte[size];
            stream.ReadExactly(data, 0, size);
            return data;
        }
        return eax_1!;
    }
}
