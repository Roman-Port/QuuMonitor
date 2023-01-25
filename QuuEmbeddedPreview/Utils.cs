using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview
{
    static class Utils
    {
        public static FileInfo GetFile(this DirectoryInfo dir, string name)
        {
            return new FileInfo(dir.FullName + Path.DirectorySeparatorChar + name);
        }
    }
}
