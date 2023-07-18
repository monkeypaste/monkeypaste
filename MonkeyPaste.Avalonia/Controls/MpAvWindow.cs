using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static TheArtOfDev.HtmlRenderer.Adapters.RGraphicsPath;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvWindow : Window {

        #region Private Variables
        private const string NO_RESULT_OBJ = "sdoifjdsfjnlkwe2423";
        #endregion

        #region Constants
        #endregion

        #region Statics

        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Overrides
        protected override Type StyleKeyOverride => typeof(Window);
        #endregion

        #region State
        public DateTime? OpenDateTime { get; set; }
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


        #endregion

        #region Constructors
        public MpAvWindow() : base() {
            Init();
        }

        public MpAvWindow(Window owner) : base(owner.PlatformImpl) {
            Init();
        }
        #endregion

        #region Public Methods

        public void ShowChild(Window owner = null) {
            SilentLockMainWindowCheck(owner);

            if (owner == null) {
                Show();
            } else {
                Show(owner);
            }
        }
        public async Task<object> ShowChildWithResultAsync(Window owner = null) {
            SilentLockMainWindowCheck(owner);

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
            SilentLockMainWindowCheck(owner);
            await ShowDialog(owner ?? MpAvWindowManager.MainWindow);
        }

        public async Task<object> ShowChildDialogWithResultAsync(Window owner = null) {
            SilentLockMainWindowCheck(owner);

            var result = await ShowDialog<object>(owner ?? MpAvWindowManager.MainWindow);

            if (owner is Window w) {
                if (!w.ShowActivated) {

                } else {
                    w.Activate();
                    w.Focus();
                }
            }
            return DialogResult;
        }

        public override string ToString() {
            return $"MpAvWindow = '{this.Title}'";
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void Init() {
#if DEBUG
            this.AttachDevTools();
#endif
            Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("AppIcon", null, null, null) as WindowIcon;
            if (Mp.Services != null &&
                Mp.Services.ScreenInfoCollection == null) {
                Mp.Services.ScreenInfoCollection = new MpAvDesktopScreenInfoCollection(this);
            }

            MpAvWindowManager.AllWindows.Add(this);
            this.Closed += MpAvWindow_Closed;
        }
        private void MpAvWindow_Closed(object sender, EventArgs e) {
            MpAvWindowManager.AllWindows.Remove(this);
            this.Closed -= MpAvWindow_Closed;
        }
        private void SilentLockMainWindowCheck(Window owner) {
            if (owner != null && owner is not MpAvMainWindow) {
                return;
            }
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
