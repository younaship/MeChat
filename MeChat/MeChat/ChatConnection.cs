using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.Serialization.Json;

using MeChat;
using MySocket;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace MeChat.Net
{
    public class ChatConnection : MyConnection
    {
        public string MyId { set; get; }
        public string Token {  set; get; }
        public MeChatWs Ws { private set; get; }

        public const string REQUEST_URI = "http://a.younaship.com:3000";

        /// <summary>
        /// ユーザー作成リクエストを送信します。(ログイン処理は含みません。)
        /// </summary>
        /// <returns>KVP(結果[bool],メッセージ[string])</returns>
        /// <param name="passwd">MD5</param>
        public async Task<KeyValuePair<bool, string>> CreateUser(string id, string email, string passWd)
        {
            ReqLogginJson req = new ReqLogginJson();
            req.id = id;
            req.email = email;
            req.passwd = passWd;

            try
            {
                var json = Serializer<ReqLogginJson>.GetJson(req);
                var resJson = await Post(REQUEST_URI + "/Account/CreateUser/", json);
                var res = Serializer<ResponseMessageJson>.GetT(resJson);

                if (res.status == "success") return new KeyValuePair<bool, string>(true, "success.");
                else return new KeyValuePair<bool, string>(false, res.data);
            }
            catch { return new KeyValuePair<bool, string>(false, "Error."); }

        }

        /// <summary>
        /// 認証トークンを取得します。ログイン処理。成功した場合、id,Tokenがthis Connectionに設定されます。
        /// </summary>
        /// <param name="passWd">Input MD5 Passwd.</param>
        public async Task<string> GetToken(string id, string passWd)
        {
            try
            {
                var req = new ReqLogginJson();
                req.id = id;
                req.passwd = passWd;
                req.device = Environment.MachineName;

                string json;

                DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(ReqLogginJson));
                using (MemoryStream stm = new MemoryStream())
                {
                    dc.WriteObject(stm, req);
                    stm.Position = 0;
                    using (StreamReader read = new StreamReader(stm)) json = read.ReadToEnd();
                }

                string resJson = await Post(REQUEST_URI + "/Account/Loggin/", json);

                var res = Serializer<ResponseMessageJson>.GetT(resJson);
                if (res.status == "success")
                {
                    this.MyId = id;
                    this.Token = res.data;
                    return res.data;
                }
                Debug.Error("GET Token Error. " + res.status + " > " + res.data);

                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// ユーザーのプロファイルデータを取得します。
        /// </summary>
        /// <param name="Id">対象ユーザーID</param>
        public async Task<UserProfile> GetUserProfile(string Id)
        {
            if (Id == null) return null;
            try
            {
                Debug.Log("Will Get UserProfile. (Req " + REQUEST_URI + "/Get/User/" + Id + ")");
                string json = await Get(REQUEST_URI + "/Get/User/" + Id);
                Debug.Log("Get Json : " + json);

                var res = Serializer<BaseJson<UserJson>>.GetT(json);
                if (res.status == "success") return res.data[0].ToUserProfile();
                else return null;
            }
            catch { Debug.Error("[Ex] in GetUserProfile()"); return null; }
        }

        /// <summary>
        /// 複数ユーザーのプロファイルデータを取得します。
        /// </summary>
        public async Task<UserProfile[]> GetUserProfiles(string[] Ids)
        {
            try
            {
                var param = CreateParam();
                var s = Ids[0];
                for (int i = 1; i < Ids.Length; i++) s += "," + Ids[i];
                param.Add("req", s);

                string uri = REQUEST_URI + "/Get/AnyUser?req=" + s;
                Debug.Log("Will Get UserProfile. (Req " + uri);
                string resJson = await Get(uri);
                Debug.Log("Get Json : " + resJson);

                var res = Serializer<BaseJson<UserJson>>.GetT(resJson);
                if (res.status == "success") {
                    var ls = new List<UserProfile>();
                    foreach (var v in res.data) ls.Add(UserProfile.FromJson(v));
                    return ls.ToArray();
                }
                else return null;
            }
            catch { Debug.Log("Ex", "in GetUserProfiles()"); return null; }
        }

        /// <summary>
        /// ユーザーとのトーク内容を取得します。
        /// </summary>
        /// <param name="time">開始時間を指定します。この時間以降の内容を取得します。(未指定："1990.01.01")</param>
        public async Task<TalkData> GetTalkData(string toId, string time = "1990-01-01")
        {
            BaseJson<TalkJson> b;

            var param = CreateParam();
            param.Add("time", time);
            param.Add("token", Token);

            string json = await Get(REQUEST_URI + "/Get/Talk/" + MyId + "/" + toId, param);
            Debug.Log("Get Json : " + json);

            if ((b = BaseJson<TalkJson>.GetT(json)) == null) return null;

            return TalkData.FromJson(MeChatHost.MeChat, toId, b);
        }

        /// <summary>
        /// 指定した相手にトークメッセージを送信します。
        /// </summary>
        public async Task<TalkJson> SendMessage(string toId, string message)
        {
            try
            {
                ReqTalkJson req = new ReqTalkJson();
                req.myid = MyId;
                req.toid = toId;
                req.message = message;
                req.token = Token;

                string json = Serializer<ReqTalkJson>.GetJson(req);

                Debug.Log("Will Send Message. (To " + toId + ") Json : " + json);
                var resJson = await Post(REQUEST_URI + "/Post/Talk", json);
                var res = Serializer<BaseJson<TalkJson>>.GetT(resJson);
                if (res.status != "success") return null;
                Debug.Log("Success", "Send Message.");
                return res.data[0];
            }
            catch { Debug.Error("[Ex] in SendMessage()"); return null; }
        }

        /// <summary>
        /// フレンド申請をします。
        /// </summary>
        public async Task<bool> FriendRequest(string toid)
        {
            if (toid == null || toid == "" || toid.Length > 30) return false;
            var req = new RequestJson();
            req.myid = MyId;
            req.toid = toid;
            req.token = Token;

            try
            {
                var resJson = await Post(REQUEST_URI + "/Post/AddFriend/", Serializer<RequestJson>.GetJson(req));
                var res = Serializer<ResponseMessageJson>.GetT(resJson);
                if (res.status == "success") return true;
                return false;
            }
            catch { return false; }
            
        }

        /// <summary>
        /// フレンドリストを取得します。
        /// </summary>
        /// <param name="time">検索開始時間を指定。この時間以降のフレンドリストを取得します。</param>
        public async Task<FriendProfile[]> GetFrineds(string time)
        {
            if (time == null) time = "1990-01-01";
            try
            {
                var param = CreateParam();
                param.Add("token", Token);
                await Task.Delay(100);
                var resJson = await Get(REQUEST_URI + "/Get/Friends/" + MyId, param);

                var res = BaseJson<FrinedJson>.GetT(resJson);
                if (res.status != "success") return null;
                Debug.Log("GET success.");

                List<FriendProfile> list = new List<FriendProfile>();
                foreach(var v in res.data)
                {
                    list.Add(new FriendProfile()
                    {
                        Id = v.idto,
                        Time = v.time
                    });
                }
                return list.ToArray();
            }
            catch { return null; }
        }

    }

    public class Serializer<T>
    {
        public static T GetT(string json)
        {
            T t;
            try
            {
                DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream stm = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    t = (T)dc.ReadObject(stm);
                }
            }
            catch { return default(T); }
            return t;
        }

        public static string GetJson(T t)
        {
            string json;
            DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream stm = new MemoryStream())
            {
                dc.WriteObject(stm, t);
                stm.Position = 0;
                using (StreamReader read = new StreamReader(stm)) json = read.ReadToEnd(); ;
            }
            return json;
        }
    }

    public class BaseJson<T> : Serializer<BaseJson<T>>
    {
        public string status;
        public T[] data;

        public static BaseJson<T> FromJson(string json)
        {
            return GetT(json);
        }
    }

    public class ResponseMessageJson
    {
        public string status;
        public string data;
    }

    public class ResponseHexJson
    {
        /// <summary>
        /// メッセージ内容を16進数(HEX)として認識します。その後stringへと置き換えます。
        /// </summary>
        public static ResponseHexJson Create(string json)
        {
            var r = new ResponseHexJson();
            
            DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(ResponseHexJson));
            using (MemoryStream stm = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                r = (ResponseHexJson)dc.ReadObject(stm);
            }
            return r;
        }

        public string status;
        public byte[] data;

        public string GetDataHex()
        {
            return MyConnection.GetHex(data);
        }
    } 

    public class UserJson
    {
        public string id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string header { get; set; }
        public string summary { get; set; }
        public string time { get; set; }

        public UserProfile ToUserProfile()
        {
            return new UserProfile(id, name, icon, header, summary);
        }
    }

    public class TalkJson
    {
        public string idfrom { get; set; }
        public string idto { get; set; }
        public string time { get; set; }
        public string summary { get; set; }
    }

    public class FrinedJson
    {
        public string idto;
        public string idfrom;
        public string time;
    }

    public class RequestJson
    {
        public string token { get; set; }
        public string myid { get; set; }
        public string toid { get; set; }
    }

    public class ReqTalkJson : RequestJson
    {
        public string message { get; set; }
    }

    public class ReqLogginJson : RequestJson
    {
        public string id { get; set; }
        public string passwd { get; set; }
        public string email { get; set; }
        public string device { get; set; }
    }

    public class MyConnection
    {
        public class Result
        {
            public enum Status { Success, Error }
            public string Message;
        }

        /// <summary>
        /// GETリクエスト。
        /// </summary>
        /// <param name="parameters">Param ("id","value(UTF-8)") </param>
        /// <returns></returns>
        public static async Task<string> Get(string uri,Dictionary<string,string> parameters = null)
        {
            try
            {
                if (parameters != null)
                {
                    uri += "?";
                    foreach (KeyValuePair<string, string> kvp in parameters) uri += "&" + kvp.Key + "=" + Uri.EscapeUriString(kvp.Value);
                }


                Debug.Log("[GET] " + uri);
                var req = (HttpWebRequest)WebRequest.Create(uri);
                WebResponse res = await req.GetResponseAsync();
                Stream stm = res.GetResponseStream();

                string json;
                using (StreamReader str = new StreamReader(stm))
                {
                    json = await str.ReadToEndAsync();
                }
                return json;
            }
            catch(Exception e)
            {
                Debug.Error("Ex" + e.GetType());
                return null;
            }
        }

        /// <summary>
        /// GETリクエスト。OLD
        /// </summary>
        /// <param name="parameters">Param ("id","value(UTF-8)") </param>
        /// <returns></returns>
        public static async Task<string> Get_(string uri, Dictionary<string, string> parameters = null)
        {
            if (parameters != null)
            {
                uri += "?";
                foreach (KeyValuePair<string, string> kvp in parameters) uri += "&" + kvp.Key + "=" + Uri.EscapeUriString(kvp.Value);
            }

            Debug.Log("[GET] " + uri);
            var req = (HttpWebRequest)WebRequest.Create(uri);
            WebResponse res = await req.GetResponseAsync();
            Stream stm = res.GetResponseStream();

            string json;
            using (StreamReader str = new StreamReader(stm))
            {
                json = await str.ReadToEndAsync();
            }
            return json;
        }

        public static Dictionary<string,string> CreateParam()
        {
            return new Dictionary<string, string>();
        }

        public static async Task<string> Post(string uri, string json)
        {
            Debug.Log("[POST] " + uri);
            var req = WebRequest.Create(uri);
            var bytes = Encoding.UTF8.GetBytes(json);
            req.Method = "POST";
            req.ContentLength = bytes.Length;
            req.Timeout = 5000;
            req.ContentType = "application/json";

            using (var reqStm = await req.GetRequestStreamAsync())
            {
               await reqStm.WriteAsync(bytes,0,bytes.Length);
            }
            var res = await req.GetResponseAsync();

            string rJson;
            using (Stream stm = res.GetResponseStream())
            using (StreamReader str = new StreamReader(stm))
            {
                rJson = await str.ReadToEndAsync();
            }
            return rJson;
        }

        /// <summary>
        /// ファイルのダウンロードを行います。(pathにはファイル名、拡張子を含む必要があります。)
        /// </summary>
        public static async Task<bool> DownLoadToLocal(string uri,string path)
        {
            Debug.Log("DL", "Try Download task["+uri+"]>["+path+"]");
            try
            {
                var web = new WebClient();
                await web.DownloadFileTaskAsync(new Uri(uri), path);
            }
            catch(Exception e) {
                Debug.Error("[Ex] Missing Download. " + e.GetType()+" : "+e.Message);
                FileInfo f = new FileInfo(path);
                if (f.Exists) f.Delete(); // 失敗したファイル削除
                return false;
            }
            Debug.Log("Success", "Download OK.");
            return true;
        }

        public static string GetMD5(string baseStr)
        {
            MD5 md5 = MD5.Create();
            var mb = md5.ComputeHash(Encoding.UTF8.GetBytes(baseStr));

            string res = "";
            foreach (byte b in mb) res += b.ToString("x2");
            return res;
        }

        public static string GetHex(byte[] bytes)
        {
            string res = "";
            foreach (byte b in bytes) res += b.ToString("x2");
            return res;
        }
    }
}
