using Avalonia;
using Avalonia.Layout;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileViewModel : MpViewModelBase<MpAvClipTrayViewModel>,
        MpISelectableViewModel,
        MpISelectorItemViewModel<MpAvClipTileViewModel>,
        MpIHoverableViewModel,
        MpIResizableViewModel {

        #region Constants

        public const double MIN_SIZE_ZOOM_FACTOR_COEFF = (double)1 / (double)7;

        #endregion

        #region Properties

        #region MpISelectableViewModel Implementation
        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region MpISelectorItemViewModel<MpAvClipTileViewModel> Implementation
        MpISelectorViewModel<MpAvClipTileViewModel> MpISelectorItemViewModel<MpAvClipTileViewModel>.Selector => Parent;

        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }

        #endregion

        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion

        #region ViewModels
        //public ObservableCollection<MpFileItemViewModel> FileItems { get; set; } = new ObservableCollection<MpFileItemViewModel>();

        //public MpImageAnnotationCollectionViewModel DetectedImageObjectCollectionViewModel { get; set; }

        public MpAvClipTileTitleSwirlViewModel TitleSwirlViewModel { get; set; }

        //public MpTemplateCollectionViewModel TemplateCollection { get; set; }

        //public MpContentTableViewModel TableViewModel { get; set; }

        public MpSourceViewModel SourceViewModel {
            get {
                //if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                //    return null;
                //}
                var svm = MpSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == SourceId);
                if (svm == null) {
                    return MpSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == MpPrefViewModel.Instance.ThisAppSourceId);
                }
                return svm;
            }
        }

        public MpAppViewModel AppViewModel => SourceViewModel.AppViewModel;

        public MpUrlViewModel UrlViewModel => SourceViewModel.UrlViewModel;

        //public MpAvClipTileViewModel Next {
        //    get {
        //        if (IsPlaceholder || Parent == null) {
        //            return null;
        //        }
        //        if (IsPinned) {
        //            int pinIdx = Parent.PinnedItems.IndexOf(this);
        //            return Parent.PinnedItems.FirstOrDefault(x => Parent.PinnedItems.IndexOf(x) == pinIdx + 1);
        //        }
        //        return Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == QueryOffsetIdx + 1);
        //    }
        //}

        //public MpAvClipTileViewModel Prev {
        //    get {
        //        if (IsPlaceholder || Parent == null || QueryOffsetIdx == 0) {
        //            return null;
        //        }
        //        if (IsPinned) {
        //            int pinIdx = Parent.PinnedItems.IndexOf(this);
        //            return Parent.PinnedItems.FirstOrDefault(x => Parent.PinnedItems.IndexOf(x) == pinIdx - 1);
        //        }
        //        return Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == QueryOffsetIdx - 1);
        //    }
        //}

        public MpColorPalletePopupMenuViewModel SelectionFgColorPopupViewModel { get; private set; } = new MpColorPalletePopupMenuViewModel();

        public MpColorPalletePopupMenuViewModel SelectionBgColorPopupViewModel { get; private set; } = new MpColorPalletePopupMenuViewModel();


        #endregion

        #region Appearance

        public string TileBorderHexColor {
            get {
                if (IsResizing) {
                    return MpSystemColors.pink;
                }
                if (CanResize) {
                    return MpSystemColors.orange1;
                }
                if (IsSelected) {
                    return MpSystemColors.Red;//.AdjustAlpha(0.7);
                }
                if (Parent.HasScrollVelocity || Parent.HasScrollVelocity) {
                    return MpSystemColors.Transparent;
                }
                if (IsHovering) {
                    return MpSystemColors.Yellow;//.AdjustAlpha(0.7);
                }
                return MpSystemColors.Transparent;
            }
        }

        #endregion

        #region Layout

        public double OuterSpacing => 5;
        public double InnerSpacing => 0;
        public double MinSize {
            get {
                double minSize = 0;
                if (Parent == null) {
                    return minSize;
                }
                if (Parent.LayoutType == MpAvClipTrayLayoutType.Stack) {
                    minSize = Parent.ListOrientation == Orientation.Horizontal ?
                                    (Parent.ClipTrayScreenHeight * Parent.ZoomFactor) :
                                    (Parent.ClipTrayScreenWidth * Parent.ZoomFactor);
                } else {
                    minSize = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds.Width * 
                                Parent.ZoomFactor * MIN_SIZE_ZOOM_FACTOR_COEFF;
                }
                //minSize = ();

                return minSize;
            }
        }

        public double TrayX {
            get {
                double trayX = 0;
                if (Parent == null) {
                    return trayX;
                }
                trayX = MinSize * ColIdx;

                return trayX;
            }
        }

        public double TrayY {
            get {
                double trayY = 0;
                if (Parent == null) {
                    return trayY;
                }
                trayY = MinSize * RowIdx;
                return trayY;
            }
        }

        public Rect TrayRect => new Rect(TrayX, TrayY, MinSize, MinSize);

        #endregion

        #region State

        public bool IsTitleReadOnly { get; set; } = true;
        public bool IsContentReadOnly { get; set; } = true;

        public bool IsSubSelectionEnabled { get; set; } = false;

        public bool IsVerticalScrollbarVisibile {
            get {
                if (IsContentReadOnly && !IsSubSelectionEnabled) {
                    return false;
                }
                // true makes auto
                return true;
                //return EditableContentSize.Height > ContentHeight;
            }
        }

        public int QueryOffsetIdx { get; set; }

        public int RowIdx {
            get {
                int rowIdx = 0;
                if(Parent == null) {
                    return rowIdx;
                }
                if(Parent.LayoutType == MpAvClipTrayLayoutType.Stack) {
                    rowIdx = Parent.ListOrientation == Orientation.Horizontal ?
                                    0 : QueryOffsetIdx;                    
                } else {
                    rowIdx = (int)((double)QueryOffsetIdx / (double)Parent.ColCount);
                }

                return rowIdx;
            }
        }

        public int ColIdx {
            get {
                int colIdx = 0;
                if (Parent == null) {
                    return colIdx;
                }
                if (Parent.LayoutType == MpAvClipTrayLayoutType.Stack) {
                    colIdx = Parent.ListOrientation == Orientation.Horizontal ?
                                    QueryOffsetIdx : 0;                    
                } else {
                    colIdx = QueryOffsetIdx - (RowIdx * Parent.ColCount);
                }                
                
                return colIdx;
            }
        }
        public bool IsVisible {
            get {
                //if (Parent == null) {
                //    return false;
                //}
                //double screenX = TrayX - Parent.ScrollOffset;
                //return screenX >= 0 &&
                //       screenX < Parent.ClipTrayScreenWidth &&
                //       screenX + TileBorderWidth <= Parent.ClipTrayScreenWidth;
                return true;
            }
        }

        public string EditorPath {
            get {
                //file:///Volumes/BOOTCAMP/Users/tkefauver/Source/Repos/MonkeyPaste/MonkeyPaste/Resources/Html/Editor/index.html
                string editorPath = Path.Combine(Environment.CurrentDirectory, "Resources", "Html", "Editor", "index.html");
                if(OperatingSystem.IsWindows()) {
                    return editorPath;
                }
                var uri = new Uri(editorPath, UriKind.Absolute);
                string uriStr = uri.AbsoluteUri;
                return uriStr;
            }
        }

        #endregion


        #region Model

        public Size UnformattedContentSize {
            get {
                if (CopyItem == null) {
                    return new Size();
                }
                return CopyItem.ItemSize.ToAvSize();
            }
            set {
                if (UnformattedContentSize != value) {
                    CopyItem.ItemSize = value.ToPortableSize();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(UnformattedContentSize));
                }
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

        public MpPortableDataFormat PreferredFormat {
            get {
                if (CopyItem == null) {
                    return null;
                }
                return CopyItem.PreferredFormat;
            }
            set {
                if (CopyItem != null && CopyItem.PreferredFormat != value) {
                    CopyItem.PreferredFormat = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PreferredFormat));
                }
            }
        }

        public MpCopyItemType ItemType {
            get {
                if (CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.ItemType;
            }
            set {
                if (CopyItem != null && CopyItem.ItemType != value) {
                    CopyItem.ItemType = value;
                    OnPropertyChanged(nameof(ItemType));
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

                    return MpPrefViewModel.Instance.ThisAppIcon.Id;
                }
                return SourceViewModel.PrimarySourceViewModel.IconId;
            }
            set {
                if (IconId != value) {
                    CopyItem.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        private string _curItemRandomHexColor;
        public string CopyItemHexColor {
            get {
                if (CopyItem == null || string.IsNullOrEmpty(CopyItem.ItemColor)) {
                    if (string.IsNullOrEmpty(_curItemRandomHexColor)) {
                        _curItemRandomHexColor = MpColorHelpers.GetRandomHexColor();
                    }
                    return _curItemRandomHexColor;
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

        #region Contructors
        public MpAvClipTileViewModel() : base(null) { }

        public MpAvClipTileViewModel(MpAvClipTrayViewModel parent) : base(parent) {
            PropertyChanged += MpAvClipTileViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpCopyItem ci, int queryOffsetIdx = -1) {
            LogPropertyChangedEvents = false;

            IsBusy = true;

            await Task.Delay(1);
            QueryOffsetIdx = queryOffsetIdx;

            CopyItem = ci;

            OnPropertyChanged(nameof(TrayX));
            OnPropertyChanged(nameof(TrayY));

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            IsBusy = false;
        }

        public override string ToString() {
            return $"Tile[{QueryOffsetIdx}]";
        }
        #endregion

        #region Private Methods

        private void MpAvClipTileViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
            }
        }

        #endregion
    }
}
