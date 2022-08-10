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
using Avalonia.Media.Imaging;
using System.Linq;
using Avalonia.Controls.Converters;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application {
        public static IClassicDesktopStyleApplicationLifetime Desktop { get; private set; }
        public App() {
            if(MpClipTileContentDataTemplateSelector.UseCefNet) {
                MpCefNetApplication.ResetEnv();
            }
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

                if (MpClipTileContentDataTemplateSelector.UseCefNet) {
                    MpCefNetApplication.InitCefNet(desktop);
                } else {
                    MpAvCefWebViewExtension.InitCef();
                }

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
                desktop.MainWindow = new MpAvMainWindow();

                CreateTrayIcon();

                desktop.MainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }
        private void CreateTrayIcon() {
            var trayIcons = new TrayIcons();
            trayIcons.Add(new TrayIcon() {
                Icon = new WindowIcon(
                    MpAvStringResourceToBitmapConverter.Instance.Convert(
                        MpPlatformWrapper.Services.PlatformResource.GetResource("AppImage"), null, null, null) as Bitmap),
                Command = MpAvMainWindowViewModel.Instance.ShowWindowCommand,
                ToolTipText = MpPrefViewModel.Instance.ApplicationName,
                Menu = new NativeMenu()
            });
            var mil = new[] {
                new NativeMenuItem() {
                            Header = "_Open",
                            Command = MpAvMainWindowViewModel.Instance.ShowWindowCommand
                        },
                        new NativeMenuItem() {
                            Header = "_Settings",
                            Command = MpAvSettingsWindowViewModel.Instance.ShowSettingsWindowCommand
                        },
                        new NativeMenuItem() {
                            Header = "-"
                        },
                        new NativeMenuItem() {
                            Header = "_Exit",
                            Command = MpAvSystemTrayViewModel.Instance.ExitApplicationCommand
                        }
            };
            mil.ForEach(x => trayIcons.First().Menu.Items.Add(x));            
        }
    }
}
