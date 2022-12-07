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
using KeyGesture = Avalonia.Input.KeyGesture;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSystemTray {
        public static void Init() {
            var tray_icons = CreateTrayIcons();
            TrayIcon.SetIcons(Application.Current, tray_icons);

        }


        public static TrayIcons CreateTrayIcons() {
            var rootIcon = CreateTrayIcon();
            rootIcon.Menu = CreateNativeMenu();

            var trayIcons = new TrayIcons {
                rootIcon
            };
            return trayIcons;
        }

        private static TrayIcon CreateTrayIcon() {
            MpMenuItemViewModel tmivm = MpAvSystemTrayViewModel.Instance.TrayMenuItemViewModel;
            var rootIcon = new TrayIcon() {
                //Menu = new NativeMenu()
            };

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
                        Converter = MpAvIconSourceObjToBitmapConverter.Instance
                    });
            } else if (tmivm.IconSourceObj != null) {
                rootIcon.Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert(tmivm.IconSourceObj, null, null, null) as WindowIcon;

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

            var nmil = MpAvSystemTrayViewModel.Instance.TrayMenuItemViewModel.SubItems.Where(x => x.IsVisible).Select(x => CreateMenuItem(x));
            foreach (var nmi in nmil) {
                nm.Add(nmi);
            }
            return nm;
        }

        private static NativeMenuItem CreateMenuItem(MpMenuItemViewModel mivm) {
            var nmi = CreateMenuItem(
                header: mivm.IsSeparator ? "-": mivm.Header,
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

                isChecked: mivm.IsChecked,
                isCheckedSrcObj: mivm.IsCheckedSrcObj,
                isCheckedPropPath: mivm.IsCheckedPropPath,
                toggleType: mivm.ToggleType.ToEnum<NativeMenuItemToggleType>(),

                keyGestureStr: mivm.InputGestureText,

                children: mivm.SubItems == null ? null : mivm.SubItems.Select(x => CreateMenuItem(x)));
            return nmi;
        }

        private static NativeMenuItem CreateMenuItem(
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

            bool? isChecked = null,
            object isCheckedSrcObj = null,
            string isCheckedPropPath = "",
            NativeMenuItemToggleType toggleType = NativeMenuItemToggleType.None,

            string keyGestureStr = "",

            IEnumerable<NativeMenuItem> children = null) {
            var nmi = new NativeMenuItem();
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
                        Converter = MpAvIconSourceObjToBitmapConverter.Instance
                    });
            } else if (iconSourcObj != null) {
                nmi.Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert(iconSourcObj, null, null, null) as Bitmap;
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
            } else if(isChecked.HasValue) {
                nmi.IsChecked = isChecked.Value;
            }
            nmi.ToggleType = toggleType;

            // KEY GESTURE
            KeyGesture inputGesture = null;
            if (!string.IsNullOrEmpty(keyGestureStr)) {
                inputGesture = KeyGesture.Parse(keyGestureStr);
            }
            nmi.Gesture = inputGesture;

            // SUB ITEMS

            if (children != null) {
                if(nmi.Menu == null) {
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
