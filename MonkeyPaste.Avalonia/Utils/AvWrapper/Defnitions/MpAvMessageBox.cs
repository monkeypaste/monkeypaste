using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Layout;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvMessageBox : MpINativeMessageBox {
        private object result;
        public async Task<bool> ShowOkCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null) {
            var result = await MpNotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalOkCancelMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor);

            if (result == MpNotificationDialogResultType.Ok) {
                return true;
            }

            if (result != MpNotificationDialogResultType.Cancel) {
                // result type mismatch
                Debugger.Break();
            }
            return false;
            //            if(OperatingSystem.IsWindows()) {
            //#if WINDOWS
            //                System.Windows.MessageBox.Show(message, title);
            //#endif
            //            } else {
            //                // add others
            //                Debugger.Break();
            //            }
            //            return false;
        }

        public async Task<bool?> ShowYesNoCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null) {
            var result = await MpNotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalYesNoCancelMessageBox,
                                    title: title,
                                    body: message,
                                    iconSourceObj: iconResourceObj,
                                    anchor: anchor);

            if (result == MpNotificationDialogResultType.Yes) {
                return true;
            }
            if (result == MpNotificationDialogResultType.No) {
                return false;
            }

            if(result != MpNotificationDialogResultType.Cancel) {
                // result type mismatch
                Debugger.Break();
            }
            return null;

//            if (OperatingSystem.IsWindows()) {
//#if WINDOWS
//                var result = System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNoCancel);
//                if(result == System.Windows.MessageBoxResult.Yes) {
//                    return true;
//                }
//                if(result == System.Windows.MessageBoxResult.No) {
//                    return false;
//                }
//#endif
//                return null;
//            } else {
//                // fill in others
//                Debugger.Break();
//            }
//            return null;
        }

        private Window CreateSampleWindow(string title, string message) {
            Button cancelButton;
            Button okButton;

            var window = new Window {
                Title = title,
                Height = 200,
                Width = 200,
                Content = new StackPanel {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = message },
                        new StackPanel() {
                            Orientation = Orientation.Horizontal,
                            Children = {
                                (cancelButton = new Button
                                {
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Content = "Cancel",
                                    IsDefault = false
                                }),
                                (okButton = new Button
                                {
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Content = "Ok",
                                    IsDefault = true
                                })
                            }
                        }
                    }
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            cancelButton.Click += (s, e) => {
                result = "Cancel";
            };
            okButton.Click += (s, e) => {
                result = "Cancel";
            };

            return window;
        }
    }
}
