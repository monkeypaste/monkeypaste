using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using System.Diagnostics;
using System;
using System.IO;
using WebViewControl;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application {
        public static IClassicDesktopStyleApplicationLifetime Desktop { get; private set; }
        public App() {
            DataContext = MpAvAppViewModel.Instance;
        }
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        private void AppTrayIcon_Clicked(object sender, EventArgs e) {
            return;
        }

        public override async void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                Desktop = desktop;

                string cefLogPath = Path.Combine(Environment.CurrentDirectory, "ceflog.txt");
                if(File.Exists(cefLogPath)) {
                    File.Delete(cefLogPath);
                }

                WebView.Settings.OsrEnabled = true;
                WebView.Settings.LogFile = "ceflog.txt";
                //WebView.Settings.EnableErrorLogOnly = true;

                
                string cefCacheDir = Path.Combine(Environment.CurrentDirectory, "cefcache");
                if (Directory.Exists(cefCacheDir)) {
                    //Directory.CreateDirectory(cefCacheDir);
                    Directory.Delete(cefCacheDir);
                }
                //WebView.Settings.CachePath = cefCacheDir;


                if (OperatingSystem.IsLinux()) {
                    await GtkHelper.EnsureInitialized();
                } else if (OperatingSystem.IsMacOS()) {
                    MpAvMacHelpers.EnsureInitialized();
                }


                await MpAvWrapper.Instance.InitializeAsync();
                await MpPlatformWrapper.InitAsync(MpAvWrapper.Instance);
                var bootstrapper = new MpAvBootstrapperViewModel();
                await bootstrapper.InitAsync();

                //desktop.MainWindow.Close();

                desktop.MainWindow = new MpAvMainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}
