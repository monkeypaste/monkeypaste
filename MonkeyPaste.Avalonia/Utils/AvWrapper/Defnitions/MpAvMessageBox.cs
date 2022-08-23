using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Layout;

namespace MonkeyPaste.Avalonia {
    public class MpAvMessageBox : MpINativeMessageBox {
        private object result;

        public bool ShowOkCancelMessageBox(string title, string message) {

            Dispatcher.UIThread.Post(async () => {
                var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(title, message);
                result = await messageBoxStandardWindow.Show();

                //var window = CreateSampleWindow(title,message);
                //window.Height = 200;
                //_ = window.ShowDialog(window);
            });

            while(result == null) {
                Thread.Sleep(100);
            }

            //if(result is string resultStr) {
            //    return resultStr.ToLower() == "ok";
            //}
            //return false;
            if (result is ButtonResult br) {
                result = null;
                return br == ButtonResult.Ok;
            }
            result = null;
            return false;
        }

        public bool? ShowYesNoCancelMessageBox(string title, string message) {
            object result = null;

            Dispatcher.UIThread.Post(async () => {
                var iconBitmap = MpAvStringResourceToBitmapConverter.Instance.Convert(
                    MpPlatformWrapper.Services.PlatformResource.GetResource("QuestionMarkImage"), null, null, null) as Bitmap;
                
                var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams {
                    ButtonDefinitions = ButtonEnum.YesNoCancel,
                    ContentTitle = title,
                    ContentHeader = message,
                    ContentMessage = String.Empty,
                    WindowIcon = new WindowIcon(iconBitmap)
                });
                result = await messageBoxStandardWindow.Show();
            });

            while (result == null) {
                Thread.Sleep(100);
            }

            if (result is ButtonResult br) {
                if(br == ButtonResult.Cancel) {
                    return null;
                }
                return br == ButtonResult.Yes;
            }
            return null;
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
