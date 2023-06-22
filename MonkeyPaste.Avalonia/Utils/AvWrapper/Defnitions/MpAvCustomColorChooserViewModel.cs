using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvCustomColorChooserViewModel :
        MpViewModelBase,
        MpICustomColorChooserMenuAsync,
        MpIChildWindowViewModel,
        MpIWantsTopmostWindowViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #region MpIChildWindowViewModel Implementation

        public bool IsChildWindowOpen { get; set; }
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
                        ucvm: ucvm);
                }
            });


        public async Task<string> ShowCustomColorMenuAsync(
            string selectedColor,
            string title = null,
            MpIUserColorViewModel ucvm = null,
            object owner = null,
            string[] fixedPalette = null) {
            string result = await Dispatcher.UIThread.InvokeAsync(async () => {
                MpConsole.WriteLine("Custom color menu opened");
                owner = owner == null ? MpAvWindowManager.ActiveWindow : owner;
                if (owner == null) {
                    // i think when context menu opens the window deactivates so
                    // just using mainwindow
                    // TODO should add LastActiveDateTime to MpAvWindow to handle this better
                    owner = MpAvWindowManager.MainWindow;
                }
                var cw = new MpAvWindow() {
                    DataContext = this,
                    Topmost = true,
                    Title = "Color Chooser".ToWindowTitleText(),
                    Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("ColorsImage", typeof(WindowIcon), null, null) as WindowIcon,
                    Width = 350,
                    Height = 450,
                    CanResize = false,
                    SystemDecorations = SystemDecorations.Full,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new MpAvColorPickerView(selectedColor, fixedPalette),
                };

                //if (owner is MpAvMainWindow) {
                //    MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
                //}

                // NOTE pre-closing context menu cause it'll make funny activation behavior

                //Mp.Services.ContextMenuCloser.CloseMenu();

                var result = await cw.ShowChildDialogWithResultAsync(owner as Window);
                //await Task.Delay(200);
                Mp.Services.ContextMenuCloser.CloseMenu();

                //if (owner is MpAvMainWindow) {
                //    MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
                //}
                if (result is string newColor) {
                    if (ucvm != null) {

                        ucvm.UserHexColor = newColor;
                        MpConsole.WriteLine($"Custom color for '{ucvm}' set to '{newColor}'");
                    }
                    return newColor;
                }

                return string.Empty;
            });
            return result;
        }

    }
}
