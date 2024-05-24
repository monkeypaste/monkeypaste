using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaWebView;
using iosTest.ViewModels;
using iosTest.Views;
using PropertyChanged;
using System;
using System.IO;
using System.Reflection;

namespace iosTest {
    [DoNotNotify]
    public partial class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }
        public override void RegisterServices() {
            base.RegisterServices();
            AvaloniaWebViewBuilder.Initialize((config) => {
                var localDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string path = Path.Combine(localDir, "Resources", "Editor");
                config.BrowserExecutableFolder = path;
            });
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow {
                    DataContext = new MainViewModel()
                };
            } else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
                singleViewPlatform.MainView = new MainView {
                    DataContext = new MainViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}