using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MySocket;
using MeChat.Net;

namespace MeChat
{
    public class MeChatWs
    {
        const string URI = "ws://a.younaship.com:3001/WS";

        public bool IsEnable; // トークデーター読み込み前など動作をさせたくないときF。読み込み後Tにします。
        public bool IsConnecting { private set; get; } // 接続状況
        public SocketEventHandler OnError;

        MeChat MeChat;
        MyWebSocket MyWs;

        public MeChatWs(MeChat meChat)
        {
            this.IsConnecting = false;
            this.IsEnable = false;
            this.MeChat = meChat;
            MyWs = new MyWebSocket(URI);
            MyWs.SetWsRequestHeader("token", MeChat.Connection.Token);
            MyWs.SetWsRequestHeader("id", MeChat.Connection.MyId);
        }

        public async Task<bool> Start()
        {
            Debug.Log("Ws", "Try Connect.");
            var result = await MyWs.Connect();
            if (!result) return false;
            MyWs.Logger += (x) => Debug.Log("ws", x);
            MyWs.OnReceive += GetMessage;

            MyWs.OnError = (x) => IsConnecting = false; //エラー ＝ 接続終了
            MyWs.OnError += OnError;
            IsConnecting = true;

            Debug.Log("Ws", "Start Receive.");
            MyWs.StartReceive();
            return true;
        }

        
        private void GetMessage(string message)
        {
            if (!IsEnable) return;
            Debug.Log("Ws", "GetMessage : " + message);
            try
            {
                var data = Serializer<WsJson<TalkJson>>.GetT(message);
                if (data.type != "talk")
                {
                    Debug.Error("Ws", "Error.No match Type.");
                    return;
                }
                var talk =  data.GetFirst();
                if (talk.idto != MeChat.Config.Id) return;
                MeChat.TalkDataHost.AddCellToMe(talk.idfrom, talk.summary, talk.time);
            }
            catch(Exception e) { Debug.Error("Ex", "in Ws.GetMessage()" + e.GetType().ToString()+" "+e.Message); }
        }

        public class WsJson<T>
        {
            public string type;
            public T[] data;
            public T GetFirst()
            {
                if (data == null) return default(T);
                return data[0];
            }
        }
    }
}
