using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpFakeWindowActionType {
        None = 0,
        Open,
        Close
    }

    public class MpAvFakeWindowViewModel :
        MpAvViewModelBase<MpAvPointerGestureWindowViewModel>,
        MpIWantsTopmostWindowViewModel,
        MpICloseWindowViewModel {
        #region Private Variables

        private const int FAKE_WINDOW_ANIM_MS = 1_000; // needs to match fakeWindow animations

        #endregion

        #region Constants
        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #region MpIWantsTopmostWindowViewModel Implementation

        public bool WantsTopmost =>
            true;
        public MpWindowType WindowType =>
            MpWindowType.Modal3;

        #endregion

        #region MpICloseWindowViewModel Implementation

        public bool IsWindowOpen { get; set; }

        #endregion

        #endregion

        #region Properties

        #region State
        bool IsFakeWindowVisible { get; set; } = true;
        public bool IsDndEnabled =>
            GestureType == MpPointGestureType.DragToOpen;
        public bool IsDragOver { get; set; }
        public bool HasDropped { get; set; }
        public MpPointGestureType GestureType =>
            Parent == null ? MpPointGestureType.None : Parent.GestureType;

        bool IsAnimating =>
            !IsOpen && !IsClosed;

        bool IsOpen =>
            FakeWindowActualTop == FakeWindowEndTop;

        bool IsClosed =>
            FakeWindowActualTop == FakeWindowStartTop;

        bool WasGestureCompleted {
            get {
                if (GestureType == MpPointGestureType.ScrollToOpen) {
                    return IsFakeWindowVisible;
                }
                return HasDropped;
            }
        }

        public MpFakeWindowActionType FakeWindowActionType { get; set; } = MpFakeWindowActionType.None;

        #endregion

        #region Appearance

        public string FakeWindowLabel =>
           GestureType == MpPointGestureType.ScrollToOpen ?
               UiStrings.WelcomeScrollToOpenFakeWindowLabel :
                HasDropped ?
                    UiStrings.WelcomeDragToOpenFakeWindowLabel2 :
                    UiStrings.WelcomeDragToOpenFakeWindowLabel1;

        public string FakeWindowDetail =>
            GestureType == MpPointGestureType.ScrollToOpen ?
                UiStrings.WelcomeScrollToOpenFakeWindowDetail :
                UiStrings.WelcomeDragToOpenFakeWindowDetail;


        #endregion


        #region Layout
        public PixelRect FakeWindowScreenRect {
            get {
                var fwr = MpAvWindowManager.AllWindows.FirstOrDefault(x => x.DataContext is MpAvWelcomeNotificationViewModel).Screens.Primary.WorkingArea;
                int w = fwr.Width;
                int h = (int)((double)fwr.Height * 0.3d);
                int x = fwr.X;
                int y = fwr.Bottom - h;
                return new PixelRect(x, y, w, h);
            }
        }

        public Size FakeWindowSize =>
            FakeWindowScreenRect.Size.ToPortableSize(MpAvWindowManager.ActiveWindow.VisualPixelDensity()).ToAvSize();

        public double FakeWindowStartTop =>
            FakeWindowSize.Height;

        public double FakeWindowEndTop =>
            0;

        public double FakeWindowActualTop { get; set; }
        #endregion


        #endregion

        #region Constructors
        public MpAvFakeWindowViewModel() : base(null) { }
        public MpAvFakeWindowViewModel(MpAvPointerGestureWindowViewModel parent) : base(parent) {
            PropertyChanged += MpAvFakeWindowViewModel_PropertyChanged;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += Instance_OnGlobalMouseReleased;
        }



        #endregion

        #region Public Methods

        public void ResetDropState() {
            IsDragOver = false;
            HasDropped = false;
        }
        public void Destroy() {
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased -= Instance_OnGlobalMouseReleased;
            IsWindowOpen = false;
            IsFakeWindowVisible = false;
            ResetDropState();
        }
        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        private void MpAvFakeWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(GestureType):
                    OnPropertyChanged(nameof(IsDndEnabled));
                    break;
                case nameof(HasDropped):
                    OnPropertyChanged(nameof(FakeWindowLabel));
                    break;
                case nameof(IsWindowOpen):
                    OnPropertyChanged(nameof(FakeWindowLabel));
                    OnPropertyChanged(nameof(FakeWindowDetail));
                    if (IsWindowOpen) {
                        break;
                    }
                    break;
            }
        }
        private MpAvWindow CreateFakeMainWindow() {
            var fmw = new MpAvWindow() {
                DataContext = this,
                Content = new MpAvFakeWindowView(),
                Background = Brushes.Transparent,
                ShowActivated = true,
                CanResize = false,
                WindowState = WindowState.Normal,
                BorderThickness = new Thickness(0),
                SystemDecorations = SystemDecorations.None,
                TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
            };
            fmw.Bind(
                Window.IsVisibleProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(IsFakeWindowVisible),
                    Mode = BindingMode.TwoWay
                });
