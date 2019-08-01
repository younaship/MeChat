using MeChat.Net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MeChat
{
    public delegate void TalkEventHundler(TalkData.Cell cell);

    public class TalkDataHost : IDataFiler
    {
        MeChat MeChat;
        public TalkDataHost(MeChat meChat)
        {
            MeChat = meChat;
        }

        public TalkEventHundler OnAddCell; // セルが追加されたときのイベントハンドラ
        public List<TalkData> Datas { private set; get; }
        public async Task<bool> Get()
        {
            Datas = new List<TalkData>();
            foreach (FriendProfile f in MeChat.FriendProfileHost.FriendProfiles)
            {
                var localData = await MeChat.Filer.LoadTalkDataAsync(f.Id);
                if (localData == null) {
                    localData = await MeChat.Connection.GetTalkData(f.Id); // ローカル無しの為すべてを要求
                    if (localData != null) //取得成功
                        Datas.Add(localData);
                    else
                        continue;
                }
                else
                {
                    var time = localData.GetLastTime(); // 最後のセルの時間
                    var data = await MeChat.Connection.GetTalkData(f.Id, time);
                    if (data == null)
                        Datas.Add(localData); // 取得失敗したらローカルのみ使用
                    else
                    {
                        var cells = new List<TalkData.Cell>(); 
                        foreach (var v in data.cells)
                            if (v.Time != time)
                                cells.Add(v); // 取得開始時刻と同時刻はかぶりの為取り除く
                        localData.cells.AddRange(cells); // 新規取得Cellを追加
                        Datas.Add(localData);
                    }
                }
            }

            return true;
        }
        
        public TalkData GetById(string id)
        {
            return Datas.Find((x) => x.ToId == id);
        }

        /// <summary>
        /// トークセルを追加します。
        /// </summary>
        public TalkData.Cell AddCellToYou(string toid,string message,string time)
        {
            var cell = new TalkData.Cell()
            {
                Time = time,
                IsMe = true,
                Message = message
            };

            Datas.Find((x) => x.ToId.ToLower() == toid.ToLower()).AddCell(cell);
            OnAddCell?.Invoke(cell);
            return cell;
        }

        public TalkData.Cell AddCellToMe(string id, string message, string time)
        {
            var cell = new TalkData.Cell()
            {
                Time = time,
                IsMe = false,
                Message = message
            };

            Datas.Find((x) => x.ToId.ToLower() == id.ToLower()).AddCell(cell);
            OnAddCell?.Invoke(cell);
            return cell;
        }
    }

    public class TalkData
    {
        public class Cell
        {
            public bool IsMe;
            public string Time;
            public string Message;
            public string Info; // 予約

        }

        public List<Cell> cells;
        public string ToId;

        public static TalkData FromJson(MeChat meChat, string toId, BaseJson<TalkJson> json)
        {
            var v = new TalkData() { ToId = toId };
            var cs = new List<Cell>();
            foreach (TalkJson t in json.data)
                cs.Add(GetCell(t, meChat.Config.Id));

            v.cells = cs;
            return v;
        }

        public static Cell GetCell(TalkJson t,string myId)
        {
            bool isMe = false;
            if (t.idto.ToLower() != myId.ToLower()) isMe = true; // 自分宛のメッセージ
            return new Cell()
            {
                IsMe = isMe,
                Message = t.summary,
                Time = t.time
            };
        }

        public void AddCell(Cell cell)
        {
            this.cells.Add(cell);
        }

        public string GetLastTime()
        {
            if (cells.Count <= 0) return null;
            return cells[cells.Count - 1].Time;
        }

        public TimeSpan GetLastTimeSpan()
        {
            if (cells.Count <= 0) return TimeSpan.Zero;
            return TimeSpan.Parse(cells[cells.Count - 1].Time);
        }

        public string GetLastMessage()
        {
            return cells[cells.Count - 1].Message;
        }

    }

}
