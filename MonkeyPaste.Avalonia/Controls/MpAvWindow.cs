using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using PropertyChanged;
using System;

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
        public void ShowChild() {
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
            Show();
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }

}
