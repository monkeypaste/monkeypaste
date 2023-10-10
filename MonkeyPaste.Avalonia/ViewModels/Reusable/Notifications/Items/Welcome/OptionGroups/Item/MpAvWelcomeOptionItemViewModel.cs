using MonkeyPaste.Common;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvWelcomeOptionItemViewModel :
        MpAvViewModelBase<MpAvWelcomeNotificationViewModel> {
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

        public bool IsGestureItem =>
            Parent != null &&
            (Parent.CurPageType == MpWelcomePageType.ScrollWheel ||
                    Parent.CurPageType == MpWelcomePageType.DragToOpen);

        public bool IsMultiRadioItem =>
            Parent != null &&
            Parent.CurOptGroupViewModel.Items.Count > 1;

        public bool IsHovering { get; set; }
        public bool IsChecked { get; set; }
        public object OptionId { get; set; }
        public bool IsHitTestable {
            get {
                if (Parent == null) {
                    return false;
                }
                //if (IsGestureItem) {
                //    // NOTE assumes gesture is 1 item
                //    // allow click uncheck
                //    // disable click check
                //    return IsChecked;
                //}
                if (IsMultiRadioItem) {
                    // disable unchecking checked
                    return !IsChecked;
                }
                return true;
            }
        }
        #endregion

        #region Appearance

        #region Icon
        public object UncheckedIconSourceObj { get; set; }
        public object CheckedIconSourceObj { get; set; }

        private object _iconSourceObj;
        public object IconSourceObj {
            get {
                if (_iconSourceObj != null) {
                    return _iconSourceObj;
                }
                return IsChecked ? CheckedIconSourceObj : UncheckedIconSourceObj;
            }
            set {
                if (_iconSourceObj != value) {
                    _iconSourceObj = value;
                    OnPropertyChanged(nameof(IconSourceObj));
                }
            }
        }
        #endregion

        #region Label
        public string UncheckedLabelText { get; set; }
        public string CheckedLabelText { get; set; }

        private string _descriptionText;
        public string LabelText {
            get {
                if (!string.IsNullOrEmpty(_descriptionText)) {
                    return _descriptionText;
                }
                return IsChecked ? CheckedLabelText : UncheckedLabelText;
            }
            set {
                if (_descriptionText != value) {
                    _descriptionText = value;
                    OnPropertyChanged(nameof(LabelText));
                }
            }
        }
        #endregion

        #region Description
        public string UncheckedDescriptionText { get; set; }
        public string CheckedDescriptionText { get; set; }

        private string _labelText;
        public string DescriptionText {
            get {
                if (!string.IsNullOrEmpty(_labelText)) {
                    return _labelText;
                }
                return IsChecked ? CheckedDescriptionText : UncheckedDescriptionText;
            }
            set {
                if (_labelText != value) {
                    _labelText = value;
                    OnPropertyChanged(nameof(DescriptionText));
                }
            }
        }
        #endregion
        public string DescriptionText2 { get; set; }

        #endregion
        #endregion

        #region Constructors

        public MpAvWelcomeOptionItemViewModel() : base(null) { }
        public MpAvWelcomeOptionItemViewModel(MpAvWelcomeNotificationViewModel parent, object optId) : base(parent) {
            PropertyChanged += MpAvGestureProfileItemViewModel_PropertyChanged;
            OptionId = optId;
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
                case nameof(IsChecked):

                    OnPropertyChanged(nameof(IsHitTestable));
                    OnPropertyChanged(nameof(LabelText));
                    OnPropertyChanged(nameof(DescriptionText));
                    OnPropertyChanged(nameof(IconSourceObj));

                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.PrimaryItem));
                    }
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand CheckOptionCommand => new MpCommand<object>(
            (args) => {
                Parent.CurOptGroupViewModel.Items.ForEach(x => x.IsChecked = x == this);
            }, (args) => {
                return !IsChecked;
            });

        public ICommand UncheckOptionCommand => new MpCommand<object>(
            (args) => {
                IsChecked = false;
            }, (args) => {
                return IsChecked && !IsMultiRadioItem;
            });
        public ICommand ToggleOptionCommand => new MpCommand<object>(
            (args) => {
                if (IsChecked) {
                    UncheckOptionCommand.Execute(args);
                } else {
                    CheckOptionCommand.Execute(args);
                }
            });

        #endregion


    }
}
