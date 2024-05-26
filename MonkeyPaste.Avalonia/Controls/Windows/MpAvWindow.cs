using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Diagnostics;
using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public class MpAvWindow :
#if WINDOWED
        MpAvChildWindow,
#else
        Window,
#endif
        MpIUserControl {

        #region Private Variables
        private const string NO_RESULT_OBJ = "sdoifjdsfjnlkwe2423";
        #endregion

        #region Constants
        #endregion

        #region Statics

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
#if !WINDOWED
        protected override Type StyleKeyOverride => typeof(Window); 
#endif
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

#if !WINDOWED
        public object DialogResult { get; set; }
#endif

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
#if WINDOWED
                return Screens.Primary;
#else
                if (this.GetVisualAncestor<Window>() is not { } w ||
                            this.Screens.ScreenFromWindow(w) is not Screen scr) {
                    return this.Screens.Primary.ToScreenInfo();
                }
                return scr.ToScreenInfo(); 
#endif
            }
        }

        #endregion


        #endregion

        #region Constructors
        public MpAvWindow() {
            Init();
        }

        #endregion

        #region Public Methods
        public void SetBounds(Rect rect) {
            Bounds = rect;
        }

        
        public new void Show(Window owner) {
            MpAvWindowManager.OpeningWindows.AddOrReplace(this);

            if (owner == null) {
                base.Show();
            } else {
                base.Show(owner);
            }
        }
        public async Task<object> ShowDialogWithResultAsync(MpAvWindow owner = null) {
            SilentLockMainWindowCheck(owner);

            _ = await ShowDialog<object>(owner ?? MpAvWindowManager.LastActiveWindow);

            if (owner != null) {
                if (!owner.ShowActivated) {

                } else {
                    owner.Activate();
                    owner.Focus();
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
#if DEBUG && !WINDOWED
            this.AttachDevTools(DefaultDevToolOptions);
#endif
            Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("AppIcon", typeof(MpAvWindowIcon), null, null) as MpAvWindowIcon;
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
            Classes.Add(MpAvThemeViewModel.Instance.PlatformShortName);

            MpAvWindowManager.AllWindows.Add(this);
            this.Closed += MpAvWindow_Closed;
        }
        private void MpAvWindow_Closed(object sender, EventArgs e) {
            MpAvWindowManager.AllWindows.Remove(this);
            this.Closed -= MpAvWindow_Closed;
        }
        private void SilentLockMainWindowCheck(MpAvWindow owner) {
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
    public abstract class MpAvWindow<TViewModel> : MpAvWindow where TViewModel : class {
        public new TViewModel BindingContext {
            get => GetValue(DataContextProperty) as TViewModel;
            set => SetValue(DataContextProperty, value);
        }
    }
}
