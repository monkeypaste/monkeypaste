using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvCustomColorChooserViewModel :
        MpAvViewModelBase,
        MpICustomColorChooserMenuAsync,
        MpICloseWindowViewModel,
        MpIWantsTopmostWindowViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #region MpIChildWindowViewModel Implementation

        public bool IsWindowOpen { get; set; }
        public MpWindowType WindowType =>
            MpWindowType.Modal;

        #endregion

        #region MpIWantsTopmostWindowViewModel Implementation


        public bool WantsTopmost =>
            true;

        #endregion
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
        public ICommand SelectCustomColorCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is MpIUserColorViewModel ucvm) {
                    string selectedColor = ucvm.UserHexColor;
                    string newColor = await ShowCustomColorMenuAsync(
                        selectedColor: selectedColor,
                        title: null,
                        ucvmObj: ucvm);
                }
            });


        public async Task<string> ShowCustomColorMenuAsync(
            string selectedColor,
            string title = null,
            object ucvmObj = null,
            object owner = null,
            string[] fixedPalette = null,
            bool allowAlpha = false) {
            string result = await Dispatcher.UIThread.InvokeAsync(async () => {
                MpConsole.WriteLine("Custom color menu opened");
                title = title == null ? UiStrings.ColorChooserDefaultWindowTitle : title;
                var cw = new MpAvWindow() {
                    DataContext = this,
                    Topmost = true,
                    Title = title.ToWindowTitleText(),
                    Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("ColorsImage", typeof(MpAvWindowIcon), null, null) as MpAvWindowIcon,
                    Width = 350,
                    Height = 450,
                    CanResize = false,
                    SystemDecorations = SystemDecorations.Full,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new MpAvColorPickerView(selectedColor, fixedPalette, allowAlpha),
                    Background = Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveBgColor)
                };

                // NOTE pre-closing context menu cause it'll make funny activation behavior


                var result = await cw.ShowDialogWithResultAsync(MpAvWindowManager.CurrentOwningWindow);
                MpConsole.WriteLine($"Custom color result: '{result.ToStringOrEmpty("NULL")}'");
                //await Task.Delay(200);
                MpAvMenuView.CloseMenu();

                //if (owner is MpAvMainWindow) {
                //    MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
                //}
                if (result is string newColor) {
                    if (ucvmObj is MpIUserColorViewModel ucvm) {

                        ucvm.UserHexColor = newColor;
                        MpConsole.WriteLine($"Custom color for '{ucvmObj}' set to '{newColor}'");
                    }
                    return newColor;
                }

                return string.Empty;
            });
            return result;
        }

    }
}
