using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvScrollToOpenGestureViewModel : MpAvPointerGestureWindowViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #region State
        public bool DoShow { get; set; }
        public bool IsPlaceholderMainWindowVisible { get; set; }
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
        protected override void Instance_OnGlobalMouseWheelScroll(object sender, MpPoint e) {
            if (Parent.IsChecked) {
                return;
            }
            base.Instance_OnGlobalMouseWheelScroll(sender, e);
            if (!MpAvMainWindowViewModel.CanScrollOpen()) {
                return;
            }
            DoShow = true;
            Parent.ToggleOptionCommand.Execute(null);
        }

        protected override MpAvWindow CreateGestureWindow() {
            var gw = base.CreateGestureWindow();
            var handle = gw.TryGetPlatformHandle().Handle;
#if WINDOWS
            MpAvToolWindow_Win32.SetAsNoHitTestWindow(handle);
#endif
            // for mac see https://stackoverflow.com/a/46025498/105028

            return gw;
        }
        #endregion

        #region Private Methods

        private void MpAvScrollToOpenGestureViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(DoShow):
                    if (DoShow) {
                        IsPlaceholderMainWindowVisible = true;
                    }
                    break;
                case nameof(IsPlaceholderMainWindowVisible):
                    if (IsPlaceholderMainWindowVisible) {
                        MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseClicked += Instance_OnGlobalMouseClicked;
                    }
                    break;
                case nameof(IsWindowOpen):
                    DoShow = false;
                    IsPlaceholderMainWindowVisible = false;
                    break;
            }
        }

        private void Instance_OnGlobalMouseClicked(object sender, bool e) {
            IsPlaceholderMainWindowVisible = false;
        }
        #endregion

        #region Commands
        #endregion


    }
}
