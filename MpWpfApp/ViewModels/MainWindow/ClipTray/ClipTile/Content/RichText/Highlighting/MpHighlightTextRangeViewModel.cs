using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpHighlightTextRangeViewModel : MpUndoableViewModelBase<MpHighlightTextRangeViewModel>, IComparable<MpHighlightTextRangeViewModel> {
        #region Private Variables

        #endregion

        #region View Models
        private MpClipTileViewModel _clipTileViewModel;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }
        #endregion

        #region Properties
        public Brush HighlightBrush {
            get {
                if(IsSelected) {
                    return Brushes.Pink;
                }
                return Brushes.Yellow;
            }
        } 

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if(_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(HighlightBrush));
                }
            }
        }

        private int _contentId = 0;
        public int ContentId {
            get {
                return _contentId;
            }
            set {
                if (_contentId != value) {
                    _contentId = value;
                    OnPropertyChanged(nameof(ContentId));
                }
            }
        }

        private TextRange _range = null;
        public TextRange Range {
            get {
                return _range;
            }
            set {
                if(_range != value) {
                    _range = value;
                    OnPropertyChanged(nameof(Range));
                }
            }
        }

        public bool IsTitleRange {
            get {
                return ContentId == 0;
            }
        }
        #endregion

        #region Public Methods
        public MpHighlightTextRangeViewModel() : this(null,null,-1) { }

        public MpHighlightTextRangeViewModel(MpClipTileViewModel ctvm, TextRange tr, int contentId) {
            ClipTileViewModel = ctvm;
            Range = tr;
            ContentId = contentId;
        }

        public void HighlightRange() {
            Range.ApplyPropertyValue(TextElement.BackgroundProperty, HighlightBrush);
            if (IsSelected && !IsTitleRange) {
                var characterRect = Range.End.GetCharacterRect(LogicalDirection.Forward);
                switch (ClipTileViewModel.CopyItemType) {
                    case MpCopyItemType.Composite:
                    case MpCopyItemType.RichText:
                        var rtb = ClipTileViewModel.RichTextBoxViewModelCollection[ContentId - 1].Rtb;
                        ClipTileViewModel.RichTextBoxListBox.ScrollIntoView(rtb);
                        rtb.ScrollToHorizontalOffset(rtb.HorizontalOffset + characterRect.Left - rtb.ActualWidth / 2d);
                        rtb.ScrollToVerticalOffset(rtb.VerticalOffset + characterRect.Top - rtb.ActualHeight / 2d);
                        break;
                    case MpCopyItemType.FileList:
                        var flivm = ClipTileViewModel.FileListViewModels[ContentId - 1];
                        ClipTileViewModel.FileListBox.ScrollIntoView(flivm);
                        break;
                }
            }
        }
        
        public void ClearHighlighting() {
            Range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
        }

        public int CompareTo(MpHighlightTextRangeViewModel ohltrvm) {
            // return -1 if this instance precedes obj
            // return 0 if this instance is obj
            // return 1 if this instance is after obj
            if(ohltrvm == null) {
                return -1;
            }
            if (this == ohltrvm) {
                return 0;
            }
            if (ContentId < ohltrvm.ContentId) {
                return -1;
            }
            if (ContentId > ohltrvm.ContentId) {
                return 1;
            }
            if (!Range.Start.IsInSameDocument(ohltrvm.Range.Start)) {
                return -1;
            }
            return Range.Start.CompareTo(ohltrvm.Range.Start);
        }
        #endregion

        #region Commands
        #endregion
    }
}
