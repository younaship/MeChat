using MeChat.File;
using MeChat.Net;

using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MeChat
{
    public class FriendProfile
    {
        public bool ToMe, ToYou;
        public string Id;
        public string Time;
    }

    public class FriendProfileHost : IDataFiler
    {
        public List<FriendProfile> FriendProfiles { get; private set; }
        MeChat MeChat;

        public FriendProfileHost(MeChat meChat)
        {
            FriendProfiles = new List<FriendProfile>();
            this.MeChat = meChat;
        }

        public async Task<bool> Get()
        {
            await GetFromLocal();
                if (await GetFromServer())
                    return true;
            return false;
        }

        private async Task<bool> GetFromLocal()
        {
            var fs = await MeChat.Filer.LoadFriendsAsync();
            if (fs == null) return false;
            FriendProfiles = new List<FriendProfile>(fs);
            return true;
        }

        private async Task<bool> GetFromServer()
        {
            Config Config = MeChatHost.MeChat.Config;

            var time = Config.Time.Friends;
            var list = await MeChat.Connection.GetFrineds(time);
            if (list == null) return false;

            var tmp = Config.Time.Friends;
            Config.Time.Friends = list.Last().Time;

            if (Config.Time.Friends != list.Last().Time) { // 最新時間が異なるため新着あり
                if (!await MeChat.Filer.SaveConfigAsync(Config))
                {
                    Config.Time.Friends = tmp;
                    return false;
                }

                var add = new List<FriendProfile>();
                foreach (var v in list)
                    if (v.Time != Config.Time.Friends)
                        add.Add(v);
                Debug.Log("Find New " + list.Length + " Friends OnServer.");
                FriendProfiles.AddRange(add);
                if (await MeChat.Filer.SaveFriendsAsync(FriendProfiles.ToArray())) return true;
                Debug.Error("Error in GetFromServer() in SaveFile.");
            }

            if (time == null) // 元データーなしの為、サーバーからの情報をすべて反映
            {
                FriendProfiles.AddRange(list);
            }

            return false;
        }
    }

    /* */

    public interface IDataFiler
    {
        /// <summary>
        /// ストレージやサーバから情報を取得し、各データーの用意をします。
        /// </summary>
        /// <returns></returns>
        Task<bool> Get();
    }
}
