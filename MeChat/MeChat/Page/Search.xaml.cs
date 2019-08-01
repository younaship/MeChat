using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MeChat.Page
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Search : ContentPage
	{
		public Search ()
		{
			InitializeComponent ();
		}

        bool isRunning = false;
        private async void Button_Clicked_LogOut(object sender, EventArgs e)
        {
            if (isRunning) return;
            else isRunning = true;

            await Task.Delay(10);
            await MeChatHost.MeChat.Loggout();
            await DisplayAlert("OK", "Logoutしました。", "OK");
            await Task.Delay(10);

            Application.Current.MainPage = new NavigationPage(new Page.LoginPage()) { Title = "ログイン" };
            isRunning = false;
        }

        private async void Button_Clicked_ReqFriend(object sender, EventArgs e)
        {
            if (isRunning) return;

            await Task.Delay(10);
            MessagingCenter.Send(App.app, "dialog_progress", "送信中");
            var res = await MeChatHost.MeChat.Connection.FriendRequest(Entry_ReqId.Text);
            MessagingCenter.Send(App.app, "dialog_progress", "");
            if (res) await DisplayAlert("成功", "送信しました。", "OK");
            else await DisplayAlert("失敗", "IDが間違っているか存在しない可能性があります。", "OK");

            isRunning = false;
        }
    }
}