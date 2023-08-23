using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvScrollToOpenGestureViewModel : MpAvPointerGestureWindowViewModel {
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
        #endregion

        #region Properties

        #region State

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
        #endregion
        #endregion

        #region Constructors
        public MpAvScrollToOpenGestureViewModel() : base(null) { }
        public MpAvScrollToOpenGestureViewModel(MpAvWelcomeOptionItemViewModel parent) : base(parent) {
            PropertyChanged += MpAvScrollToOpenGestureViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        protected override async void Instance_OnGlobalMouseWheelScroll(object sender, MpPoint e) {
            base.Instance_OnGlobalMouseWheelScroll(sender, e);

            if (!MpAvMainWindowViewModel.CanScrollOpen() || _isAnimating) {
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


        protected override MpAvWindow CreateGestureWindow() {
            var gw = base.CreateGestureWindow();
            gw.Content = new MpAvScrollToOpenGestureView();
            return gw;
        }

        #endregion

        #region Private Methods

        private void MpAvScrollToOpenGestureViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsWindowOpen):
                    if (IsWindowOpen) {
                        break;
                    }
                    ToggleFakeWindowShowAsync(true, null).FireAndForgetSafeAsync();
                    break;
            }
        }
        private async void Instance_OnGlobalMouseClicked(object sender, bool e) {
            await ToggleFakeWindowShowAsync(false, false);
        }
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
        #endregion

        #region Commands
        #endregion


    }
}
