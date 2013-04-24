using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCleaner
{
    public class FileSystemInfoFullNameComparer : IEqualityComparer<FileSystemInfo>
    {
        public static readonly FileSystemInfoFullNameComparer Instance = new FileSystemInfoFullNameComparer();

        public bool Equals(FileSystemInfo x, FileSystemInfo y)
        {
            if (x == y) return true;

            if (x == null || y == null) return false;

            return string.Equals(x.FullName, y.FullName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(FileSystemInfo obj)
        {
            return obj.FullName.ToLowerInvariant().GetHashCode();
        }
    }
}
