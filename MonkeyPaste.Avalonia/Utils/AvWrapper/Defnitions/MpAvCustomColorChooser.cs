//using AvaloniaColorPicker;
using AvaloniaColorPicker;
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


        public async Task<string> ShowCustomColorMenuAsync(string selectedColor, string title = null, MpIUserColorViewModel ucvm = null) {
            string newColor = String.Empty;

            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;

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
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
            MpPlatform.Services.ContextMenuCloser.CloseMenu();

            return newColor;
        }
        public string ShowCustomColorMenu(string selectedColor, string title = null, MpIUserColorViewModel ucvm = null) {
            return ShowCustomColorMenuAsync(selectedColor, title, ucvm).Result;
        }
    }
}
