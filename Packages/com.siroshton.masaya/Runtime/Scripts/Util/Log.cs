using System.IO;
using UnityEngine;

namespace Siroshton.Masaya.Util
{

    public static class Log
    {
        private static StreamWriter _out;

        private static void OpenOut()
        {
            if( _out == null )
            {
                string path = System.IO.Path.GetDirectoryName(Application.dataPath) + "/log.txt";
                _out = File.CreateText(path);
            }
        }

        public static void Print(string text)
        {
            OpenOut();
            _out.Write(text);
            _out.Flush();
        }

        public static void Println(string text)
        {
            OpenOut();
            _out.WriteLine(text);
            _out.Flush();
        }

    }

}