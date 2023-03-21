using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvWindow : Window, IStyleable {

        #region Private Variables
        private const string NO_RESULT_OBJ = "sdoifjdsfjnlkwe2423";
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IStyleable  Implementation
        Type IStyleable.StyleKey => typeof(Window);

        #endregion
        #endregion

        #region Properties

        public object DialogResult { get; set; }

        public MpWindowType WindowType {
            get {
                if (DataContext is MpIWindowViewModel cwvm) {
                    return cwvm.WindowType;
                }
                return MpWindowType.None;
            }
        }

        public bool WantsTopmost {
            get {
                if (DataContext is MpIWantsTopmostWindowViewModel tmwvm) {
                    return tmwvm.WantsTopmost;
                }
                return false;
            }
        }

        #endregion

        #region Constructors
        public MpAvWindow() : base() {
#if DEBUG
            this.AttachDevTools();
#endif
            MpAvWindowManager.AllWindows.Add(this);
            this.Closed += MpAvWindow_Closed;
        }
        private void MpAvWindow_Closed(object sender, EventArgs e) {
            MpAvWindowManager.AllWindows.Remove(this);
            this.Closed -= MpAvWindow_Closed;
        }
        #endregion

        #region Public Methods
        public void ShowChild(Window owner = null) {
            SilentLockMainWindowCheck();

            if (owner == null) {
                Show();
            } else {
                Show(owner);
            }
        }
        public async Task<object> ShowChildWithResultAsync(Window owner = null) {
            SilentLockMainWindowCheck();

            object result = NO_RESULT_OBJ;

            EventHandler close_handler = null;
            close_handler = (s, e) => {
                if (s is MpAvWindow w) {
                    result = w.DialogResult;
                    return;
                }
                result = null;
            };

            if (owner == null) {
                Show();
            } else {
                Show(owner);
            }
            while (true) {
                if (result is string resultStr &&
                    resultStr == NO_RESULT_OBJ) {
                    await Task.Delay(100);
                }
                break;
            }
            return result;
        }
        public async Task ShowChildDialogAsync(Window owner = null) {
            SilentLockMainWindowCheck();
            await ShowDialog(owner ?? MpAvWindowManager.MainWindow);
        }

        public async Task<object> ShowChildDialogWithResultAsync(Window owner = null) {
            SilentLockMainWindowCheck();

            var result = await ShowDialog<object>(owner ?? MpAvWindowManager.MainWindow);
            return result;
        }


        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void SilentLockMainWindowCheck() {
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen &&
                !MpAvMainWindowViewModel.Instance.IsMainWindowLocked) {

                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
            }
        }
        #endregion

        #region Commands
        #endregion
    }

}
