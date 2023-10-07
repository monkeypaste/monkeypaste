using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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
            MpWindowType.Modal2;

        #endregion

        #region MpICloseWindowViewModel Implementation

        public bool IsWindowOpen { get; set; }

        #endregion

        #endregion

        #region Properties

        #region ViewModels
        public MpAvFakeWindowViewModel FakeWindowViewModel { get; private set; }
        #endregion

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


        #endregion


        #region Layout
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
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll += Instance_OnGlobalMouseWheelScroll;

            FakeWindowViewModel = new MpAvFakeWindowViewModel(this);
        }


        public void Destroy() {
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove -= Instance_OnGlobalMouseMove;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll -= Instance_OnGlobalMouseWheelScroll;
            IsWindowOpen = false;
            FakeWindowViewModel.Destroy();
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        private void MpAvPointerGestureWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(GestureType):
                    OnPropertyChanged(nameof(MarkerLabel));
                    break;
            }
        }


        protected virtual void Instance_OnGlobalMouseWheelScroll(object sender, MpPoint e) {
            IsInGestureZone = MpAvMainWindowViewModel.IsPointerInTopEdgeZone();
            OnPropertyChanged(nameof(EdgeBrush));
            if (GestureType == MpPointGestureType.ScrollToOpen) {

                CheckForGesture();
            }
        }

        private void Instance_OnGlobalMouseMove(object sender, MpPoint e) {
            IsInGestureZone = MpAvMainWindowViewModel.IsPointerInTopEdgeZone();
            OnPropertyChanged(nameof(EdgeBrush));
            if (GestureType == MpPointGestureType.DragToOpen) {
                CheckForGesture();
            }
        }

        private MpAvWindow CreateTopEdgeMarkerWindow() {
            MpSize ss = MpAvWindowManager.ActiveWindow.Screens.Primary.Bounds.Size.ToPortableSize(MpAvWindowManager.ActiveWindow.Screens.Primary.Scaling);

            var gw = new MpAvWindow() {
                Content = new MpAvTopEdgeMarkerView(),
                DataContext = this,
                Position = new PixelPoint(),
                Width = ss.Width,
                Height = MaxWindowY,
                Background = Brushes.Transparent,//new SolidColorBrush() { Color = Colors.Orange, Opacity = 0.3 },
                WindowState = WindowState.Normal,
                CanResize = false,
                SystemDecorations = SystemDecorations.None,
                TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
            };

#if WINDOWS
            MpAvToolWindow_Win32.InitToolWindow(gw.TryGetPlatformHandle().Handle);
            //MpAvToolWindow_Win32.SetAsNoHitTestWindow(gw.TryGetPlatformHandle().Handle);
#endif
            return gw;
        }
        public void CheckForGesture() {

            if (!IsGesturing || FakeWindowViewModel.FakeWindowActionType == MpFakeWindowActionType.Open) {
                return;
            }

            FakeWindowViewModel.ToggleFakeWindowCommand.Execute(null);
        }

        #endregion

        #region Commands
        public ICommand ShowGestureWindowCommand => new MpCommand(
            () => {
                var gw = CreateTopEdgeMarkerWindow();
                gw.Show();
                FakeWindowViewModel.ToggleFakeWindowCommand.Execute(null);
            });
        #endregion
    }
}
