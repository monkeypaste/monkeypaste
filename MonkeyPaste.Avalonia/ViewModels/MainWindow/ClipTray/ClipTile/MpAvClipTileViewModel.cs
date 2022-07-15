using Avalonia;
using Avalonia.Layout;
using MonkeyPaste.Common;
using System;
using System.IO;
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
                return Path.Combine(Environment.CurrentDirectory, "Resources", "Html", "Editor", "index.html");
            }
        }
        
        #endregion

        #region Model

        public string CopyItemData {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.ItemData;
            }
            set {
                if (CopyItem.ItemData != value) {
                    CopyItem.ItemData = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemData));
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
            LogPropertyChangedEvents = true;

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
