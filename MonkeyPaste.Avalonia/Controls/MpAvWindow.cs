using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Threading;
using PropertyChanged;
using System;
using System.Threading.Tasks;

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

        private static DevToolsOptions _defaultDevToolOptions;
        public static DevToolsOptions DefaultDevToolOptions =>
            _defaultDevToolOptions ??
            (_defaultDevToolOptions =
                new DevToolsOptions() {
                    ShowAsChildWindow = false,
                    //StartupScreenIndex = 0
                });

        public static MpAvWindow Create(Window owner = default) {
            if (owner == default) {
                return new MpAvWindow();
            }
            return new MpAvWindow(owner);
        }

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
        public DateTime? OpenDateTime { get; set; }
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


        #endregion


        #endregion

        #region Constructors
        public MpAvWindow() : base() {
            Init();
        }

        public MpAvWindow(Window owner) : base(owner.PlatformImpl) {
            //Owner = owner;
            Init();
        }
        #endregion

        #region Public Methods

        public void ShowChild(Window owner = null, bool silentLock = true) {
            if (silentLock) {
                SilentLockMainWindowCheck(owner);
            }

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


    [DoNotNotify]
    public class MpAvWindow<TViewModel> : MpAvWindow/*, IViewFor<TViewModel>*/ where TViewModel : class {
        public new TViewModel BindingContext {
            get => GetValue(DataContextProperty) as TViewModel;
            set => SetValue(DataContextProperty, value);
        }

        public TViewModel ViewModel {
            get => BindingContext;
            set => BindingContext = value;
        }

        //object? IViewFor.ViewModel {
        //    get => ViewModel;
        //    set => ViewModel = (TViewModel)value;
        //}
        public MpAvWindow() : base() { }
        public MpAvWindow(Window owner) : base(owner) { }
    }
}
