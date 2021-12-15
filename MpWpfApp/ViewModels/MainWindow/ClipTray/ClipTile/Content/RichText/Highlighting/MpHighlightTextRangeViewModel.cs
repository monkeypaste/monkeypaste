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
using System.Web.UI.WebControls;

namespace MpWpfApp {
    public enum MpHighlightType {
        None = -5,
        Image = -3,
        Title = -2,
        App = -1,
        Text = 0
    }

    public class MpHighlightTextRangeViewModel : MpViewModelBase<MpHighlightTextRangeViewModelCollection>, IComparable<MpHighlightTextRangeViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public MpContentItemViewModel ContentItemViewModel {
            get {
                if(Parent == null || 
                   Parent.Parent == null || 
                   ContentItemIdx < 0 ||
                   ContentItemIdx >= Parent.Parent.Count) {
                    return null;
                }
                return Parent.Parent.ItemViewModels[ContentItemIdx];
            }
        }

        #endregion

        #region State

        public MpHighlightType HighlightType { get; set; } = MpHighlightType.None;

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

        #endregion

        #region Appearance

        public Brush HighlightBrush => IsSelected ? Brushes.Pink : Brushes.Yellow;

        #endregion

        #region Model

        public int ContentItemIdx { get; set; } = -1;
        
        public int SortOrderIdx { get; set; } = -5;

        public TextRange Range { get; set; }

        #endregion

        #endregion

        #region Public Methods
        public MpHighlightTextRangeViewModel() : base(null) { }

        public MpHighlightTextRangeViewModel(MpHighlightTextRangeViewModelCollection parent) : base(parent) {
            PropertyChanged += MpHighlightTextRangeViewModel_PropertyChanged;            
        }

        public async Task InitializeAsync(int contentItemIdx, TextRange tr, MpHighlightType ht, int sortOrderIdx) {
            await Task.Delay(1);

            ContentItemIdx = contentItemIdx;
            Range = tr;
            HighlightType = ht;
            SortOrderIdx = sortOrderIdx;
            OnPropertyChanged(nameof(ContentItemViewModel));
        }

        private void MpHighlightTextRangeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected && HighlightType == MpHighlightType.Text) {
                        Rect characterRect = Rect.Empty;
                        var iuic = Range.End.Parent.FindParentOfType<InlineUIContainer>();
                        if (iuic != null) {
                            var rhl = iuic.Parent.FindParentOfType<Hyperlink>();
                            if (rhl != null) {
                                characterRect = rhl.ContentEnd.GetCharacterRect(LogicalDirection.Backward);
                            }
                        }
                        if (characterRect == Rect.Empty) {
                            characterRect = Range.End.GetCharacterRect(LogicalDirection.Forward);
                        }
                        Parent.Parent.RequestScrollIntoView(characterRect);
                        //switch (Parent.Parent.CopyItemType) {
                        //    case MpCopyItemType.RichText:
                        //        //var rtb = ContentItemViewModel.Rtb;

                        //        //rtb.ScrollToHorizontalOffset(rtb.HorizontalOffset + characterRect.Left - rtb.ActualWidth / 2d);
                        //        //rtb.ScrollToVerticalOffset(rtb.VerticalOffset + characterRect.Top - rtb.ActualHeight / 2d);
                        //        break;
                        //    case MpCopyItemType.FileList:
                        //        //var flivm = Parent.Parent.FileListCollectionViewModel.FileItems[SortOrderIdx];
                        //        //Parent.Parent.FileListBox.ScrollIntoView(flivm);
                        //        //Parent.Parent.ContentContainerViewModel.RequestScrollIntoView(flivm);
                        //        break;
                        //}
                    }
                    //if(IsSelected && IsTitleRange && ctvm is MpRtbViewModel) {
                    //    ((MpRtbViewModel)ctvm).IsSelected = false;
                    //}
                    break;
            }
        }

        public void HighlightRange() {
           // Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.CopyItemAppIconHighlightBorder));
            foreach (var rtbvm in Parent.Parent.ItemViewModels) {
                //rtbvm.OnPropertyChanged(nameof(rtbvm.SubItemOverlayVisibility));
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
