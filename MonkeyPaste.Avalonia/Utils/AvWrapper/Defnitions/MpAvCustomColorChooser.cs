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
                    string newColor = await ShowCustomColorMenuAsync(selectedColor, ucvm);
                }
            });


        public async Task<string> ShowCustomColorMenuAsync(string selectedColor, MpIUserColorViewModel ucvm) {
            string newColor = String.Empty;

            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
            MpAvContextMenuView.Instance.IsShowingChildDialog = true;

            var cpw = new ColorPickerWindow(selectedColor.ToAvColor()) {
                IsPaletteVisible = true,
                Topmost = true
            };
            var result = await cpw.ShowDialog(MpAvMainWindow.Instance);

            if (result != null) {
                newColor = result.ToString();
                if (ucvm != null) {
                    ucvm.UserHexColor = newColor;
                }
            }
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
            MpAvContextMenuView.Instance.IsShowingChildDialog = false;
            MpPlatformWrapper.Services.ContextMenuCloser.CloseMenu();

            return newColor;
        }

        public string ShowCustomColorMenu(string selectedColor, MpIUserColorViewModel ucvm) {
            return ShowCustomColorMenuAsync(selectedColor, ucvm).Result;
        }
    }
}
