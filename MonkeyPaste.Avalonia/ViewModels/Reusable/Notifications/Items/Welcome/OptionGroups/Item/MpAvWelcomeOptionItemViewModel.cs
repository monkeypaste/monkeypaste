using MonkeyPaste.Common;
using System.Threading.Tasks;
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
        public bool IsOptionVisible { get; set; } = true;
        public bool IsGestureItem =>
            Parent != null &&
            (Parent.CurPageType == MpWelcomePageType.ScrollWheel ||
                    Parent.CurPageType == MpWelcomePageType.DragToOpen);

        public bool IsMultiRadioItem =>
            Parent != null &&
            Parent.CurOptGroupViewModel != null &&
            Parent.CurOptGroupViewModel.Items != null &&
            Parent.CurOptGroupViewModel.Items.Count > 1;

        public bool IsAccountItem =>
            Parent != null &&
            Parent.IsAccountOptSelected &&
            Parent.CurOptGroupViewModel.Items != null;

        public bool IsExistingAccountItem =>
            IsAccountItem &&
            (Parent.CurOptGroupViewModel.Items.IndexOf(this) == 0 ||
             Parent.CurOptGroupViewModel.Items.IndexOf(this) == 4);

        public bool IsStandardAccountItem =>
            IsAccountItem &&
            (Parent.CurOptGroupViewModel.Items.IndexOf(this) == 2 ||
             Parent.CurOptGroupViewModel.Items.IndexOf(this) == 6);

        public bool IsUnlimitedAccountItem =>
            IsAccountItem &&
            (Parent.CurOptGroupViewModel.Items.IndexOf(this) == 3 ||
             Parent.CurOptGroupViewModel.Items.IndexOf(this) == 7);

        public bool IsHovering { get; set; }
        public bool IsChecked { get; set; }
        public bool IsEnabled { get; set; } = true;
        public object OptionId { get; set; }

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

        public string LabelText2 { get; set; }
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
                    OnPropertyChanged(nameof(LabelText));
                    OnPropertyChanged(nameof(DescriptionText));
                    OnPropertyChanged(nameof(IconSourceObj));

                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.PrimaryItem));
                    }
                    break;
                case nameof(IsChecked):
                    if (IsChecked) {
                        if (IsExistingAccountItem) {
                            ShowExistingAccountFormAsync().FireAndForgetSafeAsync();
                        }
                    } else {

                    }
                    OnPropertyChanged(nameof(LabelText));
                    OnPropertyChanged(nameof(DescriptionText));
                    OnPropertyChanged(nameof(IconSourceObj));

                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.PrimaryItem));
                    }
                    break;
                case nameof(OptionId):
                    OnPropertyChanged(nameof(IsAccountItem));
                    OnPropertyChanged(nameof(IsExistingAccountItem));
                    OnPropertyChanged(nameof(IsStandardAccountItem));
                    OnPropertyChanged(nameof(IsUnlimitedAccountItem));
                    break;
                case nameof(IsEnabled):
                    if (IsEnabled) {

                    } else {

                    }
                    break;

            }
        }

        private async Task ShowExistingAccountFormAsync() {
            IsBusy = true;
            await MpAvAccountViewModel.Instance.ShowExistingAccountLoginWindowAsync();
            IsBusy = false;

            //switch (result_uat) {
            //    case MpUserAccountType.None:
            //        // cancel (ensure acct btns work)
            //        Parent.CurOptGroupViewModel.Items.ForEach(x => x.IsEnabled = true);
            //        Parent.IsAccountMonthToggleEnabled = true;

            //        // get cur unlim acct type item idx
            //        int unlim_item_idx = (int)MpUserAccountType.Unlimited;
            //        if (MpAvAccountViewModel.Instance.IsMonthly) {
            //            unlim_item_idx += 4;
            //        }
            //        // put selection back to unlim

            //        Parent.CurOptGroupViewModel.Items
            //            .ForEach((x, idx) => x.IsChecked = idx == unlim_item_idx);
            //        break;
            //    default:
            //        // logged in

            //        // get acct type item idx
            //        int uat_item_idx = (int)result_uat;
            //        if (MpAvAccountViewModel.Instance.IsMonthly) {
            //            uat_item_idx += 4;
            //        }

            //        // toggle monthly to acct type
            //        Parent.IsAccountMonthlyChecked = MpAvAccountViewModel.Instance.IsMonthly;
            //        // select acct type
            //        Parent.CurOptGroupViewModel.Items
            //            .ForEach((x, idx) => x.IsChecked = idx == uat_item_idx);
            //        // disable everything
            //        Parent.CurOptGroupViewModel.Items
            //            .ForEach((x, idx) => x.IsEnabled = false);
            //        Parent.IsAccountMonthToggleEnabled = false;
            //        break;
            //}
        }
        #endregion

        #region Commands
        public ICommand CheckOptionCommand => new MpCommand(
            () => {
                if (IsChecked) {
                    return;
                }
                Parent.CurOptGroupViewModel.SelectedItem = this;
            }, () => {
                return IsEnabled;
            });

        #endregion


    }
}
