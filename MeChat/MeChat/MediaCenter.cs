using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xamarin.Forms;

namespace MeChat.File
{
    public class MediaCenter
    {
        public const string DEFAULT_IMAGE = "MeChat.Droid.img.human.png";
        public string Path { private set; get; }
        public MediaCenter()
        {
            Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/media/";
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
        }

        /// <summary>
        /// ファイルが存在するかを確認します。
        /// </summary>
        public bool ChkFile(string fname)
        {
            FileInfo f = new FileInfo(Path + fname);
            return f.Exists;
        }

        /// <summary>
        /// 画像を取得します。ローカルにある場合ImageSourceとして返され、ない場合はDLされます。
        /// </summary>
        /// <param name="uri">URL</param>
        /// <param name="completeCallBack">ダウンロード完了後のコールバックです。</param>
        /// <param name="defImage">ダウンロードされていない場合表示する画像</param>
        /// <returns></returns>
        public ImageSource GetImage(string uri,Action<ImageSource> completeCallBack, string defImage = DEFAULT_IMAGE)
        {
            Debug.Log("MediaCenter", "GET "+uri+"");
            if (uri == null) return ImageSource.FromResource(defImage);

            string fname = GetMD5(uri);
            if (ChkFile(fname)) return ImageSource.FromFile(Path + fname);
            else
            {
                ImageSource src = ImageSource.FromResource(defImage);
                Download(uri, completeCallBack);
                return src;
            }
        }

        static List<string> dlList = new List<string>();
        private async void Download(string uri, Action<ImageSource> callback)
        {
            if (!dlList.Exists(x => x == uri)) dlList.Add(uri);
            else return;

            string path = Path + GetMD5(uri);
            await System.Threading.Tasks.Task.Delay(1);
            if (await Net.MyConnection.DownLoadToLocal("http://api.younaship.com" + uri, path))
            {
                var src = ImageSource.FromFile(path);
                callback?.Invoke(src);
            }
            dlList.Remove(uri);
        }

        public string GetMD5(string str)
        {
            return Net.MyConnection.GetMD5(str);
        }

    }
}
