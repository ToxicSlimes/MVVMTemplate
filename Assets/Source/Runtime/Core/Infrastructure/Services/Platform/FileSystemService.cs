using System.IO;
using Core.Abstractions.Services.Platform;

namespace Core.Infrastructure.Services.Platform
{
    public class FileSystemService : IFileSystemService
    {
        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }
}
