using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileViewModel : MpViewModelBase<MpAvClipTrayViewModel>,
        MpISelectableViewModel,
        MpISelectorItemViewModel<MpAvClipTileViewModel>,
        MpIHoverableViewModel,
        MpIResizableViewModel {
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
                    return MpSystemColors.Red;
                }
                if (Parent.HasScrollVelocity || Parent.HasScrollVelocity) {
                    return MpSystemColors.blue1;
                }
                if (IsHovering) {
                    return MpSystemColors.Yellow;
                }
                return MpSystemColors.blue1;
            }
        }

        #endregion

        #region Layout

        public double Spacing => 5;
        public double MinSize {
            get {

                if (Parent == null) {
                    return 0;
                }
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return Parent.ClipTrayScreenHeight * Parent.ZoomFactor;
                } else {
                    return Parent.ClipTrayScreenWidth * Parent.ZoomFactor;
                }
            }
        }

        public double TrayX {
            get {
                if (Parent == null) {
                    return 0;
                }
                return (MinSize * QueryOffsetIdx + ((QueryOffsetIdx + 1) * (Spacing * 2)));
            }
        }

        public double TrayY => 0;
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
            IsBusy = true;

            await Task.Delay(1);
            QueryOffsetIdx = queryOffsetIdx;

            CopyItem = ci;

            OnPropertyChanged(nameof(TrayX));
            OnPropertyChanged(nameof(TrayY));

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpAvClipTileViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                //case nameof(MinSize):
                //    if(Parent == null) {
                //        break;
                //    }
                //    Parent.OnPropertyChanged(nameof(Parent.ClipTrayTotalWidth));
                //    break;
            }
        }



        #endregion
    }
}
