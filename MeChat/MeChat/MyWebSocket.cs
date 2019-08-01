using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 *  MyWebSocekt.cs 
 *  Ver 0.11 (19.06.06)
 *  Ver 0.1 (19.05.25)
 *  クライアント側のWebSocekt通信をする為のクラス
 */

namespace MySocket
{
    public delegate void SocketEventHandler(string str);
    
    public class MyWebSocket
    {
        public string Uri { get; private set; }
        public ClientWebSocket Ws { get; private set; }
        public SocketEventHandler OnReceive { get; set; }
        public SocketEventHandler OnError { get; set; }
        public SocketEventHandler Logger { get; set; }

        CancellationTokenSource cancel;

        public MyWebSocket(string uri)
        {
            Uri = uri;
            cancel = new CancellationTokenSource();
            Ws = new ClientWebSocket();
        }

        bool isEnable;
        public async void StartReceive()
        {
            isEnable = true;
            while (isEnable)
            {
                try
                {
                    string res = await Receive();
                    OnReceive?.Invoke(res);
                }
                catch {
                    OnError?.Invoke(Ws.State.ToString());
                    ChkConnection();
                    return; }
            }
        }

        private void ChkConnection()
        {
            Logger?.Invoke("Connection : " + Ws.State);
        }

        public void Finish()
        {
            isEnable = false;
            cancel.Cancel();
        }

        public async Task<bool> Connect()
        {
            try
            {
                await Ws.ConnectAsync(new System.Uri(Uri), cancel.Token);
                if (Ws.State == WebSocketState.Open)
                {
                    Logger?.Invoke("Connected");
                    return true;
                }
                else
                {
                    Logger?.Invoke("Aborted : " + Ws.CloseStatusDescription);
                    return false;
                }
            }
            catch { return false; }
        }

        private async Task<string> Receive()
        {
            var array = new ArraySegment<byte>(new byte[256]);
            var result = await Ws.ReceiveAsync(array, cancel.Token);

            var res = Encoding.UTF8.GetString(array.Take(result.Count).ToArray());
            return res;
        }

        public async Task Send(byte[] data)
        {
            var array = new ArraySegment<byte>(data);
            await Ws.SendAsync(array, WebSocketMessageType.Binary, false, cancel.Token);
            Logger?.Invoke("Sended [" + data.Length + "] Byte Data.");
        }

        public MyWebSocket SetWsRequestHeader(string key,string value)
        {
            this.Ws.Options.SetRequestHeader(key, value);
            return this;
        }
    }
}
