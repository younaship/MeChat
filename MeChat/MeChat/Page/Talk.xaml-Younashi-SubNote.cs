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
	public partial class Talk : ContentPage
	{
        class ImageCellEx : ImageCell
        {
            public string uId;
        }

        public Talk ()
		{
			InitializeComponent ();

            /*
            List<ImageCellEx> cells = new List<ImageCellEx>();
            foreach (var v in MeChatHost.MeChat.Talks) cells.Add(new ImageCellEx() {
                ImageSource = ImageSource.FromResource(MeChatHost.MeChat.Hos.Find((x) => x.Id == v.ToId).Icon),
                Detail = v.cells.Last().Message,
                Text = MeChatHost.MeChat.Friends.Find((x) => x.Id == v.ToId).Name,
                uId = v.ToId
            });

            ListView ls = new ListView() { ItemsSource = cells , ItemTemplate = new DataTemplate(typeof(ImageCellEx)) };
            ls.ItemTemplate.SetBinding(ImageCellEx.TextProperty, "Text");
            ls.ItemTemplate.SetBinding(ImageCellEx.DetailProperty, "Detail");
            ls.ItemTemplate.SetBinding(ImageCellEx.ImageSourceProperty, "ImageSource");
            

            ls.ItemTapped += (o, e) => { Debug.Log("Tapped " + o); Selected((ImageCellEx)e.Item); };

            Content = ls;*/
        }

        private void Selected(ImageCellEx cell)
        {
            var item = MeChatHost.MeChat.Talks.Find((x) => x.ToId == cell.uId);
            Navigation.PushAsync(new TalkPage(item) { Title = "Talk" }, true);
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