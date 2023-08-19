using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpPointGestureType {
        ScrollToOpen,
        DragToOpen
    };

    public abstract class MpAvPointerGestureWindowViewModel :
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
            MpWindowType.Modal;

        #endregion

        #region MpICloseWindowViewModel Implementation

        public bool IsWindowOpen { get; set; }


        #endregion

        #endregion

        #region Properties

        #region State
        public MpPointGestureType GestureType { get; }
        #endregion

        #region Appearance
        #endregion

        #endregion

        #region Constructors
        public MpAvPointerGestureWindowViewModel() : this(null) { }
        public MpAvPointerGestureWindowViewModel(MpAvWelcomeOptionItemViewModel parent) : base(parent) {
            MpAvShortcutCollectionViewModel.Instance.StartInputListener();
            MpAvShortcutCollectionViewModel.Instance.OnGlobalDrag += Instance_OnGlobalDrag;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll += Instance_OnGlobalMouseWheelScroll;
        }

        public void DetachGestureHandlers() {
            MpAvShortcutCollectionViewModel.Instance.OnGlobalDrag -= Instance_OnGlobalDrag;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseWheelScroll -= Instance_OnGlobalMouseWheelScroll;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods

        protected virtual MpAvWindow CreateGestureWindow() {
            MpSize ss = MpAvWindowManager.ActiveWindow.Screens.Primary.Bounds.Size.ToPortableSize(MpAvWindowManager.ActiveWindow.Screens.Primary.Scaling);

            var gw = new MpAvWindow() {
                DataContext = this,
                Topmost = true,
                Content = GestureType == MpPointGestureType.ScrollToOpen ? new MpAvScrollToOpenGestureView() : new MpAvDragToOpenGestureView(),
                Position = new PixelPoint(),
                Width = ss.Width,
                Height = ss.Height,
                Background = Brushes.Transparent,//new SolidColorBrush() { Color = Colors.Orange, Opacity = 0.3 },
                WindowState = WindowState.FullScreen,
                CanResize = false,
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome,
                ExtendClientAreaToDecorationsHint = true,
                SystemDecorations = SystemDecorations.None,
                ExtendClientAreaTitleBarHeightHint = 0,
                TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
            };
            return gw;
        }
        #endregion

        #region Private Methods



        protected virtual void Instance_OnGlobalMouseWheelScroll(object sender, MpPoint e) {

        }

        protected virtual void Instance_OnGlobalDrag(object sender, MpPoint e) {
            if (!MpAvMainWindowViewModel.CanDragOpen() ||
                GestureType != MpPointGestureType.DragToOpen) {
                return;
            }
            Parent.ToggleOptionCommand.Execute(null);
        }
        #endregion

        #region Commands
        public ICommand ShowGestureWindowCommand => new MpCommand(
            () => {
                var gw = CreateGestureWindow();
                gw.Show();
            });
        #endregion
    }
}
