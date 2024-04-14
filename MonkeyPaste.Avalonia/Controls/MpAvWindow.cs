using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Diagnostics;
using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SQLite.SQLite3;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public class MpAvWindow :
        Window,
        MpIUserControl {

        #region Private Variables
        private const string NO_RESULT_OBJ = "sdoifjdsfjnlkwe2423";
        #endregion

        #region Constants
        #endregion

        #region Statics

        private static List<MpAvWindow> _openingWindows = [];
        public static IReadOnlyList<MpAvWindow> OpeningWindows =>
            _openingWindows;

        private static DevToolsOptions _defaultDevToolOptions;
        public static DevToolsOptions DefaultDevToolOptions =>
            _defaultDevToolOptions ??
            (_defaultDevToolOptions =
                new DevToolsOptions() {
                    ShowAsChildWindow = false,
                    //StartupScreenIndex = 0
                });

        #endregion

        #region Interfaces
        void MpIUserControl.SetDataContext(object dataContext) {
            DataContext = dataContext;
        }
        #endregion

        #region Properties

        public virtual MpIWindowViewModel BindingContext =>
            DataContext as MpIWindowViewModel;

        #region Overrides
        protected override Type StyleKeyOverride => typeof(Window);
        #endregion

        #region State
        public nint Handle {
            get {
                if (this.TryGetPlatformHandle() is { } ph) {
                    return ph.Handle;
                }
                return IntPtr.Zero;
            }
        }

        public object DialogResult { get; set; }

        private MpWindowType _windowType = MpWindowType.None;
        public MpWindowType WindowType {
            get {
                if (_windowType != MpWindowType.None) {
                    return _windowType;
                }
                if (DataContext is MpIWindowViewModel cwvm) {
                    return cwvm.WindowType;
                }
                return MpWindowType.None;
            }
            set {
                if (WindowType != value) {
                    _windowType = value;
                }
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

        public DateTime? OpenDateTime { get; set; }
        public DateTime LastActiveDateTime { get; set; }

        public MpIPlatformScreenInfo ScreenInfo {
            get {
                if (this.Screens.ScreenFromWindow(this) is not Screen scr) {
                    return this.Screens.Primary.ToScreenInfo();
                }
                return scr.ToScreenInfo();
            }
        }

        #endregion


        #endregion

        #region Constructors
        public MpAvWindow() : this(null) { }
        public MpAvWindow(Window owner = default) : base(owner == null ? PlatformManager.CreateWindow() : owner.PlatformImpl) {
            Init();
        }

        #endregion

        #region Public Methods
        public void SetBounds(Rect rect) {
            Bounds = rect;
        }

        protected override void OnOpened(EventArgs e) {
            base.OnOpened(e);
            Dispatcher.UIThread.Post(async () => {
                // wait for window to activate (if it does)
                await Task.Delay(500);
                _openingWindows.Remove(this);
            });
        }
        protected override void OnClosed(EventArgs e) {
            _openingWindows.Remove(this);
            base.OnClosed(e);
        }

        public new void Show(Window owner = null) {
            //if (silentLock) {
            //    SilentLockMainWindowCheck(owner);
            //}
            if (!_openingWindows.Contains(this)) {
                _openingWindows.Add(this);
            }

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow is not MpAvMainWindow) {
                desktop.MainWindow = this;
            }

            if (owner == null) {
                base.Show();
            } else {
                base.Show(owner);
            }
        }
        public async Task<object> ShowDialogWithResultAsync(Window owner = null) {
            SilentLockMainWindowCheck(owner);

#if MAC && false
            // weird issues (only check assign tile hotkey) after closing dialog so faking it...
            bool is_done = false;
            void OnClosed(object sender, EventArgs e) {
                Closed -= OnClosed;
                is_done = true;
            }
            Closed += OnClosed;
            Show();
            while (!is_done) {
                await Task.Delay(100);
            }
# else
            _ = await ShowDialog<object>(owner ?? MpAvWindowManager.LastActiveWindow);
#endif

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

        public void ShowDevTools() {
#if DEBUG
            this.Focus();
            Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence("F12");
#endif
        }
        #endregion

        #region Protected Methods
        protected override void OnClosing(WindowClosingEventArgs e) {
            base.OnClosing(e);
            if (e.Cancel || !this.Classes.Contains("fadeOut") || this.Classes.Contains("closing")) {
                return;
            }
            e.Cancel = true;
            this.Classes.Add("closing");
            TimeSpan fadeOutDur = Mp.Services.PlatformResource.GetResource<TimeSpan>("FadeOutDur");
            Dispatcher.UIThread.Post(async () => {
                await Task.Delay((int)fadeOutDur.TotalMilliseconds);
                this.Close();
                this.Classes.Remove("closing");
            });
        }
        #endregion

        #region Private Methods
        private void Init() {
#if DEBUG
            this.AttachDevTools(DefaultDevToolOptions);
#endif
            Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("AppIcon", typeof(WindowIcon), null, null) as WindowIcon;
            if (Mp.Services != null &&
                Mp.Services.ScreenInfoCollection == null) {
                Mp.Services.ScreenInfoCollection = new MpAvDesktopScreenInfoCollection(this);
            }
            if (MpAvPrefViewModel.Instance.IsThemeDark) {
                Classes.Add("dark");
            }
            if (MpAvPrefViewModel.Instance.IsTextRightToLeft) {
                Classes.Add("rtl");
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

    [DoNotNotify]
    public class MpAvNotificationWindow : MpAvWindow {
        public MpAvNotificationWindow(Window owner = default) : base(owner) { }
    }
}
