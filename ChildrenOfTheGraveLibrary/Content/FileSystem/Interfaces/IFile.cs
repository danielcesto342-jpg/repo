namespace ChildrenOfTheGraveLibrary.Content.Interfaces;

internal interface IFile
{
    int Seek(int num, SEEK_ORIGIN unk);
    int Read(byte[] buffer, uint unk, uint unk2, ref uint unk3 /* uint32_t* */);
    int GetString(out string str);
    int Tell(int num /* int* */);
    FILE_TYPE GetFileType();
    uint GetSize();
    int IsEndOfFile();

    enum SEEK_ORIGIN
    {
        SEEK_ORIGIN_BEGINNING = 0x0,
        SEEK_ORIGIN_CURRENT = 0x1,
        SEEK_ORIGIN_END = 0x2
    }
}

internal enum FILE_TYPE
{
    FILE_TYPE_TEXT = 0x0,
    FILE_TYPE_BINARY = 0x1
}
