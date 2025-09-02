
using System.Collections.Generic;
using System.Linq;

namespace Siroshton.Masaya.Util
{

    static public class PathUtil
    {

        static public string[] Split(string path)
        {
            if( path == null ) return new string[]{ "", "" };
            if( path.EndsWith("/") )
            {
                if(path.Length == 1) return new string[] { path, "" };
                else return new string[]{ path.Substring(0, path.Length - 1), "" };
            }

            int i = path.LastIndexOf('/');
            if( i < 0 ) return new string[] { "", path };

            return new string[] { path.Substring(0, i), path.Substring(i + 1) };
        }

        static public string[] GetUniquePathsFromFiles(string[] files)
        {
            if( files == null ) return null;
            if( files.Length == 0 ) return null;

            HashSet<string> paths = new HashSet<string>();
            for(int i=0;i<files.Length;i++)
            {
                string[] parts = Split(files[i]);
                if( parts[0] != "" ) paths.Add(parts[0]);
            }

            return paths.ToArray<string>();
        }
    }
}