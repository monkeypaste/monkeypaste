using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using MonkeyPaste.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data;
using System.Windows.Input;
using System;
using Avalonia;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSystemTray {
        public static void Init() {
            CreateTrayIcon();
        }


        private static void CreateTrayIcon() {
            var trayIcons = new TrayIcons();

            var rootIcon = new TrayIcon() {
                Icon = new WindowIcon(
                    MpAvStringResourceConverter.Instance.Convert(
                        MpPlatformWrapper.Services.PlatformResource.GetResource("AppTrayIcon"), null, null, null) as Bitmap),
                //Command = MpAvMainWindowViewModel.Instance.ShowWindowCommand,
                ToolTipText = MpPrefViewModel.Instance.ApplicationName,
                Menu = new NativeMenu()
            };
            rootIcon.Bind(
                TrayIcon.CommandProperty,
                new Binding() {
                    Source = MpAvMainWindowViewModel.Instance,
                    Path = nameof(MpAvMainWindowViewModel.Instance.ShowWindowCommand)
                });

            rootIcon.Menu.Opening += Menu_Opening;
            rootIcon.Menu.Closed += Menu_Closed;

            trayIcons.Add(rootIcon);

            var open_nmi = new NativeMenuItem("_Open");
            open_nmi.Bind(
                NativeMenuItem.CommandProperty, new Binding() {
                    Source = MpAvMainWindowViewModel.Instance,
                    Path = nameof(MpAvMainWindowViewModel.Instance.ShowWindowCommand)
                });

            var settings_nmi = new NativeMenuItem("_Settings");
            settings_nmi.Bind(
                NativeMenuItem.CommandProperty, new Binding() {
                    Source = MpAvSettingsWindowViewModel.Instance,
                    Path = nameof(MpAvSettingsWindowViewModel.Instance.ShowSettingsWindowCommand)
                });

            var show_converter_dev_tools_nmi = new NativeMenuItem("Show Converter DevTools");
            show_converter_dev_tools_nmi.Bind(
                NativeMenuItem.CommandProperty, new Binding() {
                    Source = MpAvPlainHtmlConverter.Instance,
                    Path = nameof(MpAvPlainHtmlConverter.Instance.ShowConverterDevTools)
                });

            var sep_mi = new NativeMenuItem("-");

            var exit_nmi = new NativeMenuItem("_Exit");
            exit_nmi.Bind(
                NativeMenuItem.CommandProperty, new Binding() {
                    Source = MpAvSystemTrayViewModel.Instance,
                    Path = nameof(MpAvSystemTrayViewModel.Instance.ExitApplicationCommand)
                });

            rootIcon.Menu.Add(open_nmi);
            rootIcon.Menu.Add(settings_nmi);
            rootIcon.Menu.Add(show_converter_dev_tools_nmi);
            rootIcon.Menu.Add(sep_mi);
            rootIcon.Menu.Add(exit_nmi);
        }
        private static void Menu_Closed(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
        }

        private static void Menu_Opening(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
        }
    }
}
