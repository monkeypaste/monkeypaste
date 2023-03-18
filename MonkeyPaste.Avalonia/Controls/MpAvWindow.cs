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
