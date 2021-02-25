using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpClipTileRichTextBoxViewModelCollection : MpObservableCollectionViewModel<MpClipTileRichTextBoxViewModel>, ICloneable {
        #region Private Variables
        private Point _lastMousePosition = new Point();
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

        public double TotalItemHeight {
            get {
                double totalHeight = 0;
                foreach(var rtbvm in this) {
                    totalHeight += rtbvm.RtbListBoxItemHeight;
                }
                return totalHeight;
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
            var richTextListBox = (ListBox)sender;
            richTextListBox.PreviewMouseMove += (s, e) => {
                _lastMousePosition = e.GetPosition(richTextListBox);
            };
            richTextListBox.Drop += ClipTileRichTextBoxViewModel_Drop;
        }

        #region Drag & Drop

        public void ClipTileRichTextBoxViewModel_PreviewMouseMove(object sender, MouseEventArgs e) {
            var draggedItem = ((FrameworkElement)sender).GetVisualAncestor<ListBoxItem>();
            if(draggedItem.DataContext is MpClipTileViewModel) {
                return;
            }
            if (draggedItem != null && e.LeftButton == MouseButtonState.Pressed) {
                //DataObject data = new DataObject();
                //data.SetData(DataFormats.Rtf, ClipTileRichTextBoxViewModel.CopyItemRichText);
                //data.SetData(DataFormats.StringFormat, ClipTileRichTextBoxViewModel.CopyItemPlainText);
                //data.SetData(DataFormats.FileDrop,new StringCollection() { ClipTileRichTextBoxViewModel.CopyItemPlainText });
                //data.SetData(Properties.Settings.Default.CompositeItemDragDropFormatName, ClipTileRichTextBoxViewModel);

                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        public void ClipTileRichTextBoxViewModel_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
            if (e.Effects.HasFlag(DragDropEffects.Move)) {
                Mouse.SetCursor(Cursors.Cross);
            } else {
                Mouse.SetCursor(Cursors.No);
            }
            e.Handled = true;
        }

        public void ClipTileRichTextBoxViewModel_Drop(object sender, DragEventArgs e) {
            var dropVm = (MpClipTileRichTextBoxViewModel)e.Data.GetData(typeof(MpClipTileRichTextBoxViewModel));
            if(dropVm == null) {
                return;
            }
            var dropVmc = dropVm.RichTextBoxViewModelCollection;

            MpClipTileRichTextBoxViewModel targetVm = null;
            MpClipTileRichTextBoxViewModelCollection targetVmc = null;
            if(((FrameworkElement)sender).DataContext.GetType() == typeof(MpClipTileRichTextBoxViewModel)) {
                targetVm = (MpClipTileRichTextBoxViewModel)((FrameworkElement)sender).DataContext;
                targetVmc = targetVm.RichTextBoxViewModelCollection;
            } else if (((FrameworkElement)sender).DataContext.GetType() == typeof(MpRichTextBoxPathOverlayViewModel)) {
                targetVm = (MpClipTileRichTextBoxViewModel)((MpRichTextBoxPathOverlayViewModel)((FrameworkElement)sender).DataContext).ClipTileRichTextBoxViewModel;
                targetVmc = targetVm.RichTextBoxViewModelCollection;
            } else if (((FrameworkElement)sender).DataContext.GetType() == typeof(MpClipTileViewModel)) {
                targetVmc = ((MpClipTileViewModel)((FrameworkElement)sender).DataContext).RichTextBoxViewModelCollection;
            }

            if (dropVm == targetVm) {
                return;
            }
            dropVmc.Remove(dropVm);

            if (targetVm != null && targetVmc != null) {
                targetVmc.Insert(targetVmc.IndexOf(targetVm), dropVm);
            } else if(targetVmc != null) {
                //target is the rtb listbox
                var rtblb = targetVmc.ClipTileViewModel.RichTextBoxListBox;                
                var mpos = MpHelpers.Instance.GetMousePosition(rtblb);
                if (mpos.Y < 0) {
                    this.Insert(0, dropVm);
                } else {
                    this.Add(dropVm);
                }
            }
            if (dropVmc != targetVmc) {
                if(dropVmc.Count == 0) {
                    MainWindowViewModel.ClipTrayViewModel.Remove(dropVmc.ClipTileViewModel);
                }
                dropVm.ClipTileViewModel = ClipTileViewModel;
                dropVm.CompositeParentCopyItemId = ClipTileViewModel.CopyItemId;
                ClipTileViewModel.CopyItem.CompositeItemList.Add(dropVm.CopyItem);
            }

            dropVm.CompositeSortOrderIdx = targetVmc.IndexOf(dropVm);
            ClipTileViewModel.CopyItem.WriteToDatabase();
            ClipTileViewModel.RichTextBoxListBox.Items.Refresh();
        }

        public MpClipTileRichTextBoxViewModel GetItemFromMouseLocation(Point p) {
            var result = VisualTreeHelper.HitTest(ClipTileViewModel.RichTextBoxListBox, p);
            if(result != null && result.VisualHit != null) {
                if(result.VisualHit.GetType() == typeof(MpClipTileRichTextBoxViewModel)) {
                    return (MpClipTileRichTextBoxViewModel)result.VisualHit;
                }
                for (int i = 0; i < this.Count; i++) {
                    double top = i > 0 ? this[i - 1].RtbListBoxItemHeight : 0;
                    double bottom = this[i].RtbListBoxItemHeight;
                    if(p.Y >= top && p.Y <= bottom) {
                        return this[i];
                    }
                }
                if(this.Count > 0) {
                    if (p.Y <= 0) {
                        return this[0];
                    }
                    return this[this.Count - 1];
                }
            }
            return null;
        }
        #endregion

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

        #region Private Methods

        #endregion
    }
}
