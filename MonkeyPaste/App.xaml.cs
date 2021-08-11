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
        public static bool StoreScreenshot = false;
        
        public MpINativeInterfaceWrapper NativeInterfaceWrapper { get; set; }

        public App() {
            InitializeComponent();
            MainPage = new MpMainShell();
        }

        public App(MpINativeInterfaceWrapper niw)  {            
            InitializeComponent();
            NativeInterfaceWrapper = niw;
            MainPage = new MpMainShell(niw);
        }

        protected override void OnStart() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Starting----------" + Environment.NewLine);
        }
        protected override void OnSleep() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Sleeping----------" + Environment.NewLine);
            Task.Run(async () => {
                if (MpMainShell.IsLoaded) {
                    byte[] ss = null;
                    while (true) {
                        if(StoreScreenshot == true) {

                        }
                        ss = NativeInterfaceWrapper.GetScreenshot().Capture();
                        var imgSrc = new MpImageConverter().Convert(ss, typeof(ImageSource)) as ImageSource;
                        var img = new Image() {
                            Source = imgSrc
                        };
                        MpConsole.WriteLine(@"Screen captured at: " + DateTime.Now.ToString());
                        MpConsole.WriteLine(string.Format(@"Dimensions: {0} x {1}", img.Width, img.Height));
                        await Task.Delay(1000);
                    }
                }
            });
        }

        protected override void OnResume() {
            MpConsole.WriteTraceLine(Environment.NewLine + @"-------------Application Resuming----------" + Environment.NewLine);
        }
    }
}
