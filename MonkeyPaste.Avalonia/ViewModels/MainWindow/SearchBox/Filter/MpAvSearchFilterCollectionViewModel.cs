﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        string MpIQueryInfoValueProvider.QueryValueName => nameof(MpAvQueryInfoViewModel.Current.FilterFlags);

        #endregion

        #region MpIPopupMenuViewModel Implementation

        public MpMenuItemViewModel PopupMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = Filters.Select(x => x.MenuItemViewModel).ToList()
                };
            }
        }
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
                            MpContentFilterType.Content),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Title",
                            nameof(MpPrefViewModel.Instance.SearchByTitle),
                            MpContentFilterType.Title),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Url",
                            nameof(MpPrefViewModel.Instance.SearchBySourceUrl),
                            MpContentFilterType.Url),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Url Title",
                            nameof(MpPrefViewModel.Instance.SearchByUrlTitle),
                            MpContentFilterType.UrlTitle),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Application Path",
                            nameof(MpPrefViewModel.Instance.SearchByProcessName),
                            MpContentFilterType.AppPath),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Application Name",
                            nameof(MpPrefViewModel.Instance.SearchByApplicationName),
                            MpContentFilterType.AppName),
                        //new MpSearchFilterViewModel(
                        //    this,
                        //    "Collections",
                        //    nameof(MpJsonPreferenceIO.Instance.SearchByTag),
                        //    MpContentFilterType.Tag),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Description",
                            nameof(MpPrefViewModel.Instance.SearchByDescription),
                            MpContentFilterType.Meta),
                        new MpAvSearchFilterViewModel(this,true),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Text Type",
                            nameof(MpPrefViewModel.Instance.SearchByTextType),
                            MpContentFilterType.TextType),
                        new MpAvSearchFilterViewModel(
                            this,
                            "File Type",
                            nameof(MpPrefViewModel.Instance.SearchByFileType),
                            MpContentFilterType.FileType),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Image Type",
                            nameof(MpPrefViewModel.Instance.SearchByImageType),
                            MpContentFilterType.ImageType),
                        new MpAvSearchFilterViewModel(this,true),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Case Sensitive",
                            nameof(MpPrefViewModel.Instance.SearchByIsCaseSensitive),
                            MpContentFilterType.CaseSensitive),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Whole Word",
                            nameof(MpPrefViewModel.Instance.SearchByWholeWord),
                            MpContentFilterType.WholeWord),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Regular Expression",
                            nameof(MpPrefViewModel.Instance.SearchByRegex),
                            MpContentFilterType.Regex)
                    };
                }
                return _filters;
            }
        }
        #endregion

        #region State

        public MpContentFilterType FilterType {
            get {
                MpContentFilterType ft = MpContentFilterType.None;
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
            MpAvQueryInfoViewModel.Current.RegisterProvider(this);

            ValidateFilters();
            foreach (var sfvm in Filters.Where(x => !x.IsSeperator)) {
                sfvm.PropertyChanged += Sfvm_PropertyChanged;
            }
        }
        #endregion

        #region Private Methods

        private void MpAvSearchFilterCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsFilterPopupOpen):
                    MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen = IsFilterPopupOpen;
                    break;
            }
        }
        private void Sfvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var sfvm = sender as MpAvSearchFilterViewModel;
            switch (e.PropertyName) {
                case nameof(sfvm.IsChecked):
                    ValidateFilters();
                    break;
            }
        }

        private void ValidateFilters() {
            //var regex_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByRegex));
            //var case_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByIsCaseSensitive));
            //var whole_word_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByWholeWord));

            //if (regex_filter.IsChecked.IsTrue()) {
            //    case_filter.IsChecked = null;
            //    whole_word_filter.IsChecked = null;
            //} else {
            //    case_filter.IsChecked = false;
            //}

            //var text_type_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByTextType));
            //var image_type_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByImageType));
            //var file_type_filter = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByFileType));

            //if(text_type_filter.IsChecked.IsFalse() && image_type_filter.IsChecked.IsFalse() && file_type_filter.IsChecked.IsFalse()) {
            //    text_type_filter.IsChecked = true;
            //    image_type_filter.IsChecked = true;
            //    file_type_filter.IsChecked = true;
            //}
        }
        #endregion

        #region Commands
        #endregion
    }
}