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
    public enum MpHighlightType {
        None = -5,
        Title = -2,
        App = -1,
        Text = 0
    }

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
                //if(_isSelected != value)
                    {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(HighlightBrush));
                }
            }
        }

        private int _contentId = -5;
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
                return ContentId == (int)MpHighlightType.Title;
            }
        }

        public bool IsAppRange {
            get {
                return ContentId == (int)MpHighlightType.App;
            }
        }
        #endregion

        #region Public Methods
        public MpHighlightTextRangeViewModel() : this(null,null,-1) { }

        public MpHighlightTextRangeViewModel(MpClipTileViewModel ctvm, TextRange tr, int contentId) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(IsSelected):
                        if (IsSelected && !IsTitleRange && !IsAppRange) {
                            Rect characterRect = Rect.Empty;
                            var iuic = Range.End.Parent.FindParentOfType<InlineUIContainer>();
                            if(iuic != null) {
                                var rhl = iuic.Parent.FindParentOfType<Hyperlink>();
                                if(rhl != null) {
                                    characterRect = rhl.ContentEnd.GetCharacterRect(LogicalDirection.Backward);
                                }
                            }
                            if(characterRect == Rect.Empty) {
                                characterRect = Range.End.GetCharacterRect(LogicalDirection.Forward);
                            }
                            switch (ClipTileViewModel.CopyItemType) {
                                case MpCopyItemType.Composite:
                                case MpCopyItemType.RichText:
                                    var rtb = ClipTileViewModel.RichTextBoxViewModelCollection[ContentId].Rtb;
                                    ClipTileViewModel.RichTextBoxViewModelCollection.RichTextBoxListBox.ScrollIntoView(rtb);
                                    rtb.ScrollToHorizontalOffset(rtb.HorizontalOffset + characterRect.Left - rtb.ActualWidth / 2d);
                                    rtb.ScrollToVerticalOffset(rtb.VerticalOffset + characterRect.Top - rtb.ActualHeight / 2d);
                                    break;
                                case MpCopyItemType.FileList:
                                    var flivm = ClipTileViewModel.FileListCollectionViewModel[ContentId];
                                    ClipTileViewModel.FileListBox.ScrollIntoView(flivm);
                                    break;
                            }
                        }
                        if(IsSelected && IsTitleRange && ctvm is MpRtbListBoxItemRichTextBoxViewModel) {
                            ((MpRtbListBoxItemRichTextBoxViewModel)ctvm).IsSubSelected = false;
                        }
                        break;
                }
            };
            ClipTileViewModel = ctvm;
            Range = tr;
            ContentId = contentId;
        }

        public void HighlightRange() {
            if(IsAppRange) {
                ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.CopyItemAppIconHighlightBorder));
            } else {
                Range.ApplyPropertyValue(TextElement.BackgroundProperty, HighlightBrush);
            }                       
        }
        
        public void ClearHighlighting() {
            if (IsAppRange) {
                
            } else {
                Range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
            }
            
        }

        public int CompareTo(MpHighlightTextRangeViewModel ohltrvm) {
            // return -1 if this instance precedes obj
            // return 0 if this instance is obj
            // return 1 if this instance is after obj
            if(ohltrvm == null) {
                return -1;
            }
            if(Range == null) {
                if(ContentId > ohltrvm.ContentId) {
                    return 1;
                }
                return -1;
            }
            if (ohltrvm.Range == null) {
                if (ContentId < ohltrvm.ContentId) {
                    return -1;
                }
                return 1;
            }
            //if (Range.Start == ohltrvm.Range.Start && Range.End == ohltrvm.Range.End) {
            //    return 0;
            //}
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
            //return ohltrvm.Range.Start.CompareTo(Range.Start);
        }
        #endregion

        #region Commands
        #endregion
    }
}
