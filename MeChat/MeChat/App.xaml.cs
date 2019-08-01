using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace MeChat
{
	public partial class App : Application
	{
        public static App app { private set; get; }

		public App ()
		{
            app = this;
			InitializeComponent();

            if (!MeChatHost.Awake())
                MainPage = new NavigationPage(new Page.LoginPage()) { Title = "ログイン" };
            else
            {
                MainPage = new ContentPage();
                Awake();
            }
                
        }

        public async void Awake()
        {
            await MeChatHost.MeChat.Load();

            Debug.Log("Load Home.");
            MainPage = new NavigationPage(new CustomTabbedPage()
            {
                Title = "ホーム"
            });
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}

        /// <summary>
        /// メッセージで表示、空で非表示
        /// </summary>
        public void OnProgress(string message = "")
        {
            MessagingCenter.Send(this, "dialog_progress", message);
        }
    }
}
