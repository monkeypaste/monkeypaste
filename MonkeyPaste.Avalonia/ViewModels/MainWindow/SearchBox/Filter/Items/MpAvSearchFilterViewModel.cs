﻿using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchFilterViewModel :
        MpAvViewModelBase<MpAvSearchFilterCollectionViewModel>,
        MpAvIMenuItemViewModel {
        #region Private Variables

        private MpContentQueryBitFlags _filterType = MpContentQueryBitFlags.None;

        #endregion

        #region Interfaces

        #region MpAvIMenuItemViewModel Implementation

        string MpAvIMenuItemViewModel.IconTintHexStr =>
            null;
        string MpAvIMenuItemViewModel.IconBorderHexColor =>
            MpSystemColors.Transparent;
        public bool IsHovering { get; set; }
        bool MpAvIMenuItemViewModel.IsSubMenuOpen { get; set; }
        ICommand MpAvIMenuItemViewModel.Command => ToggleIsCheckedCommand;
        object MpAvIMenuItemViewModel.CommandParameter => null;
        string MpAvIMenuItemViewModel.Header => Label;
        object MpAvIMenuItemViewModel.IconSourceObj => null;
        string MpAvIMenuItemViewModel.InputGestureText => null;
        public MpMenuItemType MenuItemType =>
            MpMenuItemType.Checkable;
        bool MpAvIMenuItemViewModel.StaysOpenOnClick =>
            true;
        bool MpAvIIsVisibleViewModel.IsVisible =>
            true;
        bool MpAvIMenuItemViewModel.IsThreeState =>
            false;

        IEnumerable<MpAvIMenuItemViewModel> MpAvIMenuItemViewModel.SubItems =>
            null;

        public bool HasLeadingSeparator { get; set; }
        #endregion

        #endregion

        #region Properties

        private MpAvMenuItemViewModel _mivm;
        public MpAvMenuItemViewModel MenuItemViewModel {
            get {
                //if (IsSeparator) {
                //    return new MpAvMenuItemViewModel() {
                //        IsSeparator = true
                //    };
                //}
                if (_mivm == null) {
                    _mivm = new MpAvMenuItemViewModel() {
                        Identifier = this,
                        IsCheckedSrcObj = this,
                        IsCheckedPropPath = nameof(IsChecked),

                        //CommandSrcObj = this,
                        //CommandPath = nameof(ToggleIsCheckedCommand),
                        Header = Label,
                        IconBorderHexColor = MpSystemColors.Black,
                        IconSrcBindingObj = this,
                        IconPropPath = nameof(CheckBoxBgHexStr),
                        CheckedResourceSrcObj = this,
                        CheckedResourcePropPath = nameof(CheckedResourceObj),


                        IsChecked = IsChecked,
                        Command = ToggleIsCheckedCommand,
                        IconHexStr = CheckBoxBgHexStr,
                    };
                }
                return _mivm;
            }
        }

        #region Appearance

        public string Label { get; set; }

        public string CheckBoxBgHexStr =>
            IsEnabled ? MpSystemColors.White : MpSystemColors.Gray;

        public string CheckedResourceObj =>
            IsChecked.HasValue && IsChecked.Value ? "CheckSvg" : null;// MpPlatform.Services.PlatformResource.GetResource("CheckSvg") as string : null;
        #endregion

        #region Model

        public bool? IsChecked { get; set; }

        public bool IsEnabled =>
            IsChecked != null;

        public string PreferenceName { get; set; }

        public MpContentQueryBitFlags FilterType => _filterType;

        public MpContentQueryBitFlags FilterValue => IsChecked.IsTrue() ? _filterType : MpContentQueryBitFlags.None;

        #endregion

        #endregion

        #region Constructors

        public MpAvSearchFilterViewModel() : base(null) { }


        public MpAvSearchFilterViewModel(MpAvSearchFilterCollectionViewModel parent, string label, string prefName, MpContentQueryBitFlags filterType, bool hasLeadingSeparator = false) : base(parent) {
            PropertyChanged += MpSearchFilterViewModel_PropertyChanged;
            HasLeadingSeparator = hasLeadingSeparator;
            _filterType = filterType;
            Label = label;
            PreferenceName = prefName;
            if (MpAvPrefViewModel.Instance[PreferenceName] == null) {
                IsChecked = false;
            } else {
                IsChecked = (bool)MpAvPrefViewModel.Instance[PreferenceName];
            }

        }

        #endregion

        #region Private Methods

        private void MpSearchFilterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsChecked):
                    OnPropertyChanged(nameof(CheckBoxBgHexStr));
                    OnPropertyChanged(nameof(CheckedResourceObj));
                    OnPropertyChanged(nameof(IsEnabled));
                    OnPropertyChanged(nameof(ToggleIsCheckedCommand));
                    OnPropertyChanged(nameof(MenuItemViewModel));
                    Parent.OnPropertyChanged(nameof(Parent.PopupMenuViewModel));
                    MpAvPrefViewModel.Instance[PreferenceName] = IsChecked;
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ToggleIsCheckedCommand => new MpCommand(
            () => {
                IsChecked = !IsChecked;
                Parent.PopupMenuViewModel.SubItems.ForEach(x => x.OnPropertyChanged(nameof(MenuItemViewModel.CheckedResourcePropPath)));
                Parent.PopupMenuViewModel.SubItems.ForEach(x => x.OnPropertyChanged(nameof(MenuItemViewModel.IconPropPath)));
            }, () => {
                return IsEnabled;
            });

        #endregion
    }
}