#if WINDOWS 
            MpAvToolWindow_Win32.SetAsToolWindow(fmw.TryGetPlatformHandle().Handle);
#endif
            fmw.Position = FakeWindowScreenRect.Position;
            fmw.Width = FakeWindowSize.Width;
            fmw.Height = FakeWindowSize.Height;
            FakeWindowActualTop = FakeWindowStartTop;

            return fmw;
        }

        private void Instance_OnGlobalMouseReleased(object sender, bool e) {
            if (IsClosed) {
                return;
            }
            if (IsDragOver) {
                // this implies drop will happen/is happening, timing is weird
                // so don't hide when dropping
                IsDragOver = false;
                HasDropped = true;
                return;
            }
            ToggleFakeWindowCommand.Execute(null);
        }

        DispatcherTimer _animationTimer;
        private async Task AnimateMainWindowAsync(double finalTop) {
            // close 0.12 20
            // open 
            double zeta = 0.22d;
            double omega = 25;
            double[] x = new double[] { FakeWindowActualTop };
            double[] xt = new double[] { finalTop };
            double[] v = new double[1];
            double min_done_v = 0.5d;// 0.9d;
            bool isDone = false;
            DateTime prevTime = DateTime.Now;
            if (_animationTimer == null) {
                _animationTimer = new DispatcherTimer();
                _animationTimer.Interval = TimeSpan.FromMilliseconds(1000d / 60d);
            }
            EventHandler tick = (s, e) => {
                var curTime = DateTime.Now;
                double dt = (curTime - prevTime).TotalMilliseconds / 1000.0d;
                prevTime = curTime;
                for (int i = 0; i < x.Length; i++) {
                    MpAnimationHelpers.Spring(ref x[i], ref v[i], xt[i], dt, zeta, omega);
                }
                bool is_v_zero = v.All(x => Math.Abs(x) <= min_done_v);

                if (is_v_zero) {
                    // consider done when all v's are pretty low or canceled
                    isDone = true;
                    _animationTimer.Stop();
                    return;
                }
                //SetMainWindowRect(new MpRect(x));
                FakeWindowActualTop = x[0];

            };

            _animationTimer.Tick += tick;
            _animationTimer.Start();

            var timeout_sw = Stopwatch.StartNew();
            while (!isDone) {
                await Task.Delay(5);
                if (timeout_sw.ElapsedMilliseconds >= 2000) {
                    isDone = true;
                }
            }
            _animationTimer.Stop();
            _animationTimer.Tick -= tick;
            FakeWindowActualTop = finalTop;
        }
        #endregion

        #region Commands
        public ICommand ToggleFakeWindowCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (!IsWindowOpen) {
                    var fw = CreateFakeMainWindow();
                    fw.ShowChild();
                    IsFakeWindowVisible = false;
                    return;
                }
                switch (FakeWindowActionType) {
                    case MpFakeWindowActionType.None:
                    case MpFakeWindowActionType.Close:
                        FakeWindowActionType = MpFakeWindowActionType.Open;

                        IsFakeWindowVisible = true;
                        await AnimateMainWindowAsync(FakeWindowEndTop);
                        break;
                    case MpFakeWindowActionType.Open:
                        FakeWindowActionType = MpFakeWindowActionType.Close;

                        await AnimateMainWindowAsync(FakeWindowStartTop);
                        IsFakeWindowVisible = false;
                        if (WasGestureCompleted) {
                            MpAvWelcomeNotificationViewModel.Instance.ToggleGestureDemoCommand.Execute(null);
                        }
                        break;
                }

            }, (args) => {
                return !IsAnimating;
            });
        #endregion
    }
}
