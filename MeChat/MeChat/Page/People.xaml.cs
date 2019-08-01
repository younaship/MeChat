using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MeChat.Page
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class People : ContentPage , IMyInTabPage
    {
        MeChat MeChat;
        public People(MeChat meChat)
        {
            MeChat = meChat;
            InitializeComponent();
            Refresh();
            Debug.Log("People OK.");
        }

        private ListView GetListView()
        {
            var users = MeChatHost.MeChat.UserProfileHost.UserProfiles;

            List<ImageCell> cells = new List<ImageCell>();

            foreach (var v in users)
                cells.Add(GetCell(v));

            ListView ls = new ListView() { ItemsSource = cells, ItemTemplate = new DataTemplate(typeof(ImageCell)), RowHeight = 80 };
            ls.ItemTemplate.SetBinding(ImageCell.TextProperty, "Text");
            ls.ItemTemplate.SetBinding(ImageCell.DetailProperty, "Detail");
            ls.ItemTemplate.SetBinding(ImageCell.ImageSourceProperty, "ImageSource");

            ls.ItemTapped += (o, e) => { Debug.Log("Tapped " + o); Selected((ImageCell)e.Item); };
            

            return ls;
        }

        private ImageCell GetCell(UserProfile user)
        {
            ImageCell cell = new ImageCell()
            {
                Detail = user.Summary,
                Text = user.Name,
            };

            cell.ImageSource = MeChatHost.MeChat.MediaCenter.GetImage(user.Icon, (x) => {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    Debug.Log("People", "Change Image On Main Thread.");
                    cell.ImageSource = x;
                });
            });

            if (user.Id == MeChatHost.MeChat.Config.Id)
                cell.TextColor = Color.Green;

            return cell;
        }

        private void Selected(ImageCell cell)
        {
            var item = MeChatHost.MeChat.UserProfileHost.UserProfiles.Find((x) => x.Name == cell.Text);
            Navigation.PushAsync(new ProfilePage(item) { Title = "Profile" }, true);
        }

        public void Refresh()
        {
            ListView view;
            Stack_Main = new StackLayout();
            Content = ((view = GetListView()));
        }
    }

}