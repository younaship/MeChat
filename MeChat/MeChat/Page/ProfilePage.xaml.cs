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
	public partial class ProfilePage : ContentPage
	{
        private const string DEFAULT_HEADER = "MeChat.Droid.img.sample.sample_header.jpg";
        public UserProfile UserProfile { private set; get; }

		public ProfilePage (UserProfile profile)
		{
            UserProfile = profile;
			InitializeComponent ();

            SetView();
		}

        public void SetView()
        {
            Label_Name.Text =  UserProfile.Name;
            Label_Summary.Text =  UserProfile.Summary;
            Image_Header.Source = MeChatHost.MeChat.MediaCenter.GetImage(UserProfile.Header, (x) =>
            {
                Image_Header.Source = x;
            }, DEFAULT_HEADER);
            Image_Icon.Source = MeChatHost.MeChat.MediaCenter.GetImage(UserProfile.Icon, (x) =>
            {
                Image_Header.Source = x;
            });
        }
	}
}