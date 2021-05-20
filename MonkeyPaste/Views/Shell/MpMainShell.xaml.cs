using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Threading;

namespace MonkeyPaste
{
    public partial class MpMainShell : Shell
    {
        public MpMainShell()
        {
            InitializeComponent();

            //var collectionSection = new ShellSection() {
            //    Title = "Collections",
            //    Icon = App.Current.Resources["TagsIcon"] as ImageSource
            //};
            //collectionSection.Items.Add(new ShellContent() {
            //    Content = new MpMainView()
            //});

            //FlyoutItem.Items.Add(collectionSection);
            Device.BeginInvokeOnMainThread(Shell_Appearing);
        }

        private async void Shell_Appearing() {
            //var db = MpResolver.Resolve<MpDb>();

            //while (!db.IsLoaded) {
            //    Thread.Sleep(50);
            //}


            var tagList = await MpDb.Instance.GetItems<MpTag>();
            tagList = tagList.OrderBy(x => x.TagSortIdx).ToList();
            foreach (var tag in tagList) {
                var tagIcon = App.Current.Resources["SquareIcon"+tagList.IndexOf(tag)] as FontImageSource;
                tagIcon.Color = GetItemColor(tag.TagName);
                var tagSection = new ShellSection() {
                    Title = tag.TagName,
                    Icon = tagIcon
                };
                tagSection.Items.Add(new ShellContent() {
                    Content = new MpMainView()
                });
                FlyoutItem.Items.Add(tagSection);
            }

            var settingsSection = new ShellSection() {
                Title = "Settings",
                Icon = App.Current.Resources["SettingsIcon"] as ImageSource
            };
            settingsSection.Items.Add(new ShellContent() {
                Content = new MpSettingsPageView()
            });
            FlyoutItem.Items.Add(settingsSection);

            var helpSection = new ShellSection() {
                Title = "Help",
                Icon = App.Current.Resources["HelpIcon"] as ImageSource
            };
            helpSection.Items.Add(new ShellContent() {
                Content = new MpHelpPageView()
            });
            FlyoutItem.Items.Add(helpSection);
        }

        private Color GetItemColor(string tagName) {
            return tagName switch {
                "Recent" => Color.Green,
                "All" => Color.Blue,
                "Favorites" => Color.Yellow,
                "Help" => Color.Orange,
                _ => Color.Pink
            };
        }
    }
}
