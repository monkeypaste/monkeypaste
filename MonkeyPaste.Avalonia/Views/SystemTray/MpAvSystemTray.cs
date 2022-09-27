using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using MonkeyPaste.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSystemTray {
        public static void Init() {
            CreateTrayIcon();
        }


        private static void CreateTrayIcon() {
            var trayIcons = new TrayIcons();
            var rootIcon = new TrayIcon() {
                Icon = new WindowIcon(
                    MpAvStringResourceToBitmapConverter.Instance.Convert(
                        MpPlatformWrapper.Services.PlatformResource.GetResource("AppImage"), null, null, null) as Bitmap),
                Command = MpAvMainWindowViewModel.Instance.ShowWindowCommand,
                ToolTipText = MpPrefViewModel.Instance.ApplicationName,
                Menu = new NativeMenu()
            };
            rootIcon.Menu.Opening += Menu_Opening;
            rootIcon.Menu.Closed += Menu_Closed;
            //rootIcon.Clicked += RootIcon_Clicked;
            trayIcons.Add(rootIcon);
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
                    Header = "Show Converter DevTools",
                    Command = MpAvHtmlClipboardDataConverter.ShowConverterDevTools
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


        private static void Menu_Closed(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
        }

        private static void Menu_Opening(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
        }
    }
}
