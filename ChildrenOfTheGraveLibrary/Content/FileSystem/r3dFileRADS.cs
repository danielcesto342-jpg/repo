using System.IO;
using ChildrenOfTheGraveLibrary.Content.Interfaces;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content.FileSystem;

internal class r3dFileRADS : r3dFile
{
    internal IFile m_pFile;
    internal string m_FileName;
    internal byte[] m_Data;  //char*

    public string GetFileName()
    {
        //Wtf?
        //if (*(uint32_t*)((char*)this + 0x24) < 0x10)
        //{
        //    return &this[0x10];
        //}
        //return *(uint32_t*)(this + 0x10);

        return m_FileName;
    }
    public int GetSize()
    {
        return m_Data?.Length ?? 0;
    }
    public bool IsValid()
    {
        return m_pFile != null;
    }
    public byte[] GetUnpackedData() //void*
    {
        //wtf?
        /* jump -> this->vtable->GetData */
        return GetData();
    }
    public byte[] GetData()
    {
        if (m_Data is null)
        {
            m_Data = File.ReadAllBytes(m_FileName);
        }
        return m_Data;
    }

    internal void Close()
    {
        //?
    }
}
