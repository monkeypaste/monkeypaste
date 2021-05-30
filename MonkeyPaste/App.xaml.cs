using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application {      
        public App() {
            MpTempFileManager.Instance.Init();
            Device.BeginInvokeOnMainThread(async () => { await MpDb.Instance.Init(); });
            InitializeComponent();
            MainPage = new MpMainShell();
        }

        protected override void OnStart() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Starting----------" + Environment.NewLine);
        }
        protected override void OnSleep() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Sleeping----------" + Environment.NewLine);
        }

        protected override void OnResume() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Resuming----------" + Environment.NewLine);
        }
    }
}
