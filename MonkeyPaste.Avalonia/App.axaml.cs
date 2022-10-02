using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using System.Diagnostics;
using System;
using System.IO;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using Avalonia.Media.Imaging;
using System.Linq;
using Avalonia.Controls.Converters;
using Avalonia.Themes.Fluent;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application {

        public static event EventHandler FrameworkInitialized;
        public static event EventHandler FrameworkShutdown;
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

                desktop.Startup += Startup;
                desktop.Exit += Exit;

                var bootstrapper = new MpAvBootstrapperViewModel();
                await bootstrapper.InitAsync();

            }

            base.OnFrameworkInitializationCompleted();
        }


        private void Startup(object sender, ControlledApplicationLifetimeStartupEventArgs e) {
            FrameworkInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void Exit(object sender, ControlledApplicationLifetimeExitEventArgs e) {
            FrameworkShutdown?.Invoke(this, EventArgs.Empty);
        }

        private void RootIcon_Clicked(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = DateTime.Now - MpAvShortcutCollectionViewModel.Instance.LastRightClickDateTime < TimeSpan.FromMilliseconds(1000);

        }
    }
}
