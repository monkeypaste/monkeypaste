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
using MonkeyPaste;

namespace MpWpfApp {
    public enum MpHighlightType {
        None = -5,
        Image = -3,
        Title = -2,
        App = -1,
        Text = 0
    }

    public class MpHighlightTextRangeViewModel : MpUndoableViewModelBase<MpHighlightTextRangeViewModel>, IComparable<MpHighlightTextRangeViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        private MpClipTileViewModel _hostClipTileViewModel;
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                return _hostClipTileViewModel;
            }
            set {
                if (_hostClipTileViewModel != value) {
                    _hostClipTileViewModel = value;
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                }
            }
        }

        private MpContentItemViewModel _rtbItemViewModel;
        public MpContentItemViewModel ContentItemViewModel {
            get {
                return _rtbItemViewModel;
            }
            set {
                if (_rtbItemViewModel != value) {
                    _rtbItemViewModel = value;
                    OnPropertyChanged(nameof(ContentItemViewModel));
                }
            }
        }
        #endregion

        private MpHighlightType _highlightType = MpHighlightType.None;
        public MpHighlightType HighlightType {
            get {
                return _highlightType;
            }
            set {
                if(_highlightType != value) {
                    _highlightType = value;
                    OnPropertyChanged(nameof(HighlightType));
                }
            }
        }

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

        private int _sortOrderIdx = -5;
        public int SortOrderIdx {
            get {
                return _sortOrderIdx;
            }
            set {
                if (_sortOrderIdx != value) {
                    _sortOrderIdx = value;
                    OnPropertyChanged(nameof(SortOrderIdx));
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
        #endregion

        


        #region Public Methods
        public MpHighlightTextRangeViewModel() : this(null,null,null,-1,MpHighlightType.None) { }

        public MpHighlightTextRangeViewModel(MpClipTileViewModel ctvm, MpContentItemViewModel rtbvm, TextRange tr, int contentId, MpHighlightType ht) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(IsSelected):
                        if (IsSelected && HighlightType == MpHighlightType.Text) {
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
                            switch (HostClipTileViewModel.CopyItemType) {
                                case MpCopyItemType.RichText:
                                    //var rtb = ContentItemViewModel.Rtb;
                                    HostClipTileViewModel.ContentContainerViewModel.RequestScrollIntoView(characterRect);
                                    //rtb.ScrollToHorizontalOffset(rtb.HorizontalOffset + characterRect.Left - rtb.ActualWidth / 2d);
                                    //rtb.ScrollToVerticalOffset(rtb.VerticalOffset + characterRect.Top - rtb.ActualHeight / 2d);
                                    break;
                                case MpCopyItemType.FileList:
                                    var flivm = HostClipTileViewModel.FileListCollectionViewModel[SortOrderIdx];
                                    HostClipTileViewModel.FileListBox.ScrollIntoView(flivm);
                                    break;
                            }
                        }
                        //if(IsSelected && IsTitleRange && ctvm is MpRtbViewModel) {
                        //    ((MpRtbViewModel)ctvm).IsSubSelected = false;
                        //}
                        break;
                }
            };
            HostClipTileViewModel = ctvm;
            ContentItemViewModel = rtbvm;
            Range = tr;
            SortOrderIdx = contentId;
            HighlightType = ht;
        }

        public void HighlightRange() {
           // HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItemAppIconHighlightBorder));
            foreach (MpRtbItemViewModel rtbvm in HostClipTileViewModel.ContentContainerViewModel.ItemViewModels) {
                rtbvm.OnPropertyChanged(nameof(rtbvm.SubItemOverlayVisibility));
                //rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemAppIconHighlightBorder));
            }
            if (HighlightType == MpHighlightType.App || HighlightType == MpHighlightType.Image) {
                
            } else {
                Range.ApplyPropertyValue(TextElement.BackgroundProperty, HighlightBrush);
            }                       
        }
        
        public void ClearHighlighting() {
            if (HighlightType == MpHighlightType.App || HighlightType == MpHighlightType.Image) {
                
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

            if(SortOrderIdx < ohltrvm.SortOrderIdx) {
                return -1;
            }
            if(SortOrderIdx > ohltrvm.SortOrderIdx) {
                return 1;
            }
            return 0;
        }
        #endregion

        #region Commands
        #endregion
    }
}
