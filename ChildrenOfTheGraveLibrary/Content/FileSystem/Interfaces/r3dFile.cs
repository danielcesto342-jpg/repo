namespace ChildrenOfTheGraveLibrary.Content.Interfaces;

internal interface r3dFile
{
    internal string GetFileName();
    internal int GetSize();
    internal bool IsValid();
    internal byte[] GetUnpackedData(); //void*
    internal byte[] GetData(); //void*
}