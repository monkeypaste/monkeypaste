
using MpResources = MonkeyPaste.Avalonia.Resources.Locales.Resources;

namespace MonkeyPaste.Avalonia {
    public class MpAvGestureProfileItemViewModel :
        MpViewModelBase<MpAvGestureProfileCollectionViewModel>,
        MpIHoverableViewModel,
        MpIHasIconSourceObjViewModel,
        MpILabelTextViewModel,
        MpIDescriptionTextViewModel {
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
        public bool IsHovering { get; set; }
        public bool IsChecked { get; set; }
        #endregion

        #region Appearance
        public object IconSourceObj {
            get {
                switch (ProfileType) {
                    case MpShortcutRoutingProfileType.Global:
                        return "GlobeImage";
                    case MpShortcutRoutingProfileType.Internal:
                        return "PrivateImage";
                    default:
                        return string.Empty;
                }
            }
        }
        public string LabelText =>
            ProfileType.ToString();
        public string DescriptionText {
            get {
                switch (ProfileType) {
                    case MpShortcutRoutingProfileType.Global:
                        return "All interop shortcuts (those useful outside of MonkeyPaste) are enabled by default. Pressing Caps Lock (without any other key) will show or hide the interface at anytime (not currently supported on Linux)";
                    case MpShortcutRoutingProfileType.Internal:
                        return "No global shortcuts are enabled by default.";
                    default:
                        return string.Empty;
                }
            }
        }
        #endregion

        #region Model
        public MpShortcutRoutingProfileType ProfileType { get; set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvGestureProfileItemViewModel() : this(null, MpShortcutRoutingProfileType.None) { }
        public MpAvGestureProfileItemViewModel(MpAvGestureProfileCollectionViewModel parent, MpShortcutRoutingProfileType profileType) : base(parent) {
            PropertyChanged += MpAvGestureProfileItemViewModel_PropertyChanged;
            ProfileType = profileType;
            IsChecked = ProfileType == MpPrefViewModel.Instance.InitialStartupRoutingProfileType;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvGestureProfileItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.HoverItem));
                    }
                    break;
            }
        }
        #endregion

        #region Commands
        #endregion


    }
}
