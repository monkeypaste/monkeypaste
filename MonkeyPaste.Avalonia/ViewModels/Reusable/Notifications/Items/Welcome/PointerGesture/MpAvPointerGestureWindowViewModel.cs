using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpPointGestureType {
        None,
        ScrollToOpen,
        DragToOpen
    };

    public class MpAvPointerGestureWindowViewModel :
        MpAvViewModelBase<MpAvWelcomeOptionItemViewModel>,
        MpIWantsTopmostWindowViewModel,
        MpICloseWindowViewModel {
        #region Private Variables
        private bool _isAnimating = false;
        private MpAvWindow _fakeMainWindow;
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
            MpWindowType.Modal;

        #endregion

        #region MpICloseWindowViewModel Implementation

        public bool IsWindowOpen { get; set; }


        #endregion

        #endregion

        #region Properties

        #region State
        public MpPointGestureType GestureType { get; private set; } = MpPointGestureType.None;


        public bool IsGesturing =>
            GestureType == MpPointGestureType.ScrollToOpen ?
                MpAvMainWindowViewModel.CanScrollOpen() :
                MpAvMainWindowViewModel.CanDragOpen() && MpAvDoDragDropWrapper.DragDataObject != null;

        public bool IsInGestureZone { get; private set; }
        #endregion

        #region Appearance
        public IBrush EdgeBrush =>
            IsInGestureZone ? Brushes.Lime : Brushes.Red;
        public string MarkerLabel =>
            GestureType == MpPointGestureType.ScrollToOpen ?
                UiStrings.WelcomeScrollToOpenMarkerLabel :
                UiStrings.WelcomeDragToOpenMarkerLabel;

        public string FakeWindowLabel =>
           GestureType == MpPointGestureType.ScrollToOpen ?
               UiStrings.WelcomeScrollToOpenFakeWindowLabel :
               UiStrings.WelcomeDragToOpenFakeWindowLabel;

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
        public double MaxWindowY =>
            130;
        #endregion


        #endregion

        #region Constructors
        public MpAvPointerGestureWindowViewModel() : base(null) { }
        public MpAvPointerGestureWindowViewModel(MpAvWelcomeOptionItemViewModel parent, MpPointGestureType gestureType) : base(parent) {
            PropertyChanged += MpAvPointerGestureWindowViewModel_PropertyChanged;
            GestureType = gestureType;

            MpAvShortcutCollectionViewModel.Instance.StartInputListener();
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove += Instance_OnGlobalMouseMove;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalDrag += Instance_OnGlobalDrag;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll += Instance_OnGlobalMouseWheelScroll;
        }


        public void Destroy() {
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove -= Instance_OnGlobalMouseMove;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalDrag -= Instance_OnGlobalDrag;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll -= Instance_OnGlobalMouseWheelScroll;
            IsWindowOpen = false;
            ToggleFakeWindowShowAsync(true, null).FireAndForgetSafeAsync();
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods

        protected virtual MpAvWindow CreateTopEdgeMarkerWindow() {
            MpSize ss = MpAvWindowManager.ActiveWindow.Screens.Primary.Bounds.Size.ToPortableSize(MpAvWindowManager.ActiveWindow.Screens.Primary.Scaling);

            var gw = new MpAvWindow() {
                Content = new MpAvTopEdgeMarkerView(),
                DataContext = this,
                Topmost = true,
                Position = new PixelPoint(),
                Width = ss.Width,
                Height = MaxWindowY,
                Background = Brushes.Transparent,//new SolidColorBrush() { Color = Colors.Orange, Opacity = 0.3 },
                WindowState = WindowState.Normal,
                CanResize = false,
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome,
                ExtendClientAreaToDecorationsHint = true,
                SystemDecorations = SystemDecorations.None,
                ExtendClientAreaTitleBarHeightHint = 0,
                TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
            };

#if WINDOWS
            MpAvToolWindow_Win32.SetAsNoHitTestWindow(gw.TryGetPlatformHandle().Handle);
#endif
            return gw;
        }

        #endregion

        #region Private Methods

        private void MpAvPointerGestureWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(GestureType):
                    OnPropertyChanged(nameof(MarkerLabel));
                    OnPropertyChanged(nameof(FakeWindowLabel));
                    OnPropertyChanged(nameof(FakeWindowDetail));
                    break;
                case nameof(IsWindowOpen):
                    if (IsWindowOpen) {
                        break;
                    }
                    ToggleFakeWindowShowAsync(true, null).FireAndForgetSafeAsync();
                    break;
            }
        }


        protected virtual void Instance_OnGlobalMouseWheelScroll(object sender, MpPoint e) {
            CheckForGestureAsync().FireAndForgetSafeAsync();
        }

        protected virtual void Instance_OnGlobalDrag(object sender, MpPoint e) {
            CheckForGestureAsync().FireAndForgetSafeAsync();
        }
        private void Instance_OnGlobalMouseMove(object sender, MpPoint e) {
            IsInGestureZone = MpAvMainWindowViewModel.IsPointerInTopEdgeZone();
            OnPropertyChanged(nameof(EdgeBrush));
        }

        private async Task CheckForGestureAsync() {
            if (!IsGesturing || _isAnimating) {
                return;
            }

            _isAnimating = true;
            if (_fakeMainWindow == null) {
                if (Parent.IsChecked) {
                    return;
                }
                Parent.ToggleOptionCommand.Execute(null);
                _fakeMainWindow = CreateFakeMainWindow();
                _fakeMainWindow.Opened += async (s, e1) => {
                    await Task.Delay(300);
                    _fakeMainWindow.Classes.Add("show");
                    await ToggleFakeWindowShowAsync(false, true);
                    _isAnimating = false;
                };
                _fakeMainWindow.ShowChild();
                return;
            }
            await ToggleFakeWindowShowAsync(false, null);
            _isAnimating = false;
        }

        #region Fake Main Window
        private MpAvWindow CreateFakeMainWindow() {
            var fmw = new MpAvWindow() {
                DataContext = this,
                Topmost = true,
                Content = new MpAvFakeWindowView(),
                Background = Brushes.Transparent,//new SolidColorBrush() { Color = Colors.Orange, Opacity = 0.3 },
                WindowState = WindowState.Normal,
                CanResize = false,
                BorderThickness = new Thickness(0),
                //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome,
                //ExtendClientAreaToDecorationsHint = true,
                //ExtendClientAreaTitleBarHeightHint = -1,
                SystemDecorations = SystemDecorations.None,
                TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
            };
#if WINDOWS
            MpAvToolWindow_Win32.SetAsNoHitTestWindow(fmw.TryGetPlatformHandle().Handle);
#endif
            fmw.Position = FakeWindowScreenRect.Position;
            fmw.Width = FakeWindowSize.Width;
            fmw.Height = FakeWindowSize.Height;
            if (fmw.Content is MpAvFakeWindowView fwv &&
                fwv.FindControl<Border>("PlaceholderWindow") is Border fwv_panel) {
                Canvas.SetTop(fwv_panel, FakeWindowStartTop);
            }
            return fmw;
        }

        private async Task ToggleFakeWindowShowAsync(bool destroy, bool? show) {
            if (_fakeMainWindow == null ||
                _fakeMainWindow.Content is not MpAvFakeWindowView fwv ||
                fwv.FindControl<Border>("PlaceholderWindow") is not Border fwv_panel) {
                return;
            }
            bool is_shown = fwv_panel.Classes.Contains("show");
            if (!is_shown && show.IsFalse()) {
                return;
            }
            if (is_shown || destroy) {
                fwv_panel.Classes.Add("hide");
                fwv_panel.Classes.Remove("show");
            } else {
                fwv_panel.Classes.Add("show");
                fwv_panel.Classes.Remove("hide");
            }
            if (!destroy || is_shown) {
                await Task.Delay(FAKE_WINDOW_ANIM_MS);
            }
            if (destroy) {
                _fakeMainWindow.Close();
                _fakeMainWindow = null;
                MpAvShortcutCollectionViewModel.Instance.OnGlobalMousePressed -= Instance_OnGlobalMouseClicked;
            } else if (!is_shown) {
                // open now
                MpAvShortcutCollectionViewModel.Instance.OnGlobalMousePressed += Instance_OnGlobalMouseClicked;

            }
        }

        private async void Instance_OnGlobalMouseClicked(object sender, bool e) {
            await ToggleFakeWindowShowAsync(false, false);
        }
        #endregion

        #endregion

        #region Commands
        public ICommand ShowGestureWindowCommand => new MpCommand(
            () => {
                var gw = CreateTopEdgeMarkerWindow();
                gw.Show();
            });
        #endregion
    }
}
