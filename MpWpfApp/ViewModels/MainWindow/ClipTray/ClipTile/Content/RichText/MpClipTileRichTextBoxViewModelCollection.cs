using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpClipTileRichTextBoxViewModelCollection : MpObservableCollectionViewModel<MpClipTileRichTextBoxViewModel> {
        #region Private Variables

        #endregion        

        #region Properties
        #region ViewModels
        private MpClipTileViewModel _clipTileViewModel;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                    OnPropertyChanged(nameof(SelectedClipTileRichTextBoxViewModel));
                    OnPropertyChanged(nameof(SelectedRtb));
                }
            }
        }

        public MpClipTileRichTextBoxViewModel SelectedClipTileRichTextBoxViewModel {
            get {
                if(ClipTileViewModel == null || this.Count == 0) {
                    return null;
                }
                var svm = this.Where(x => x.IsSelected).FirstOrDefault();
                if(svm == null && this.Count > 0) {
                    this[0].IsSelected = true;
                    svm = this[0];
                }
                return svm;
            }
            set {
                if (ClipTileViewModel != null && 
                    SelectedClipTileRichTextBoxViewModel != value && 
                    this.Contains(value)) {
                    ClearSelection();
                    this[this.IndexOf(value)].IsSelected = true;
                    OnPropertyChanged(nameof(SelectedClipTileRichTextBoxViewModel));
                    OnPropertyChanged(nameof(SelectedRtb));
                }
            }
        }

        public RichTextBox SelectedRtb {
            get {
                if(SelectedClipTileRichTextBoxViewModel == null) {
                    return null;
                }
                return SelectedClipTileRichTextBoxViewModel.Rtb;
            }
        }

        public MpEventEnabledFlowDocument FullDocument {
            get {
                var fullDocument = new MpEventEnabledFlowDocument();

                foreach(var rtbvm in this) {
                    MpHelpers.Instance.CombineFlowDocuments((MpEventEnabledFlowDocument)rtbvm.Rtb.Document, fullDocument, !rtbvm.CopyItem.IsInlineWithPreviousCompositeItem);
                }
                return fullDocument;
            }
        }
        #endregion
        #endregion

        #region Public Methods
        public MpClipTileRichTextBoxViewModelCollection() { }

        public MpClipTileRichTextBoxViewModelCollection(MpClipTileViewModel ctvm) : base() {
            ClipTileViewModel = ctvm;
        }

        public void ClearSelection() {
            foreach(var rtbvm in this) {
                rtbvm.IsSelected = false;
            }
        }

        public void ResetSelection() {
            ClearSelection();
            if(this.Count > 0) {
                this[0].IsSelected = true;
            }
        }
        public void ClearAllHyperlinks() {
            foreach(var rtbvm in this) {
                rtbvm.ClearHyperlinks();
            }
        }

        public void CreateAllHyperlinks() {
            foreach (var rtbvm in this) {
                rtbvm.CreateHyperlinks();
            }
        }
        #endregion
    }
}
