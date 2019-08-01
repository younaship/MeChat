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
	public partial class Talk : ContentPage , IMyInTabPage
	{
        class ImageCellEx : ImageCell
        {
            public string uId;
        }

        MeChat MeChat;
        public Talk (MeChat MeChat)
		{
            this.MeChat = MeChat;
			InitializeComponent();
        }


        private void OnEvent(TalkData.Cell cell)
        {
            Debug.Log("TalkView", "On Event cell");
            Device.BeginInvokeOnMainThread(() => Refresh());
        }

        protected override void OnAppearing()
        {
            Refresh();
            base.OnAppearing();
            MeChat.TalkDataHost.OnAddCell += OnEvent;
        }

        protected override void OnDisappearing()
        {
            MeChat.TalkDataHost.OnAddCell -= OnEvent;
            base.OnDisappearing();
        }

        private void Selected(ImageCellEx cell)
        {
            var item = MeChatHost.MeChat.TalkDataHost.GetById(cell.uId);
            Navigation.PushAsync(new TalkPage(this.MeChat,item) { Title = MeChat.UserProfileHost.GetNameById(cell.uId) }, true);
        }

        private ListView GetListView()
        {
            List<ImageCellEx> cells = new List<ImageCellEx>();
            foreach (var v in MeChatHost.MeChat.TalkDataHost.Datas)
                if (v.cells.Count > 0) cells.Add(GetCell(v.ToId, MeChat.UserProfileHost.GetNameById(v.ToId), v.GetLastMessage(), MeChat.UserProfileHost.GetIconById(v.ToId)));

            ListView ls = new ListView() { ItemsSource = cells, ItemTemplate = new DataTemplate(typeof(ImageCellEx)) };
            ls.ItemTemplate.SetBinding(ImageCell.TextProperty, "Text");
            ls.ItemTemplate.SetBinding(ImageCell.DetailProperty, "Detail");
            ls.ItemTemplate.SetBinding(ImageCell.ImageSourceProperty, "ImageSource");

            ls.ItemTapped += (o, e) => { Debug.Log("Tapped " + o); Selected((ImageCellEx)e.Item); };

            return ls;
        }

        private ImageCellEx GetCell(string id,string name,string message,string icon)
        {
            ImageCellEx ex = null;
            return ex = new ImageCellEx()
            {
                ImageSource = MeChat.MediaCenter.GetImage(icon, (x) => {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                    {
                        Debug.Log("TalkList", "Change Image On Main Thread.");
                        ex.ImageSource = x;
                    });
                }),
                Detail = message,
                Text = name,
                uId = id
            };
        }

        ListView view;
        public void Refresh()
        {
            Content = ((view = GetListView()));
        }
    }

    public class TalkCell_DATA
    {
        public TalkCell_DATA(string icon, string name, string detail) { Name = name; Detail = detail; Icon = icon; }
        public string Name;
        public string Detail;
        public string Icon;
    }
}