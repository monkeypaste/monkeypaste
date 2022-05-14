using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using Xamarin.Forms.Xaml;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application {
        public static bool StoreScreenshot = false;

        public MpIPlatformWrapper NativeInterfaceWrapper => MpPlatformWrapper.Services;

        public App() : this(null) { }

        public App(MpIPlatformWrapper niw)  {            
            InitializeComponent();
            //MainPage = new MpMainShell(niw);
            MpMainDisplayInfo.Init();

            MainPage = new MpMainPage(niw);
            //string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);// @"/storage/emulated/0/Download/"
            //string path = System.IO.Path.Combine(folder, string.Format(@"index.html"));

            //var html = MpHelpers.LoadFileResource("MonkeyPaste.Resources.Html.Editor.index.html");
            //MpHelpers.WriteTextToFile(path, html);
            //MpConsole.WriteLine(@"Editor written to: " + path);

            //path = path.Replace("index.html", "Editor2.js");
            //html = MpHelpers.LoadFileResource("MonkeyPaste.Resources.Html.Editor.Editor2.js");
            //MpHelpers.WriteTextToFile(path, html);
            //MpConsole.WriteLine(@"Editor js written to: " + path);
        }

        protected override void OnStart() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Starting----------" + Environment.NewLine);
        }
        protected override void OnSleep() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Sleeping----------" + Environment.NewLine);
            //Task.Run(async () => {
            //    if (MpMainShell.IsLoaded) {
            //        byte[] ss = null;
            //        while (true) {
            //            if()
            //            ss = NativeInterfaceWrapper.GetScreenshot().Capture();

            //            if (StoreScreenshot == true) {
            //                string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);// @"/storage/emulated/0/Download/"
            //                string path = System.IO.Path.Combine(folder, string.Format(@"screen{0}.png", DateTime.Now.ToString()));

            //                MpHelpers.WriteByteArrayToFile(path, ss);
            //            }


            //            var imgSrc = new MpImageConverter().Convert(ss, typeof(ImageSource)) as ImageSource;
            //            var img = new Image() {
            //                Source = imgSrc
            //            };
            //            MpConsole.WriteLine(@"Screen captured at: " + DateTime.Now.ToString());
            //            MpConsole.WriteLine(string.Format(@"Dimensions: {0} x {1}", img.Width, img.Height));
            //            await Task.Delay(1000);
            //        }
            //    }
            //});
        }

        protected override void OnResume() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Resuming----------" + Environment.NewLine);
        }
    }
}
