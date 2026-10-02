using System;
using System.IO;

namespace Core.Infrastructure.Services.Save.Files
{
    internal static class FilePathUtility
    {
        public static bool IsContainedAndLinkFree(string rootPath, string absolutePath)
        {
            try
            {
                var root = Path.GetFullPath(rootPath);
                var candidate = Path.GetFullPath(absolutePath);
                var boundary = root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                    ? root
                    : root + Path.DirectorySeparatorChar;
                var comparison = Path.DirectorySeparatorChar == '\\'
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

                return candidate.StartsWith(boundary, comparison) &&
                       ContainsReparsePoint(root, candidate) == false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool TryResolve(string rootPath, string relativePath, out string absolutePath)
        {
            absolutePath = null;

            if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(relativePath))
                return false;

            if (Path.IsPathRooted(relativePath))
                return false;

            var lastCharacter = relativePath[relativePath.Length - 1];
            if (lastCharacter == '/' || lastCharacter == '\\')
                return false;

            try
            {
                var root = Path.GetFullPath(rootPath);
                var boundary = root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                    ? root
                    : root + Path.DirectorySeparatorChar;
                var combined = Path.GetFullPath(Path.Combine(root, relativePath));
                var comparison = Path.DirectorySeparatorChar == '\\'
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

                if (combined.StartsWith(boundary, comparison) == false)
                    return false;

                if (ContainsReparsePoint(root, combined))
                    return false;

                absolutePath = combined;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool ContainsReparsePoint(string root, string absolutePath)
        {
            if (Directory.Exists(root) &&
                (File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0)
                return true;

            var relative = absolutePath.Substring(root.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var segments = relative.Split(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            for (var i = 0; i < segments.Length; i++)
            {
                current = Path.Combine(current, segments[i]);

                try
                {
                    if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                        return true;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (DirectoryNotFoundException)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
