using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpContentItemViewModel : 
        MpViewModelBase<MpContentItemCollectionViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpIResizableViewModel,
        MpITextSelectionRange,
        MpIUserColorViewModel {

        #region Properties

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation
        public bool IsSelected { get; set; }
        #endregion

        #region MpIResizableViewModel Implementation
        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion

        #region MpITextSelectionRange Implementation

        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }
        public bool IsAllSelected { get; set; }
        public string SelectedPlainText { get; set; }

        #endregion

        #region MpIUserColorViewModel

        public string UserHexColor { get; set; }

        #endregion

        #region State

        public bool IsPinned { get; set; } = false;

        #endregion

        #region Model

        public bool IsCompositeChild {
            get {
                if (CopyItem == null || base.Parent == null) {
                    return false;
                }
                return CopyItem.CompositeParentCopyItemId > 0 || Parent.Items.Count > 1;
            }
        }

        public DateTime CopyItemCreatedDateTime {
            get {
                if (CopyItem == null) {
                    return DateTime.MinValue;
                }
                return CopyItem.CopyDateTime;
            }
        }

        public string HotkeyIconTooltip {
            get {
                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                    return @"Assign Shortcut";
                }
                return ShortcutKeyString;
            }
        }

        public int CompositeSortOrderIdx {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeSortOrderIdx;
            }
            set {
                if (CopyItem != null && CopyItem.CompositeSortOrderIdx != value) {
                    CopyItem.CompositeSortOrderIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                }
            }
        }

        public int CompositeParentCopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeParentCopyItemId;
            }
            set {
                if (CopyItem != null && CopyItem.CompositeParentCopyItemId != value) {
                    CopyItem.CompositeParentCopyItemId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                }
            }
        }
        public string CopyItemTitle {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Title;
            }
            set {
                if (CopyItem != null && CopyItem.Title != value) {
                    CopyItem.Title = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemTitle));
                }
            }
        }

        public MpCopyItemType CopyItemType {
            get {
                if (CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.ItemType;
            }
            set {
                if (CopyItem != null && CopyItem.ItemType != value) {
                    CopyItem.ItemType = value;
                    OnPropertyChanged(nameof(CopyItemType));
                }
            }
        }

        public string CopyItemGuid {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Guid;
            }
        }

        public string RootCopyItemGuid {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.RootCopyItemGuid;
            }
            set {
                if (RootCopyItemGuid != value) {
                    CopyItem.RootCopyItemGuid = value;
                    OnPropertyChanged(nameof(RootCopyItemGuid));
                }
            }
        }


        public int CopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.Id;
            }
            set {
                if (CopyItem != null && CopyItem.Id != value) {
                    CopyItem.Id = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        public int SourceId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.SourceId;
            }
        }

        public string CopyItemData {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.ItemData;
            }
            set {
                if (CopyItem != null && CopyItem.ItemData != value) {
                    CopyItem.ItemData = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemData));
                    OnPropertyChanged(nameof(CurrentSize));
                }
            }
        }

        public int IconId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                if (CopyItem.IconId > 0) {
                    return CopyItem.IconId;
                }
                if (SourceViewModel == null) {
                    // BUG currently when plugin creates new content it is not setting source info
                    // so return app icon
                    return MpPreferences.ThisAppSource.PrimarySource.IconId;
                }
                return SourceViewModel.PrimarySource.IconId;
            }
            set {
                if (IconId != value) {
                    CopyItem.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        public string CopyItemHexColor {
            get {
                if (CopyItem == null || string.IsNullOrEmpty(CopyItem.ItemColor)) {
                    return MpColorHelpers.GetRandomHexColor();
                }
                return CopyItem.ItemColor;
            }
            set {
                if (CopyItemHexColor != value) {
                    CopyItem.ItemColor = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemHexColor));
                }
            }
        }


        public MpCopyItem CopyItem { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpContentItemViewModel() : base(null) {
            IsBusy = true;
        }

        public MpContentItemViewModel(MpContentItemCollectionViewModel parent) : base(parent) {
            IsBusy = true;
        }


        #endregion

        #region Public Methods

        #endregion
    }
}
