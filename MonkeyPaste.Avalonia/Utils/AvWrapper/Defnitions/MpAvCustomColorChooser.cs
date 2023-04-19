using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvCustomColorChooser : MpICustomColorChooserMenuAsync {
        public ICommand SelectCustomColorCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is MpIUserColorViewModel ucvm) {
                    string selectedColor = ucvm.UserHexColor;
                    string newColor = await ShowCustomColorMenuAsync(selectedColor, null, ucvm);
                }
            });


        public async Task<string> ShowCustomColorMenuAsync(string selectedColor, string title = null, MpIUserColorViewModel ucvm = null, object owner = null) {

            var cw = new MpAvWindow() {
                DataContext = ucvm,
                Topmost = true,
                Title = "Color Chooser".ToWindowTitleText(),
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("ColorsImage", typeof(WindowIcon), null, null) as WindowIcon,
                Width = 350,
                Height = 475,
                CanResize = false,
                SystemDecorations = SystemDecorations.Full,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new MpAvColorPickerView(selectedColor)
            };

            var result = await cw.ShowChildDialogWithResultAsync(owner as Window);
            Mp.Services.ContextMenuCloser.CloseMenu();

            if (result is string newColor) {
                if (ucvm != null) {
                    ucvm.UserHexColor = newColor;
                }
                return newColor;
            }

            return string.Empty;
        }

    }
}
