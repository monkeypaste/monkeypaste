using Avalonia.Controls;
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

            //var cpw = new ColorPickerWindow(selectedColor.ToAvColor()) {
            //    IsPaletteVisible = true,
            //    Topmost = true,
            //    Title = string.IsNullOrWhiteSpace(title) ? "Pick a color, any color" : title
            //};
            //var result = await cpw.ShowDialog(MpAvMainView.Instance);


            var cw = new Window() {
                Content = new MpAvColorPickerView() {
                    SelectedHexColor = selectedColor
                },
                Width = 300,
                Height = 300,
                WindowState = WindowState.Normal,
                CanResize = false,
                SystemDecorations = SystemDecorations.BorderOnly,
                ShowInTaskbar = false
            };

            await cw.ShowDialog(MpAvWindowManager.MainWindow);
            if (cw.Tag is string) {
                newColor = cw.Tag.ToString();
                if (ucvm != null) {
                    ucvm.UserHexColor = newColor;
                }
            }
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
            Mp.Services.ContextMenuCloser.CloseMenu();

            return newColor;
        }
        public string ShowCustomColorMenu(string selectedColor, string title = null, MpIUserColorViewModel ucvm = null) {
            return ShowCustomColorMenuAsync(selectedColor, title, ucvm).Result;
        }
    }
}
