using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;
using Avalonia.Controls.Primitives;
using AvaloniaColorPicker;
using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Media;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    public class MpAvCustomColorChooser : MpICustomColorChooserMenuAsync {
        public ICommand SelectCustomColorCommand => new MpAsyncCommand<object>(
            async(args) => {
                if (args is MpIUserColorViewModel ucvm) {
                    string selectedColor = ucvm.UserHexColor;
                    string newColor = await ShowCustomColorMenuAsync(selectedColor, null, ucvm);
                }
            });


        public async Task<string> ShowCustomColorMenuAsync(string selectedColor, string title = null, MpIUserColorViewModel ucvm = null) {
            string newColor = String.Empty;

            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
            MpAvMenuExtension.IsChildDialogOpen = true;

            var cpw = new ColorPickerWindow(selectedColor.ToAvColor()) {
                IsPaletteVisible = true,
                Topmost = true,
                Title = string.IsNullOrWhiteSpace(title) ? "Pick a color, any color" : title
            };
            var result = await cpw.ShowDialog(MpAvMainWindow.Instance);

            if (result != null) {
                newColor = result.ToString();
                if (ucvm != null) {
                    ucvm.UserHexColor = newColor;
                }
            }
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
            MpAvMenuExtension.IsChildDialogOpen = false;
            MpPlatformWrapper.Services.ContextMenuCloser.CloseMenu();

            return newColor;
        }
        public string ShowCustomColorMenu(string selectedColor, string title = null, MpIUserColorViewModel ucvm = null) {
            return ShowCustomColorMenuAsync(selectedColor,title, ucvm).Result;
        }
    }
}
