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
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application {
        public static IClassicDesktopStyleApplicationLifetime Desktop { get; private set; }
        public App() {
            DataContext = MpAvSystemTrayViewModel.Instance;
        }
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        private void AppTrayIcon_Clicked(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
            return;
        }

        public override async void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                Desktop = desktop;
                
                //MpAvCefNetWebViewExtension.InitCefNet(desktop);
                MpAvCefWebViewExtension.InitCef();

                if (OperatingSystem.IsLinux()) {
                    await GtkHelper.EnsureInitialized();
                } else if (OperatingSystem.IsMacOS()) {
                    MpAvMacHelpers.EnsureInitialized();
                }

                await MpAvWrapper.Instance.InitializeAsync();
                await MpPlatformWrapper.InitAsync(MpAvWrapper.Instance);
                var bootstrapper = new MpAvBootstrapperViewModel();
                await bootstrapper.InitAsync();

                MpConsole.WriteLine("Loaded");

                //desktop.MainWindow.Close();
                desktop.Exit += Desktop_Exit;
                desktop.MainWindow = new MpAvMainWindow();
                desktop.MainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_Exit(object sender, ControlledApplicationLifetimeExitEventArgs e) {
            MpAvCefNetWebViewExtension.ShutdownCefNet();            
        }
    }
}
