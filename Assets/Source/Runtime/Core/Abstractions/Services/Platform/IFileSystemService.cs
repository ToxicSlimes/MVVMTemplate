namespace Core.Abstractions.Services.Platform
{
    public interface IFileSystemService
    {
        byte[] ReadAllBytes(string path);
        void WriteAllBytes(string path, byte[] bytes);
        bool FileExists(string path);
    }
}
