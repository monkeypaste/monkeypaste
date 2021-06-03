using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application {
        public App() {
            MpTempFileManager.Instance.Init();
            InitializeComponent();
            MainPage = new MpMainShell();
        }

        protected override void OnStart() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Starting----------" + Environment.NewLine);
        }
        protected override void OnSleep() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Sleeping----------" + Environment.NewLine);

            //var msv = Application.Current.MainPage as MpMainShell;
            //if(msv.SettingsPageView != null) {
            //    if(msv.SettingsPageView.ChatPageView != null) {
            //        var cpvm = msv.SettingsPageView.ChatPageView.BindingContext as MpChatPageViewModel;
            //        var chatService = cpvm.ChatService;
            //        chatService.Dispose();
            //    }
            //}
        }

        protected override void OnResume() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Resuming----------" + Environment.NewLine);

            //var msv = Application.Current.MainPage as MpMainShell;
            //if (msv.SettingsPageView != null) {
            //    if (msv.SettingsPageView.ChatPageView != null) {
            //        var cpvm = msv.SettingsPageView.ChatPageView.BindingContext as MpChatPageViewModel;
            //        var chatService = cpvm.ChatService;
            //        Task.Run(async () => {
            //            if (!chatService.IsConnected) {
            //                await chatService.CreateConnection();
            //            }
            //        });

            //        Page view = null;
            //        if (MpViewModelBase.User != null) {
            //            view = msv.SettingsPageView.ChatPageView;
            //        } else {
            //            view = msv;
            //        }
            //        //var navigationPage = new NavigationPage(view);
            //        MainPage = view;
            //    }
            //}
        }
    }
}
