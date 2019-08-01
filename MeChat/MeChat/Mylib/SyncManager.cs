using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MeChat.File{

    public class FileScaner
    {
        ///<summary> 指定フォルダー内のファイルをすべて検索し相対パスの配列で受け取ります。</summary>
        public static string[] GetFilesRelPath(string path)
        {
            List<string> list = new List<string>();
            foreach (File f in GetFiles(path)) list.Add(f.RelPath);
            return list.ToArray();
        }

        ///<summary> 指定フォルダー内のファイルをすべて検索します。</summary>
        public static File[] GetFiles(string path)
        {
            List<File> files = new List<File>();
            ScanFolder(ref files, path);
            return files.ToArray();
        }

        private static void ScanFolder(ref List<File> files, string folderPath)
        {
            DirectoryInfo info = new DirectoryInfo(folderPath);
            FileInfo[] fInfo = info.GetFiles();
            foreach (FileInfo f in fInfo) files.Add(new File()
            {
                FilePath = f.DirectoryName,
                FileName = f.Name,
                Extension = f.Extension,
                Current = folderPath
            });

            string[] fols = System.IO.Directory.GetDirectories(folderPath, " * ", System.IO.SearchOption.AllDirectories);
            foreach (var fol in fols) ScanFolder(ref files, fol);
        }

        public class File
        {
            public string FilePath, FileName, Extension, Current;
            public string Path { get { return FilePath + "\\" + FileName; } }
            public string RelPath { get { return Path.Remove(0, Current.Length + 1); } } // \まで抜くため +1
        }
    }

    public class SyncManager : FileScaner
    {
        /// <param name="syncPath">同期先のファイルパスを指定します。(例 C:\Test)</param>
        public SyncManager(string syncPath) { SyncPath = syncPath; }

        public string SyncPath { private set; get; }

        /// <summary>同期されてないファイルリストを取得します。</summary>
        /// <param name="sourcePath">比較元のフォルダーパス</param>
        /// <param name="excessFiles">比較先に存在する余分なファイルリストを出力します。(!)元に無いフォルダ未探索</param>
        public string[] GetNoSyncList(string sourcePath, out string[] excessFiles)
        {
            List<string> toList = new List<string>(GetFilesRelPath(SyncPath));
            List<string> fromList = new List<string>(GetFilesRelPath(sourcePath));
            List<string> overList = new List<string>();

            foreach (string s in toList) if (fromList.Contains(s)) fromList.Remove(s);
                else overList.Add(s);
            excessFiles = overList.ToArray();

            return fromList.ToArray();
        }

    }

}