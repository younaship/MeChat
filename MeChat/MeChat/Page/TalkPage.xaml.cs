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
	public partial class TalkPage : ContentPage
	{
        public TalkData TalkData { private set; get; }
        MeChat MeChat;

        public TalkPage(MeChat meChat, TalkData talkData)
		{
            this.MeChat = meChat;
            TalkData = talkData;
			InitializeComponent ();
            foreach (TalkData.Cell cell in TalkData.cells) AddCell(cell);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MoveToLast(false);
            MeChat.TalkDataHost.OnAddCell += OnEvent;
        }

        protected override void OnDisappearing()
        {
            MeChat.TalkDataHost.OnAddCell -= OnEvent;
            base.OnDisappearing();
        }

        private void OnEvent(TalkData.Cell cell)
        {
            Debug.Log("TalkView", "On Event cell");
            Device.BeginInvokeOnMainThread(() => {
                AddCell(cell);
                MoveToLast(true);
            });
        }


        public int day;
        /// <summary>
        /// トークセルをビューに追加します。
        /// </summary>
        public View AddCell(TalkData.Cell cell)
        {
            DateTime date = DateTime.Parse(cell.Time);
            if (date.Day != day)
            {
                day = date.Day;
                Stack_Main.Children.Add(GetViewDate(date.ToString("M/d")));
            }

            StackLayout stk = new StackLayout();
            stk.Orientation = StackOrientation.Vertical;
            stk.Margin = 5;
            stk.MinimumWidthRequest = 100;

            Label label = new Label()
            {
                Margin = new Thickness(5, 2),
                Text = cell.Message,
                LineBreakMode = LineBreakMode.CharacterWrap
            };
            if (!cell.IsMe)
            {
                stk.HorizontalOptions = LayoutOptions.Start;
                stk.BackgroundColor = Color.SkyBlue;
            }
            else
            {
                stk.HorizontalOptions = LayoutOptions.End;
                stk.BackgroundColor = Color.Pink;
            }

            stk.Children.Add(label);

            stk.Children.Add(new Label()
            {
                Margin = new Thickness(5,2),
                FontSize = 11,
                TextColor = Color.Gray,
                Text = date.ToString("HH:mm"),
                HorizontalOptions = LayoutOptions.End
            });

            Stack_Main.Children.Add(stk);
            return label;
        }

        private View GetViewDate(string date)
        {
            return new Label()
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = 3,
                TextColor = Color.Gray,
                Text = date
            };
        }

        // メッセージ送信アクション
        private void Button_Clicked(object sender, EventArgs e)
        { 
            RunSend();
        }

        bool isEnableSend = true;
        private async void RunSend()
        {
            Editor_Message.IsEnabled = false;

            if (!isEnableSend) return;
            else isEnableSend = false;

            string message = Editor_Message.Text;
            var res = await MeChat.SendMessage(TalkData.ToId, message);
            if (res != null) // 送信成功
            {
                AddCell(res);
                Editor_Message.Text = "";
                MoveToLast(true);
            }

            Editor_Message.IsEnabled = true;
            isEnableSend = true;
            return;
        }

        public async void MoveToLast(bool anim)
        {
            var last = Stack_Main.Children.Last();
            await ScrollView.ScrollToAsync(last, ScrollToPosition.End, anim);
        }
    }
}