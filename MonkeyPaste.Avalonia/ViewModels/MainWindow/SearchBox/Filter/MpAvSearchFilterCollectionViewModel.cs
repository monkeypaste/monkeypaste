using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchFilterCollectionViewModel : 
        MpViewModelBase<MpAvSearchBoxViewModel>,
        MpIPopupMenuViewModel,
        MpIQueryInfoValueProvider {
        #region Private Variables

        #endregion

        #region Statics

        #endregion

        #region MpIQueryInfoProvider Implementation
        object MpIQueryInfoValueProvider.Source => this;
        string MpIQueryInfoValueProvider.SourcePropertyName => nameof(FilterType);

        string MpIQueryInfoValueProvider.QueryValueName => nameof(MpIQueryInfo.QueryFlags);

        #endregion

        #region MpIPopupMenuViewModel Implementation

        public MpMenuItemViewModel PopupMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    ParentObj = this,
                    SubItems = Filters.Select(x => x.MenuItemViewModel).ToList()
                };
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
                            nameof(MpPrefViewModel.Instance.SearchByContent),
                            MpContentQueryBitFlags.Content),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Title",
                            nameof(MpPrefViewModel.Instance.SearchByTitle),
                            MpContentQueryBitFlags.Title),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Url",
                            nameof(MpPrefViewModel.Instance.SearchBySourceUrl),
                            MpContentQueryBitFlags.Url),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Url Title",
                            nameof(MpPrefViewModel.Instance.SearchByUrlTitle),
                            MpContentQueryBitFlags.UrlTitle),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Application Path",
                            nameof(MpPrefViewModel.Instance.SearchByProcessName),
                            MpContentQueryBitFlags.AppPath),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Application Name",
                            nameof(MpPrefViewModel.Instance.SearchByApplicationName),
                            MpContentQueryBitFlags.AppName),
                        //new MpSearchFilterViewModel(
                        //    this,
                        //    "Collections",
                        //    nameof(MpJsonPreferenceIO.Instance.SearchByTag),
                        //    MpContentFilterType.Tag),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Annotations",
                            nameof(MpPrefViewModel.Instance.SearchByAnnotation),
                            MpContentQueryBitFlags.Annotations),
                        new MpAvSearchFilterViewModel(this,true),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Text Type",
                            nameof(MpPrefViewModel.Instance.SearchByTextType),
                            MpContentQueryBitFlags.TextType),
                        new MpAvSearchFilterViewModel(
                            this,
                            "File Type",
                            nameof(MpPrefViewModel.Instance.SearchByFileType),
                            MpContentQueryBitFlags.FileType),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Image Type",
                            nameof(MpPrefViewModel.Instance.SearchByImageType),
                            MpContentQueryBitFlags.ImageType),
                        new MpAvSearchFilterViewModel(this,true),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Case Sensitive",
                            nameof(MpPrefViewModel.Instance.SearchByIsCaseSensitive),
                            MpContentQueryBitFlags.CaseSensitive),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Whole Word",
                            nameof(MpPrefViewModel.Instance.SearchByWholeWord),
                            MpContentQueryBitFlags.WholeWord),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Regular Expression",
                            nameof(MpPrefViewModel.Instance.SearchByRegex),
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
                if(FilterType != value) {
                    foreach (var sfvm in Filters) {
                        sfvm.IsChecked = value.HasFlag(sfvm.FilterType);
                    }
                }
            }
        }

        public bool IsFilterPopupOpen { get; set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvSearchFilterCollectionViewModel() : this(null) { }
        public MpAvSearchFilterCollectionViewModel(MpAvSearchBoxViewModel parent) : base(parent) {
            PropertyChanged += MpAvSearchFilterCollectionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public void Init() {
            OnPropertyChanged(nameof(Filters));
            MpPlatform.Services.Query.RegisterProvider(this);

            // if regex is set on load, disable others
            //var regex_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByRegex));
            //if (regex_filter.IsChecked.IsTrue()) {
            //    ValidateFilters(regex_filter);
            //}
            foreach (var sfvm in Filters.Where(x => !x.IsSeperator)) {
                sfvm.PropertyChanged += Sfvm_PropertyChanged;
            }
        }
        #endregion

        #region Private Methods

        private void MpAvSearchFilterCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsFilterPopupOpen):
                    if(IsFilterPopupOpen) {
                        break;
                    }
                    MpPrefViewModel.Instance.LastQueryInfoJson = 
                        MpPlatform.Services.Query.SerializeJsonObject();
                    break;
            }
        }
        private void Sfvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var sfvm = sender as MpAvSearchFilterViewModel;
            switch (e.PropertyName) {
                case nameof(sfvm.IsChecked):
                    ValidateFilters(sfvm);
                    if(IsPopupMenuOpen) {
                        // let search update query as filters change
                        Parent.PerformSearchCommand.Execute(null);
                    }
                    break;
            }
        }

        private void ValidateFilters(MpAvSearchFilterViewModel change_fvm) {
            bool needsUpdate = false;
            if (change_fvm.FilterType.HasFlag(MpContentQueryBitFlags.Regex)) {
                // checking regex disables case and whole word, unchecking reenables
                var case_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByIsCaseSensitive));
                var whole_word_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByWholeWord));

                if(change_fvm.IsChecked.IsTrue()) {
                    case_filter.IsChecked = false;
                    whole_word_filter.IsChecked = false;
                    
                    case_filter.IsChecked = null;
                    whole_word_filter.IsChecked = null;
                } else {
                    case_filter.IsChecked = false;
                    whole_word_filter.IsChecked = false;
                }

                needsUpdate = true;
            } else if (change_fvm.IsChecked.IsFalse() && 
                (change_fvm.FilterType.HasFlag(MpContentQueryBitFlags.TextType) ||
                change_fvm.FilterType.HasFlag(MpContentQueryBitFlags.ImageType) ||
                change_fvm.FilterType.HasFlag(MpContentQueryBitFlags.FileType))) {
                // when last content type is unchecked reenable all content types
                var text_type_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByTextType));
                var image_type_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByImageType));
                var file_type_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByFileType));

                if (text_type_filter.IsChecked.IsFalse() && 
                    image_type_filter.IsChecked.IsFalse() && 
                    file_type_filter.IsChecked.IsFalse()) {

                    text_type_filter.IsChecked = true;
                    image_type_filter.IsChecked = true;
                    file_type_filter.IsChecked = true;


                    needsUpdate = true;
                }
            }

            if(needsUpdate) {
                var target = MpAvContextMenuView.Instance.PlacementTarget;
                if(target == null) {
                    return;
                }
                var offset = new MpPoint(MpAvContextMenuView.Instance.HorizontalOffset, MpAvContextMenuView.Instance.VerticalOffset);
                MpAvMenuExtension.CloseMenu();
                MpAvMenuExtension.ShowMenu(target, PopupMenuViewModel,offset,PlacementMode.Pointer);
            }
                
        }
        #endregion

        #region Commands
        #endregion
    }
}
