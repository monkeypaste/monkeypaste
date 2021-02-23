using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpClipTileRichTextBoxViewModelCollection : MpObservableCollectionViewModel<MpClipTileRichTextBoxViewModel>, ICloneable {
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
                foreach(var rtbvm in this) {
                    if(rtbvm.IsSelected) {
                        return rtbvm;
                    }
                }
                return null;
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
                var fullDocument = MpHelpers.Instance.ConvertRichTextToFlowDocument(MpHelpers.Instance.ConvertPlainTextToRichText(string.Empty));

                foreach(var rtbvm in this) {
                    MpHelpers.Instance.CombineFlowDocuments((MpEventEnabledFlowDocument)rtbvm.Rtb.Document, fullDocument, !rtbvm.CopyItem.IsInlineWithPreviousCompositeItem);
                }
                return fullDocument;
            }
        }
        #endregion

        #region Layout
        public double RelativeWidthMax {
            get {
                double maxWidth = 0;
                foreach(var rtbvm in this) {
                    maxWidth = Math.Max(maxWidth, rtbvm.RtbRelativeWidthMax);
                }
                return maxWidth;
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileRichTextBoxViewModelCollection() { }

        public MpClipTileRichTextBoxViewModelCollection(MpClipTileViewModel ctvm) : base() {            
            ClipTileViewModel = ctvm;
        }

        public void ClipTileRichTextBoxViewModelCollection_Loaded(object sender, RoutedEventArgs args) {
            var rtblb = (ListBox)sender;
            
        }

        public new void Add(MpClipTileRichTextBoxViewModel rtbvm) {
            base.Add(rtbvm);
        }
        public void AnimateItems(double fromWidth,double toWidth, double fromHeight, double toHeight,double fromTop, double toTop,double fromBottom, double toBottom) {
            if(toWidth > 0) {
                foreach (var rtbvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            fromWidth,
                            toWidth,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            new List<FrameworkElement> { rtbvm.Rtb },
                            FrameworkElement.WidthProperty,
                            (s1, e44) => {
                                rtbvm.UpdateLayout();
                            });
                }
            }
            if (toHeight > 0) {

            }
            if (toTop > 0) {
                foreach(var rtbvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            fromTop,
                            toTop,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            new List<FrameworkElement> { rtbvm.Rtb },
                            Canvas.TopProperty,
                            (s1, e44) => {

                            });
                    fromTop += rtbvm.RtbListBoxItemHeight;
                    toTop += rtbvm.RtbListBoxItemHeight;
                }
            }
            if (toBottom > 0) {

            }
        }

        public void SelectRichTextBoxViewModel(int idx) {
            if(idx < 0 || idx >= this.Count) {
                return;
            }
            for (int i = 0; i < this.Count; i++) {
                this[i].SetSelection(i == idx);
            }
        }

        public void SelectRichTextBoxViewModel(MpClipTileRichTextBoxViewModel rtbvm) {
            if(!this.Contains(rtbvm)) {
                return;
            }
            SelectRichTextBoxViewModel(this.IndexOf(rtbvm));
        }
        public void ClearSelection() {
            foreach(var rtbvm in this) {
                rtbvm.SetSelection(false);
            }
        }

        public void ResetSelection() {
            ClearSelection();
            if(this.Count > 0) {
                this[0].SetSelection(true);
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

        public object Clone() {
            var nrtbvmc = new MpClipTileRichTextBoxViewModelCollection(ClipTileViewModel);
            foreach(var rtbvm in this) {
                nrtbvmc.Add((MpClipTileRichTextBoxViewModel)rtbvm.Clone());
            }
            return nrtbvmc;
        }
        #endregion
    }
}
