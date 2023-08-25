using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpFakeWindowState {
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
        public MpPointGestureType GestureType =>
            Parent == null ? MpPointGestureType.None : Parent.GestureType;

        bool IsAnimating =>
            !IsOpen && !IsClosed;

        bool IsOpen =>
            FakeWindowActualTop == FakeWindowEndTop;

        bool IsClosed =>
            FakeWindowActualTop == FakeWindowStartTop;

        public MpFakeWindowState FakeWindowState { get; set; } = MpFakeWindowState.None;

        #endregion

        #region Appearance

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

        public double FakeWindowActualTop { get; set; }
        #endregion


        #endregion

        #region Constructors
        public MpAvFakeWindowViewModel() : base(null) { }
        public MpAvFakeWindowViewModel(MpAvPointerGestureWindowViewModel parent) : base(parent) {
            PropertyChanged += MpAvFakeWindowViewModel_PropertyChanged;

            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += Instance_OnGlobalMouseReleased;
        }


        public void Destroy() {
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased -= Instance_OnGlobalMouseReleased;
            IsWindowOpen = false;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        private void MpAvFakeWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(GestureType):
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
                WindowState = WindowState.Normal,
                ShowActivated = true,
                CanResize = false,
                BorderThickness = new Thickness(0),
                SystemDecorations = SystemDecorations.None,
                TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
            };
#if WINDOWS && DEBUG
            MpAvToolWindow_Win32.SetAsNoHitTestWindow(fmw.TryGetPlatformHandle().Handle);
            MpAvToolWindow_Win32.InitToolWindow(fmw.TryGetPlatformHandle().Handle);
#endif
            fmw.Position = FakeWindowScreenRect.Position;
            fmw.Width = FakeWindowSize.Width;
            fmw.Height = FakeWindowSize.Height;
            if (fmw.Content is MpAvFakeWindowView fwv &&
                fwv.FindControl<Border>("PlaceholderWindow") is Border fwv_panel) {
                Canvas.SetTop(fwv_panel, FakeWindowStartTop);
            } // check actual top is bound correct
            return fmw;
        }

        private void Instance_OnGlobalMouseReleased(object sender, bool e) {
            if (IsClosed) {
                return;
            }
            ToggleFakeWindowCommand.Execute(null);
        }

        #endregion

        #region Commands
        public ICommand ToggleFakeWindowCommand => new MpCommand<object>(
            (args) => {
                if (!IsWindowOpen) {
                    var fw = CreateFakeMainWindow();
                    fw.ShowChild();
                    return;
                }
                switch (FakeWindowState) {
                    case MpFakeWindowState.None:
                    case MpFakeWindowState.Close:
                        FakeWindowState = MpFakeWindowState.Open;
                        break;
                    case MpFakeWindowState.Open:
                        FakeWindowState = MpFakeWindowState.Close;
                        break;
                }

            }, (args) => {
                return !IsAnimating;
            });
        #endregion
    }
}
