using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchFilterCollectionViewModel :
        MpAvViewModelBase<MpAvSearchBoxViewModel>,
        MpAvIMenuItemViewModel,
        MpIPopupMenuViewModel {
        #region Private Variables

        #endregion

        #region Statics

        #endregion

        #region MpAvIMenuItemCollectionViewModel Implementation
        ICommand MpAvIMenuItemViewModel.Command { get; }
        object MpAvIMenuItemViewModel.CommandParameter { get; }
        string MpAvIMenuItemViewModel.Header { get; }
        object MpAvIMenuItemViewModel.IconSourceObj { get; }
        string MpAvIMenuItemViewModel.InputGestureText { get; }
        bool MpAvIMenuItemViewModel.StaysOpenOnClick { get; }
        bool MpAvIMenuItemViewModel.HasLeadingSeparator { get; }
        bool MpAvIMenuItemViewModel.IsThreeState { get; }
        bool MpAvIMenuItemViewModel.IsVisible { get; }
        bool MpAvIMenuItemViewModel.IsEnabled => true;
        public bool? IsChecked { get; set; } = false;
        MpMenuItemType MpAvIMenuItemViewModel.MenuItemType { get; }
        bool MpAvIMenuItemViewModel.IsSubMenuOpen {
            get => IsPopupMenuOpen;
            set => IsPopupMenuOpen = value;
        }
        IEnumerable<MpAvIMenuItemViewModel> MpAvIMenuItemViewModel.SubItems =>
            Filters;

        #endregion

        #region MpIPopupMenuViewModel Implementation

        private MpAvMenuItemViewModel _pmivm;
        public MpAvMenuItemViewModel PopupMenuViewModel {
            get {
                if (_pmivm == null) {
                    _pmivm = new MpAvMenuItemViewModel() {
                        ParentObj = this,
                        SubItems = Filters.Select(x => x.MenuItemViewModel).ToList()
                    };
                }
                return _pmivm;
            }
        }

        public bool IsPopupMenuOpen { get; set; }
        #endregion

        #region Properties

        #region ViewModels

        private ObservableCollection<MpAvSearchFilterViewModel> _filters;
        public ObservableCollection<MpAvSearchFilterViewModel> Filters {
            get {
                if (_filters == null) {
                    _filters = new ObservableCollection<MpAvSearchFilterViewModel>() {
                        new MpAvSearchFilterViewModel(
                            this,
                            "Content",
                            nameof(MpAvPrefViewModel.Instance.SearchByContent),
                            MpContentQueryBitFlags.Content),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Title",
                            nameof(MpAvPrefViewModel.Instance.SearchByTitle),
                            MpContentQueryBitFlags.Title),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Url",
                            nameof(MpAvPrefViewModel.Instance.SearchBySourceUrl),
                            MpContentQueryBitFlags.Url),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Url Title",
                            nameof(MpAvPrefViewModel.Instance.SearchByUrlTitle),
                            MpContentQueryBitFlags.UrlTitle),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Application Path",
                            nameof(MpAvPrefViewModel.Instance.SearchByProcessName),
                            MpContentQueryBitFlags.AppPath),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Application Name",
                            nameof(MpAvPrefViewModel.Instance.SearchByApplicationName),
                            MpContentQueryBitFlags.AppName),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Annotations",
                            nameof(MpAvPrefViewModel.Instance.SearchByAnnotation),
                            MpContentQueryBitFlags.Annotations),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Text Type",
                            nameof(MpAvPrefViewModel.Instance.SearchByTextType),
                            MpContentQueryBitFlags.TextType,true),
                        new MpAvSearchFilterViewModel(
                            this,
                            "File Type",
                            nameof(MpAvPrefViewModel.Instance.SearchByFileType),
                            MpContentQueryBitFlags.FileType),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Image Type",
                            nameof(MpAvPrefViewModel.Instance.SearchByImageType),
                            MpContentQueryBitFlags.ImageType),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Case Sensitive",
                            nameof(MpAvPrefViewModel.Instance.SearchByIsCaseSensitive),
                            MpContentQueryBitFlags.CaseSensitive,true),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Whole Word",
                            nameof(MpAvPrefViewModel.Instance.SearchByWholeWord),
                            MpContentQueryBitFlags.WholeWord),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Regular Expression",
                            nameof(MpAvPrefViewModel.Instance.SearchByRegex),
                            MpContentQueryBitFlags.Regex)
                    };
                }
                return _filters;
            }
        }
        #endregion

        #region State

        public MpContentQueryBitFlags FilterType {
            get {
                MpContentQueryBitFlags ft = MpContentQueryBitFlags.None;
                foreach (var sfvm in Filters) {
                    ft |= sfvm.FilterValue;
                }
                return ft;
            }
            set {
                if (FilterType != value) {
                    foreach (var sfvm in Filters) {
                        sfvm.IsChecked = value.HasFlag(sfvm.FilterType);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Constructors
        public MpAvSearchFilterCollectionViewModel() : this(null) { }
        public MpAvSearchFilterCollectionViewModel(MpAvSearchBoxViewModel parent) : base(parent) {
            PropertyChanged += MpAvSearchFilterCollectionViewModel_PropertyChanged;
            ResetFiltersToDefaultCommand.Execute(null);
        }

        #endregion

        #region Public Methods

        public void Init() {
            OnPropertyChanged(nameof(Filters));

            MpMessenger.RegisterGlobal(ReceiveGlobalMessage);
            foreach (var sfvm in Filters) {
                sfvm.PropertyChanged += Sfvm_PropertyChanged;
            }
        }
        #endregion

        #region Private Methods

        private void MpAvSearchFilterCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsPopupMenuOpen):
                    if (IsPopupMenuOpen) {
                        break;
                    }
                    //MpPrefViewModel.Instance.LastQueryInfoJson =
                    //    Mp.Services.Query.SerializeJsonObject();
                    break;
            }
        }
        private void Sfvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var sfvm = sender as MpAvSearchFilterViewModel;
            switch (e.PropertyName) {
                case nameof(sfvm.IsChecked):
                    ValidateFilters(sfvm);
                    if (IsPopupMenuOpen) {
                        // pass focus back to search box to trigger unexpand when clicked away
                        //Parent.IsTextBoxFocused = true;
                        // let search update query as filters change
                        Parent.PerformSearchCommand.Execute(null);
                    }
                    break;
            }
        }
        private void ReceiveGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TagSelectionChanged:
                    var sttvm = MpAvTagTrayViewModel.Instance.SelectedItem;
                    if (sttvm != null &&
                        sttvm.IsLinkTag) {
                        ResetFiltersToDefaultCommand.Execute(null);
                    }
                    break;

            }
        }
        public void ValidateFilters(MpAvSearchFilterViewModel change_fvm) {
            if (change_fvm.FilterType.HasFlag(MpContentQueryBitFlags.Regex)) {
                // checking regex disables case and whole word, unchecking reenables
                var case_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpAvPrefViewModel.Instance.SearchByIsCaseSensitive));
                var whole_word_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpAvPrefViewModel.Instance.SearchByWholeWord));

                if (change_fvm.IsChecked.IsTrue()) {
                    case_filter.IsChecked = null;
                    whole_word_filter.IsChecked = null;
                } else {
                    case_filter.IsChecked = false;
                    whole_word_filter.IsChecked = false;
                }
            } else if (change_fvm.IsChecked.IsFalse() &&
                (change_fvm.FilterType.HasFlag(MpContentQueryBitFlags.TextType) ||
                change_fvm.FilterType.HasFlag(MpContentQueryBitFlags.ImageType) ||
                change_fvm.FilterType.HasFlag(MpContentQueryBitFlags.FileType))) {
                // when last content type is unchecked reenable all content types
                var text_type_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpAvPrefViewModel.Instance.SearchByTextType));
                var image_type_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpAvPrefViewModel.Instance.SearchByImageType));
                var file_type_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpAvPrefViewModel.Instance.SearchByFileType));

                if (text_type_filter.IsChecked.IsFalse() &&
                    image_type_filter.IsChecked.IsFalse() &&
                    file_type_filter.IsChecked.IsFalse()) {

                    text_type_filter.IsChecked = true;
                    image_type_filter.IsChecked = true;
                    file_type_filter.IsChecked = true;
                }
            }
            Filters.ForEach(x => x.OnPropertyChanged(nameof(x.IsEnabled)));
        }
        #endregion

        #region Commands
        public ICommand ResetFiltersToDefaultCommand => new MpCommand(
            () => {
                Filters.ForEach(x => x.IsChecked = MpSearchCriteriaItem.DefaultSimpleFilters.HasFlag(x.FilterType));
            });



        #endregion
    }
}
