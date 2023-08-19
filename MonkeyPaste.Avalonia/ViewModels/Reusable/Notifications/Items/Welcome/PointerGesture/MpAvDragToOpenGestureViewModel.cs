using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvDragToOpenGestureViewModel :
        MpAvPointerGestureWindowViewModel,
        MpIMovableViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIMovableViewModel Implementation
        public int MovableId { get; } = 1;
        public bool IsMoving { get; set; }
        public bool CanMove { get; set; } = true;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; } = 50;
        public double Height { get; } = 50;
        #endregion

        #endregion

        #region Properties
        #region State
        public bool DoShow { get; set; }
        public bool IsPlaceholderMainWindowVisible { get; set; }
        #endregion
        #endregion

        #region Constructors
        public MpAvDragToOpenGestureViewModel() : base(null) { }
        public MpAvDragToOpenGestureViewModel(MpAvWelcomeOptionItemViewModel parent) : base(parent) {
            PropertyChanged += MpAvDragToOpenGestureViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        protected override void Instance_OnGlobalMouseWheelScroll(object sender, MpPoint e) {
            base.Instance_OnGlobalMouseWheelScroll(sender, e);
            if (!MpAvMainWindowViewModel.CanScrollOpen()) {
                return;
            }
            DoShow = true;
            Parent.ToggleOptionCommand.Execute(null);
        }
        #endregion

        #region Private Methods

        private void MpAvDragToOpenGestureViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
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
