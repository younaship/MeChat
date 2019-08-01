using System;
using System.Collections.Generic;
using System.Text;
using MeChat.Net;
using MeChat.File;
using System.Threading.Tasks;

namespace MeChat
{
    public class MeChatHost
    {
        public static MeChat MeChat { private set; get; }
        /// <summary>
        /// 起動します。資格情報がない場合Falseが返されます。
        /// </summary>
        public static bool Awake() {
            MeChat = new MeChat();
            return MeChat.Awake();
        }
    }

    public class MeChat
    {
        public MeChat() {
            Filer = new Filer();
            MediaCenter = new MediaCenter();
            Connection = new ChatConnection();

            this.Config = new Config();
            FriendProfileHost = new FriendProfileHost(this);
            UserProfileHost = new UserProfileHost(this, FriendProfileHost.FriendProfiles);
            TalkDataHost = new TalkDataHost(this);
        }

        public MeChatWs MeChatWs { get; private set; }
        public Filer Filer { get; private set; }
        public MediaCenter MediaCenter { get; private set; }
        public ChatConnection Connection { get; private set; }

        public Config Config { get; private set; } // 設定・情報ファイル
        public FriendProfileHost FriendProfileHost { get; private set; } // フレンドリスト情報
        public UserProfileHost UserProfileHost { get; private set; } // フレンドなどのプロファイル情報
        public TalkDataHost TalkDataHost { get; set; }

        /// <summary>
        /// ログインを試行します。成功するとトークンが保存されます。
        /// </summary>
        public async Task<bool> Login(string id,string passWd)
        {
            var token = await Connection.GetToken(id, MyConnection.GetMD5(passWd));
            if (token == null) return false;

            if (this.Config == null) this.Config = new Config();
            this.Config.Id = id;
            this.Config.Token = token;
            if (!await Filer.SaveConfigAsync(this.Config)) Debug.Error("MeChat","Missing Save Conf.");

            return true;
        }

        public async Task Loggout()
        {
            Filer.DeleteAllFiles();
            await Task.Delay(1);
            return;
        }

        /// <summary>
        /// 起動時アクション(ユーザー情報(ログイン状況)確認 ※ファイル読み込み)
        /// </summary>
        public bool Awake() {

            if ((this.Config = Filer.LoadConfig())== null) return false;
         
            Connection.MyId = this.Config.Id;
            Connection.Token = this.Config.Token;
            Debug.Log("MeChat", "Loaded Config and Loggin [" + this.Config.Id + "]");
            

            return true;
        }

        /// <summary>
        /// Wsを準備します。
        /// </summary>
        private async Task WsAwake()
        {
            MeChatWs = new MeChatWs(this);
            await MeChatWs.Start();
        }

        /// <summary>
        /// 設定された項目(Config)を使い、すべてのデーターの準備を行います。その後Wsなど準備をします。
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            await WsAwake();

            Debug.Log("MeChat","Load FriendProfile");
            await GetData(FriendProfileHost);

            Debug.Log("MeChat","Load UserProfile");
            await GetData(UserProfileHost);

            Debug.Log("MeChat","Load TalkData");
            await GetData(TalkDataHost);

            MeChatWs.IsEnable = true;
        }

        /// <summary>
        /// ローカル及びサーバーからデーターを取得します。
        /// </summary>
        private async Task<bool> GetData(IDataFiler iData)
        {
            return await iData.Get();
        }

        /// <summary>
        /// メッセージを送信します。成功した場合送信したCell情報が返されます。失敗した場合はnullになります。
        /// </summary>
        public async Task<TalkData.Cell> SendMessage(string toId,string message)
        {
            var res = await Connection.SendMessage(toId, message);
            if (res == null) return null;
            return TalkDataHost.AddCellToYou(res.idto, res.summary, res.time);
                
        }

       
    }

}
