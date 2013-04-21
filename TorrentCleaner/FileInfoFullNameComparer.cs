using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCleaner
{
    public class FileInfoFullNameComparer : IEqualityComparer<FileInfo>
    {
        public static readonly FileInfoFullNameComparer Instance = new FileInfoFullNameComparer();

        public bool Equals(FileInfo x, FileInfo y)
        {
            if (x == y) return true;

            if (x == null || y == null) return false;

            return string.Equals(x.FullName, y.FullName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(FileInfo obj)
        {
            return obj.FullName.ToLowerInvariant().GetHashCode();
        }
    }
}
