using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using KeyGesture = Avalonia.Input.KeyGesture;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSystemTray {
        public static void Init() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            InitStartupTray();
        }
        private static void InitActualTray() {
            var rootIcon = CreateTrayIcon();
            rootIcon.Menu = CreateNativeMenu();

            TrayIcon.SetIcons(Application.Current, new TrayIcons { rootIcon });
        }
        private static void InitStartupTray() {
            var startupIcon = new TrayIcon() {
                ToolTipText = UiStrings.SysTrayPleaseWaitTooltip,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("HourGlassImage", typeof(WindowIcon), null, null) as WindowIcon,
                Menu = new NativeMenu()
            };
            startupIcon.Menu.Add(
                new NativeMenuItem() {
                    Header = UiStrings.CommonCancelLabel,
                    Command = MpAvSystemTrayViewModel.Instance.ExitApplicationCommand,
                    CommandParameter = "systray cancel click"
                });
            TrayIcon.SetIcons(Application.Current, new TrayIcons { startupIcon });
        }

        private static void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.StartupComplete:
                    InitActualTray();
                    break;
            }
        }
        private static TrayIcon CreateTrayIcon() {
            MpAvMenuItemViewModel tmivm = MpAvSystemTrayViewModel.Instance.TrayMenuItemViewModel;
            var rootIcon = new TrayIcon();

            // IS ENABLED

            rootIcon.Bind(
                    NativeMenuItem.IsEnabledProperty,
                    new Binding() {
                        Source = MpAvSystemTrayViewModel.Instance,
                        Path = nameof(MpAvSystemTrayViewModel.Instance.IsSystemTrayItemsEnabled)
                    });

            // TOOLTIP

            if (tmivm.TooltipSrcObj != null) {
                rootIcon.Bind(
                    TrayIcon.ToolTipTextProperty,
                    new Binding() {
                        Source = tmivm.TooltipSrcObj,
                        Path = tmivm.TooltipPropPath
                    });
            } else {
                rootIcon.ToolTipText = tmivm.Tooltip == null ? string.Empty : tmivm.Tooltip.ToString();
            }

            // ICON

            if (tmivm.IconSrcBindingObj != null) {
                rootIcon.Bind(
                    TrayIcon.IconProperty,
                    new Binding() {
                        Source = tmivm.IconSrcBindingObj,
                        Path = tmivm.IconPropPath,
                        Converter = MpAvStringHexToBitmapTintConverter.Instance
                    });
            } else if (tmivm.IconSourceObj != null) {
                rootIcon.Icon = MpAvStringHexToBitmapTintConverter.Instance.Convert(tmivm.IconSourceObj, typeof(WindowIcon), null, null) as WindowIcon;

            }

            // COMMAND

            if (tmivm.CommandSrcObj != null) {
                rootIcon.Bind(
                    TrayIcon.CommandProperty,
                    new Binding() {
                        Source = tmivm.CommandSrcObj,
                        Path = tmivm.CommandPath,
                    });
            } else {
                rootIcon.Command = tmivm.Command;
            }

            // COMMAND PARAMETER

            if (tmivm.CommandParamSrcObj != null) {
                rootIcon.Bind(
                    TrayIcon.CommandParameterProperty,
                    new Binding() {
                        Source = tmivm.CommandParamSrcObj,
                        Path = tmivm.CommandParamPropPath,
                    });
            } else {
                rootIcon.CommandParameter = tmivm.CommandParameter;
            }



            return rootIcon;
        }

        public static NativeMenu CreateNativeMenu() {
            var nm = new NativeMenu();
            // SUB ITEMS

            var nmil = MpAvSystemTrayViewModel.Instance
                .TrayMenuItemViewModel
                .SubItems
                .OfType<MpAvMenuItemViewModel>()
                .Where(x => x.IsVisible)
                .Select(x => CreateMenuItem(x));
            foreach (var nmi in nmil) {
                nm.Add(nmi);
            }
            return nm;
        }

        private static NativeMenuItem CreateMenuItem(MpAvMenuItemViewModel mivm) {
            var nmi = CreateMenuItem(
                isSeparator: mivm.IsSeparator,

                header: mivm.Header,
                headerSrcObj: mivm.HeaderSrcObj,
                headerPath: mivm.HeaderPropPath,

                cmd: mivm.Command,
                cmdSrc: mivm.CommandSrcObj,
                cmdPath: mivm.CommandPath,

                cmdParam: mivm.CommandParameter,
                cmdParamSrc: mivm.CommandParamSrcObj,
                cmdParamPath: mivm.CommandParamPropPath,

                iconSourcObj: mivm.IconSourceObj,
                iconSourcBindingObj: mivm.IconSrcBindingObj,
                iconSrcPath: mivm.IconPropPath,

                isEnabled: mivm.IsEnabled,
                isEnabledBindingObj: mivm.IsEnabledSrcObj,
                isEnabledSrcPath: mivm.IsEnabledPropPath,

                isChecked: mivm.IsChecked,
                isCheckedSrcObj: mivm.IsCheckedSrcObj,
                isCheckedPropPath: mivm.IsCheckedPropPath,
                toggleType: mivm.ToggleType.ToEnum<NativeMenuItemToggleType>(),

                keyGestureStr: mivm.InputGestureText,
                keyGestureSrcObj: mivm.InputGestureSrcObj,
                keyGesturePropPath: mivm.InputGesturePropPath,

                children: mivm.SubItems == null ? null : mivm.SubItems.OfType<MpAvMenuItemViewModel>().Select(x => CreateMenuItem(x)));
            return nmi;
        }

        private static NativeMenuItem CreateMenuItem(
            bool isSeparator = false,

            string header = "",
            object headerSrcObj = null,
            string headerPath = "",

            ICommand cmd = null,
            object cmdSrc = null,
            string cmdPath = "",

            object cmdParam = null,
            object cmdParamSrc = null,
            string cmdParamPath = "",

            object iconSourcObj = null,
            object iconSourcBindingObj = null,
            string iconSrcPath = "",

            bool isEnabled = true,
            object isEnabledBindingObj = null,
            string isEnabledSrcPath = default,

            bool? isChecked = null,
            object isCheckedSrcObj = null,
            string isCheckedPropPath = "",
            NativeMenuItemToggleType toggleType = NativeMenuItemToggleType.None,

            string keyGestureStr = "",
            object keyGestureSrcObj = null,
            string keyGesturePropPath = "",

            IEnumerable<NativeMenuItem> children = null) {
            if (isSeparator) {
                return new NativeMenuItemSeparator();
            }
            var nmi = new NativeMenuItem();
            // IS ENABLED

            if (isEnabledBindingObj != null) {
                nmi.Bind(
                    NativeMenuItem.IsEnabledProperty,
                    new Binding() {
                        Source = isEnabledBindingObj,
                        Path = isEnabledSrcPath
                    });
            } else {
                nmi.IsEnabled = isEnabled;
            }

            // HEADER

            if (headerSrcObj != null) {
                nmi.Bind(
                    NativeMenuItem.HeaderProperty,
                    new Binding() {
                        Source = headerSrcObj,
                        Path = headerPath
                    });
            } else {
                nmi.Header = header;
            }

            // ICON

            if (iconSourcBindingObj != null) {
                nmi.Bind(
                    NativeMenuItem.IconProperty,
                    new Binding() {
                        Source = iconSourcBindingObj,
                        Path = iconSrcPath,
                        Converter = MpAvStringHexToBitmapTintConverter.Instance,
                    });
            } else if (iconSourcObj != null) {
                nmi.Icon = MpAvStringHexToBitmapTintConverter.Instance.Convert(iconSourcObj, null, null, null) as Bitmap;
            }

            // COMMAND

            if (cmdSrc != null) {
                nmi.Bind(
                    NativeMenuItem.CommandProperty,
                    new Binding() {
                        Source = cmdSrc,
                        Path = cmdPath,
                    });
            } else {
                nmi.Command = cmd;
            }

            // COMMAND PARAMETER

            if (cmdParamSrc != null) {
                nmi.Bind(
                    NativeMenuItem.CommandParameterProperty,
                    new Binding() {
                        Source = cmdParamSrc,
                        Path = cmdParamPath,
                    });
            } else {
                nmi.CommandParameter = cmdParam;
            }


            // CHECKED

            if (isCheckedSrcObj != null) {
                nmi.Bind(
                    NativeMenuItem.IsCheckedProperty,
                    new Binding() {
                        Source = isCheckedSrcObj,
                        Path = isCheckedPropPath
                    });
            } else if (isChecked.HasValue) {
                nmi.IsChecked = isChecked.Value;
            }
            nmi.ToggleType = toggleType;

            // KEY GESTURE
            if (keyGestureSrcObj != null) {
                nmi.Bind(
                    NativeMenuItem.GestureProperty,
                    new Binding() {
                        Source = keyGestureSrcObj,
                        Path = keyGesturePropPath,
                        Converter = MpAvKeyStringToKeyGestureConverter.Instance
                    });
            } else {
                KeyGesture inputGesture = null;
                nmi.IsChecked = isChecked.Value;
                if (!string.IsNullOrEmpty(keyGestureStr) &&
                !keyGestureStr.Contains(MpInputConstants.SEQUENCE_SEPARATOR)) {
                    inputGesture = MpAvKeyStringToKeyGestureConverter.Instance.Convert(keyGestureStr, null, null, null) as KeyGesture;
                }
                nmi.Gesture = inputGesture;
            }


            // SUB ITEMS

            if (children != null) {
                if (nmi.Menu == null) {
                    nmi.Menu = new NativeMenu();
                }
                foreach (var cnmi in children) {
                    nmi.Menu.Items.Add(cnmi);
                }
            }
            return nmi;
        }
    }
}
