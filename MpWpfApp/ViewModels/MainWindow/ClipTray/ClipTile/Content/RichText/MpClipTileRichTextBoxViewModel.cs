using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpClipTileRichTextBoxViewModel : MpUndoableViewModelBase<MpClipTileRichTextBoxViewModel> {
        #region Private Variables

        #endregion

        #region ViewModels
        private MpClipTileViewModel _clipTileViewModel;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if(_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    OnPropertyChanged(nameof(IsInlineWithPreviousCompositeItem));
                }
            }
        }
        #endregion

        #region Properties
        public MpObservableCollection<TextRange> LastContentHighlightRangeList { get; set; } = new MpObservableCollection<TextRange>();

        #region View 
        private RichTextBox _rtb;
        public RichTextBox Rtb {
            get {
                return _rtb;
            }
            set {
                if (_rtb != value) {
                    _rtb = value;
                    OnPropertyChanged(nameof(Rtb));
                }
            }
        }
        #endregion

        #region Business Logic 
        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.SelectedRichTextBoxViewModel));
                    ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.SelectedRtb));
                    ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.IsSelected));
                }
            }
        }
        #endregion

        #region Model
        public int CompositeParentCopyItemId {
            get {
                if(CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeParentCopyItemId;
            }
            set {
                if(CopyItem != null && CopyItem.CompositeParentCopyItemId != value) {
                    CopyItem.CompositeParentCopyItemId = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.CopyItem));
                }
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
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.CopyItem));
                }
            }
        }

        public bool IsInlineWithPreviousCompositeItem {
            get {
                if (CopyItem == null) {
                    return false;
                }
                return CopyItem.IsInlineWithPreviousCompositeItem;
            }
            set {
                if (CopyItem != null && CopyItem.IsInlineWithPreviousCompositeItem != value) {
                    CopyItem.IsInlineWithPreviousCompositeItem = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(IsInlineWithPreviousCompositeItem));
                    ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.CopyItem));
                }
            }
        }

        private MpCopyItem _copyItem;
        public MpCopyItem CopyItem {
            get {
                return _copyItem;
            }
            set {
                if(_copyItem != value) {
                    _copyItem = value;
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    OnPropertyChanged(nameof(IsInlineWithPreviousCompositeItem));
                }
            }
        }
        #endregion
        #endregion

        #region Public Methods
        public MpClipTileRichTextBoxViewModel() : this(null,null) { }

        public MpClipTileRichTextBoxViewModel(MpClipTileViewModel ctvm, MpCopyItem ci) : base() {
            CopyItem = ci;
            ClipTileViewModel = ctvm;
        }

        public void ClipTileRichTextBoxListItem_Loaded(object sender, RoutedEventArgs e) {
            Rtb = (RichTextBox)sender;
            Rtb.Document = MpHelpers.Instance.ConvertRichTextToFlowDocument(CopyItem.ItemRichText);
            Rtb.CreateHyperlinks();
            Rtb.Document.PageWidth = Rtb.Width - Rtb.Padding.Left - Rtb.Padding.Right;
            Rtb.Document.PageHeight = Rtb.Height - Rtb.Padding.Top - Rtb.Padding.Bottom;

            if (ClipTileViewModel.WasAddedAtRuntime) {
                //force new items to have left alignment
                Rtb.SelectAll();
                Rtb.Selection.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                Rtb.CaretPosition = Rtb.Document.ContentStart;
            }
        }
        #endregion
    }
}
