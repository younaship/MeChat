using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MeChat.Page;

namespace MeChat
{

    public class CustomTabbedPage : TabbedPage
    {
        public CustomTabbedPage()
        {
            BackgroundColor = Color.LightGray;

            People people;
            Children.Add(people = new Page.People(MeChatHost.MeChat)
            {
                Title = "People"
                
            });

            Talk talk;
            Children.Add(talk = new Page.Talk(MeChatHost.MeChat)
            {
                Title = "TALK",
                BackgroundColor = Color.Beige
            });

            Children.Add(new Page.Search()
            {
                BackgroundColor = Color.Beige,
                Title = "Search"
            });

            ToolbarItems.Add(new ToolbarItem()
            {
                Text = " 更新 ",
                Command = new Command(() =>
                {
                    people.Refresh();
                    talk.Refresh();
                })
            });

            ToolbarItems.Add(new ToolbarItem()
            {
                Text = " GET ",
                Command = new Command(async()=>
                {
                    App.app.OnProgress("Waiting...");
                    await MeChatHost.MeChat.Load();
                    people.Refresh();
                    talk.Refresh();
                    App.app.OnProgress();

                })
            });

        }

        protected override void OnPagesChanged(NotifyCollectionChangedEventArgs e)
        {
            Debug.Log("On Page Changed.");
            base.OnPagesChanged(e);
        }



    }
}
