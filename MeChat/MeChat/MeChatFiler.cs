using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace MeChat.File
{
    /* ローカルデーターを管理するためのクラス
     *
     */
    public enum DataType { Profile, Talk }
    public class Filer
    {
        const string FOLDER_PATH_MEDIA = "media\\";
        const string FILEPATH_CONFIG = "data2.conf";
        const string FILEPATH_TALK = "talk.data";
        const string FILEPATH_FRIENDS = "friends.data";
        const string FILEPATH_USER_PROFILE = "profiles.data";

        public string Path { private set; get; }

        public Filer() { Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+"/" ; }

        /// <summary>
        /// ローカルデーターを読み込みます。子のデーターにはログイン情報が含まれます。(Null is NotFound.)
        /// </summary>
        /// <returns></returns>
        public Config LoadConfig()
        {
            Debug.Log("Loading Config.");
            try
            {
                return IO<Config>.Load(Path + FILEPATH_CONFIG);
            }
            catch { return null; }
        }

        bool enableSaveConfig = true;
        public async Task<bool> SaveConfigAsync(Config config)
        {
            if (!enableSaveConfig) return false;
            enableSaveConfig = false;

            bool result = false;
            try
            {
                result = await IO<Config>.SaveAsync(Path + FILEPATH_CONFIG, config);
                Debug.Log("Fin", "Save Config.");
            }
            catch { }

            enableSaveConfig = true;
            return result;
        }

        /* FRIEND DATA */
        public async Task<FriendProfile[]> LoadFriendsAsync()
        {
            try
            {
                return await IO<FriendProfile[]>.LoadAsync(Path + FILEPATH_FRIENDS);
            }
            catch { return null; }
        }

        public async Task<bool> SaveFriendsAsync(FriendProfile[] profiles)
        {
            try
            {
                return await IO<FriendProfile[]>.SaveAsync(Path + FILEPATH_FRIENDS, profiles);
            }
            catch { return false; }
        }

        /* TALK DATA */
        public TalkData LoadTalkData(string toId)
        {
            try
            {
                return IO<TalkData>.Load(Path + "talk/" + toId);
            }
            catch { return null; }
        }

        public async Task<TalkData> LoadTalkDataAsync(string toId)
        {
            try
            {
                return await IO<TalkData>.LoadAsync(Path + "talk/" + toId);
            }
            catch { return null; }
        }

        public bool SaveTalkData(TalkData data)
        {
            string toId = data.ToId;
            if (toId == null) return false;
            try
            {
                return IO<TalkData>.Save(Path + "talk/" + toId, data);
            }
            catch { return false; }
        }

        /* USER PROFILE DATA */
        public async Task<UserProfile[]> LoadUserProfiles()
        {
            try
            {
                return await IO<UserProfile[]>.LoadAsync(Path + FILEPATH_USER_PROFILE);
            }
            catch { return null; }
        }

        public async Task<bool> SaveUserProfiles(UserProfile[] profiles)
        {
            try
            {
                return await IO<UserProfile[]>.SaveAsync(Path + FILEPATH_USER_PROFILE, profiles);
            }
            catch { return false; }
        }


        /* LogOut and All Delete*/

        public void DeleteAllFiles()
        {
            FileManager.DeleteAll(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            FileInfo f = new FileInfo("/data/user/0/com.companyname.MeChat/files/profiles.data");
            if (f.Exists)
            {
                f.Delete();
                Debug.Log("DEBUG", "NOW DEBUG DELETE.");
            }
            return;
        }

        public class FileManager
        {
            public void DeleteAll_(string path)
            {
                DirectoryInfo d = new DirectoryInfo(path);

                Debug.Log("Find " + d.GetFiles().Length + " File " + d.GetDirectories().Length + " Dire in " + d.FullName + "");


                foreach (var v in d.GetDirectories())
                    try { v.Delete(); Debug.Log("Deleted Directory [" + v.FullName + "]"); }
                    catch { Debug.Error("Delete Directory Missing![" + v.FullName + "]"); }

                foreach (var v in d.GetFiles())
                    try { v.Delete(); Debug.Log("Deleted File [" + v.FullName + "]"); }
                    catch { Debug.Error("Delete File Missing![" + v.FullName + "]"); }
            }

            public static void DeleteAll(string path)
            {
                List<FileInfo> ls = new List<FileInfo>();
                FindDrectory(ref ls, path);
                Debug.Log("FInd " + ls.Count);
                foreach (var v in ls) {
                    Debug.Log("try delete "+v.FullName);
                    try { v.Delete(); }
                    catch (Exception e){ Debug.Log("Exception " + e); }
                }
            }

            private static void FindDrectory(ref List<FileInfo> files,string path)
            {
                DirectoryInfo d = new DirectoryInfo(path);
                files.AddRange(d.GetFiles());
                foreach (var v in d.GetDirectories())
                    FindDrectory(ref files, v.FullName);
            }
        }

        public class IO<T>
        {
            /// <summary>
            /// T を Json としてファイルを書き込みます。
            /// </summary>
            public static bool Save(string path, T data)
            {
                Debug.Log("Fs > " + path);
                try
                {
                    FileStream f = new FileStream(path, FileMode.Create);
                    DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(T));
                    dc.WriteObject(f, data);
                    f.Close();
                    return true;
                }
                catch { return false; }
            }

            public static async Task<bool> SaveAsync(string path, T data)
            {
                Debug.Log("Fs > " + path);
                try
                {
                    FileStream f = new FileStream(path, FileMode.Create);
                    MemoryStream m = new MemoryStream();

                    DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(T));
                    dc.WriteObject(m, data);
                    m.Position = 0;

                    string json;
                    using (StreamReader str = new StreamReader(m))
                        json = await str.ReadToEndAsync();
                    using (StreamWriter stm = new StreamWriter(f))
                    {
                        byte[] b = Encoding.UTF8.GetBytes(json);
                        await f.WriteAsync(b, 0, b.Length);
                    }

                    f.Close();
                    return true;
                }
                catch (Exception e) { return false; }
            }

            /// <summary>
            /// Json を T としてファイルを読み込みます。
            /// </summary>
            public static T Load(string path)
            {
                Debug.Log("Fs > " + path);
                try
                {
                    FileStream f = new FileStream(path, FileMode.Open);
                    DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(T));
                    T t = (T)dc.ReadObject(f);
                    f.Close();
                    Debug.Log("Success", "Load File : type [" + typeof(T).ToString() + "]");
                    return t;
                }
                catch
                {
                    Debug.Log("Missing", "Load File type [" + typeof(T).ToString() + "]");
                    return default(T);
                }
            }

            public static async Task<T> LoadAsync(string path)
            {
                Debug.Log("Fs > " + path);
                try
                {
                    string s;
                    using (FileStream f = new FileStream(path, FileMode.Open))
                    using (StreamReader read = new StreamReader(f))
                        s = await read.ReadToEndAsync();

                    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(s));
                    DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(T));
                    T t = (T)dc.ReadObject(ms);
                    ms.Close();
                    Debug.Log("Success", "Load File Async : type [" + typeof(T).ToString() + "]");
                    return t;
                }
                catch
                {
                    Debug.Log("Missing", "Load File Async type [" + typeof(T).ToString() + "]");
                    return default(T);
                }
            }
        }

    }

    public class Config
    {
        public string Id; // ログイン済みID
        public string Token; // 認証トークン情報
        public UpdateTime Time = new UpdateTime(); // 最終更新時間

        public class UpdateTime
        {
            public string Talk;
            public string Friends;
        }
    }
}