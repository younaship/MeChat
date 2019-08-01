using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MeChat
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Main_Tab : TabbedPage
    {
        public Main_Tab ()
        {
            InitializeComponent();

            Children.Add(new Page.Talk(MeChatHost.MeChat)
            {
                Title = "People"

            });

            Children.Add(new ContentPage()
            {
                Title = "TALK",
                BackgroundColor = Color.Beige
            });

            Children.Add(new ContentPage()
            {
                BackgroundColor = Color.Yellow,
                Title = "Search"
            });
        }
    }
}