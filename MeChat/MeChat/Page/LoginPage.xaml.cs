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
	public partial class LoginPage : ContentPage
	{
		public LoginPage ()
		{
			InitializeComponent ();
		}

        public void Button_OK_Clicked(object obj,EventArgs e)
        {
            Loggin();
        }

        bool isRunning = false;
        private async void Loggin()
        {
            if (isRunning) return;
            else isRunning = true;
            MessagingCenter.Send(App.app, "dialog_progress", "ログイン中");

            bool result;
            if (result = await MeChatHost.MeChat.Login(Editor_UId.Text, Editor_PassWd.Text))
            {
                MessagingCenter.Send(App.app, "dialog_progress", "ログインしました。");
                await MeChatHost.MeChat.Load();
                await Task.Delay(10);
                App.app.MainPage = new NavigationPage(new CustomTabbedPage()
                {
                    Title = "ホーム"
                });
            }
            else
            {

            }

            MessagingCenter.Send(App.app, "dialog_progress", "");
            isRunning = false;

            if (!result) await DisplayAlert("失敗", "idもしくはパスワードが異なる可能性があります。","OK");
        }
	}
}