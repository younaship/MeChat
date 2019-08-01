using MeChat.File;
using MeChat.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MeChat
{
    public class UserProfile
    {
        public UserProfile(string id)
        {
            Id = id;
        }

        public UserProfile(string id, string name, string icon, string header, string summary)
        {
            Id = id;
            Name = name;
            Icon = icon;
            Header = header;
            Summary = summary;
        }

        public string Id { private set; get; }
        public string Name { private set; get; }
        public string Icon { private set; get; }
        public string Header { private set; get; }
        public string Summary { private set; get; }

        public string Time { set; get; } // ローカル保存用 : 情報取得時間

        /// <summary>
        /// IDのみの情報から他の情報も取得します。
        /// </summary>
        public async Task<UserProfile> GetFromServer(ChatConnection connection)
        {
            await connection.GetUserProfile(Id);
            return this;
        }

        public static UserProfile FromJson(UserJson user)
        {
            return new UserProfile(user.id)
            {
                Header = user.header,
                Icon = user.icon,
                Name = user.name,
                Summary = user.summary,
                Time = user.time
            };
        }
    }

    public class UserProfileHost : IDataFiler
    {
        public List<UserProfile> UserProfiles { get; private set; }
        public List<FriendProfile> FriendProfiles { get; private set; }
        MeChat MeChat;

        public UserProfileHost(MeChat meChat, List<FriendProfile> friends)
        {
            UserProfiles = new List<UserProfile>();
            this.MeChat = meChat;
            this.FriendProfiles = friends;
        }

        /// <summary>
        /// ローカルもしくはサーバー取得に失敗した場合Falseが返されます。Falseでもデーターが存在する可能性があります。
        /// </summary>
        public async Task<bool> Get()
        {

            if (!await GetFromLocal()) // ローカルから取得できない場合サーバーからの情報をすべて利用
            {
                if (FriendProfiles.Count <= 0) return false;
                UserProfiles = new List<UserProfile>();

                var my = await MeChat.Connection.GetUserProfile(MeChatHost.MeChat.Config.Id);
                if (my == null) return false;
                UserProfiles.Add(my);

                var res = await MeChat.Connection.GetUserProfiles(FriendProfiles.ConvertAll<string>((x) => {
                    return x.Id;
                }).ToArray());

                if (res == null) return false;
                UserProfiles.AddRange(res);
                await MeChat.Filer.SaveUserProfiles(UserProfiles.ToArray());
                return true;
            }
            List<string> reqIdList = new List<string>(); //サーバーに要求するIDリスト

            foreach (var f in FriendProfiles)
                if (!UserProfiles.Exists(x => x.Id == f.Id))
                    reqIdList.Add(f.Id); // ローカルに存在しないIDをリストに追加
            
            var get = await MeChat.Connection.GetUserProfiles(reqIdList.ToArray());
            if (get == null) return false;

            Debug.Log("Add : " + get.Length + " items. in UserProfiles.GET()");
            UserProfiles.AddRange(get);
            return true;
        }

        private async Task<bool> GetFromLocal()
        {
            var fs = await MeChat.Filer.LoadUserProfiles();
            if (fs == null) return false;
            UserProfiles = new List<UserProfile>(fs);
            return true;
        }

        public UserProfile GetById(string id)
        {
            return UserProfiles.Find((x) => x.Id.ToLower() == id.ToLower());
        }

        public string GetNameById(string id)
        {
            var u = UserProfiles.Find((x) => x.Id.ToLower() == id.ToLower());
            if (u == null) return "Unknown";
            return u.Name;
        }

        public string GetIconById(string id)
        {
            var u = UserProfiles.Find((x) => x.Id.ToLower() == id.ToLower());
            if (u == null) return null;
            return u.Icon;
        }
    }
}
