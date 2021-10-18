using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTileExpanderBehavior : Behavior<MpClipTileContainerView> {
        private object _dc;

        private  Point _deltaSize = new Point();

        protected override void OnAttached() {
            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
        }
        
        private void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
           if(e.OldValue != null && e.OldValue is MpClipTileViewModel octvm) {
                octvm.OnExpandRequest -= Ctvm_OnExpandRequest;
                octvm.OnUnExpandRequest -= Ctvm_OnUnExpandRequest;
                MpMainWindowViewModel.Instance.OnMainWindowHide -= MainWindowViewModel_OnMainWindowHide;
            }
           if(e.NewValue != null && e.NewValue is MpClipTileViewModel nctvm) {
                nctvm.OnExpandRequest += Ctvm_OnExpandRequest;
                nctvm.OnUnExpandRequest += Ctvm_OnUnExpandRequest;
                MpMainWindowViewModel.Instance.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
            }
            _dc = e.NewValue;
        }

        public void Resize(double newContentHeight) {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            if(!ctvm.IsExpanded) {
                return;
            }
            var mwvm = MpMainWindowViewModel.Instance;
            newContentHeight = Math.Max(newContentHeight,MpMeasurements.Instance.ClipTileContentHeight);
            double nchd = newContentHeight - ctvm.TileContentHeight;

            mwvm.MainWindowTop -= nchd;
            mwvm.ClipTrayHeight += nchd;
            ctvm.TileBorderHeight += nchd;
            ctvm.TileContentHeight += nchd;

            _deltaSize.Y += nchd;

            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            clv.UpdateLayout();

            var civl = AssociatedObject.GetVisualDescendents<MpContentItemView>().ToList();
            foreach (var civ in civl) {
                civ.UpdateLayout();
            }
            AssociatedObject.UpdateLayout();
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            ctvm.IsExpanded = false;
        }

        private void Ctvm_OnUnExpandRequest(object sender, EventArgs e) {
            MpHelpers.Instance.RunOnMainThread(Unexpand);
        }

        private void Ctvm_OnExpandRequest(object sender, EventArgs e) {
            MpHelpers.Instance.RunOnMainThread(Expand);
        }

        private void Expand() {
            _deltaSize = new Point();

            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            var mwvm = MpMainWindowViewModel.Instance;


            //trigger app mode column to hide
            ctvm.OnPropertyChanged(nameof(ctvm.FlipButtonVisibility));
            ctvm.Parent.OnPropertyChanged(nameof(ctvm.Parent.IsAnyTileExpanded));
            mwvm.OnPropertyChanged(nameof(mwvm.AppModeButtonGridWidth));

            //collapse all other tiles
            ctvm.Parent.Items
                .Where(x => x != ctvm)
                .ForEach(y => y.ItemVisibility = Visibility.Collapsed);

            ctvm.IsSelected = true;

            //find max change in y so main window doesn't go past top of screen
            double maxDeltaHeight = MpMeasurements.Instance.MainWindowMaxHeight - MpMeasurements.Instance.MainWindowMinHeight;
            maxDeltaHeight = Math.Min(maxDeltaHeight, SystemParameters.PrimaryScreenHeight);

            //calculate the diff of the tiles total content height and its current height without letting it get smaller
            double deltaContentHeight = Math.Max(0,ctvm.TotalExpandedSize.Height - ctvm.ContainerSize.Height);
            //adjust the difference for the toolbars shown after expansion
            if (ctvm.IsAnyPastingTemplate) {
                deltaContentHeight += MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
            }
            deltaContentHeight += MpMeasurements.Instance.ClipTileEditToolbarHeight;

            //make change in height so window doesn't get smaller but also doesn't extend past top of screen
            //_deltaSize.Height = Math.Min(maxDeltaHeight, deltaContentHeight);
            //sanity check so heights are the same after all that
            _deltaSize.Y = Math.Max(MpMeasurements.Instance.MainWindowMinHeight, Math.Min(maxDeltaHeight, deltaContentHeight));
            _deltaSize.X =  mwvm.ClipTrayWidth - ctvm.TileBorderWidth - MpMeasurements.Instance.ClipTileExpandedMargin;

            mwvm.MainWindowTop -= _deltaSize.Y;

            mwvm.ClipTrayHeight += _deltaSize.Y;

            ctvm.TileBorderWidth += _deltaSize.X;
            ctvm.TileBorderHeight += _deltaSize.Y;

            ctvm.TileContentWidth += _deltaSize.X;
            ctvm.TileContentHeight += _deltaSize.Y;



            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            if(clv != null) {
                clv.EditToolbarView.Visibility = Visibility.Visible;
                clv.UpdateLayout();
            }

            var civl = AssociatedObject.GetVisualDescendents<MpContentItemView>().ToList();
            foreach(var civ in civl) {
                var civm = civ.DataContext as MpContentItemViewModel;
                civm.OnPropertyChanged(nameof(civm.EditorHeight));
                civm.OnPropertyChanged(nameof(civm.EditorCursor));
                civm.OnPropertyChanged(nameof(civm.IsEditingContent));
                //civ.EditorView.Rtb.Width = ctvm.TileContentWidth;
                civ.EditorView.Rtb.FitDocToRtb();
                if(civm.IsSelected) {
                    civ.EditorView.Rtb.Focus();
                    civ.EditorView.Rtb.CaretPosition = civ.EditorView.Rtb.Document.ContentStart;

                    civ.EditorView.SizeContainerToContent();
                }
            }

            if(clv != null) {
                var sv = clv.ContentListBox.GetScrollViewer();
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                sv.InvalidateScrollInfo();

                clv.UpdateAdorner();
            }
            //clv.ContentListBox.Items.Refresh();

            AssociatedObject.UpdateLayout();

            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel += ApplicationHook_MouseWheel;
        }        

        public void Unexpand() {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            var mwvm = MpMainWindowViewModel.Instance;

            //trigger app mode column to hide
            ctvm.OnPropertyChanged(nameof(ctvm.FlipButtonVisibility));
            ctvm.Parent.OnPropertyChanged(nameof(ctvm.Parent.IsAnyTileExpanded));
            mwvm.OnPropertyChanged(nameof(mwvm.AppModeButtonGridWidth));

            ctvm.Parent.Items
                .Where(x => x != ctvm && !x.IsPlaceholder)
                .ForEach(y => y.ItemVisibility = Visibility.Visible);

            mwvm.MainWindowTop += _deltaSize.Y;
            
            mwvm.ClipTrayHeight -= _deltaSize.Y;

            ctvm.TileBorderWidth -= _deltaSize.X;
            ctvm.TileBorderHeight -= _deltaSize.Y;

            ctvm.TileContentWidth -= _deltaSize.X;
            ctvm.TileContentHeight -= _deltaSize.Y;

            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            if(clv != null) {
                clv.EditToolbarView.Visibility = Visibility.Collapsed;
                clv.UpdateLayout();

                var sv = clv.ContentListBox.GetScrollViewer();
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

                var civl = AssociatedObject.GetVisualDescendents<MpContentItemView>().ToList();
                foreach (var civ in civl) {
                    var civm = civ.DataContext as MpContentItemViewModel;
                    civm.ClearEditing();
                    civm.OnPropertyChanged(nameof(civm.EditorHeight));
                    civm.OnPropertyChanged(nameof(civm.EditorCursor));
                    civm.OnPropertyChanged(nameof(civm.IsEditingContent));
                    civ.EditorView.Rtb.FitDocToRtb();
                }

                MpHelpers.Instance.RunOnMainThread(async () => {
                    await Task.WhenAll(civl.Select(x => x.EditorView.SyncModelsAsync()).ToArray());
                });

                clv.UpdateAdorner();
            }

            _deltaSize = new Point();
            
            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel -= ApplicationHook_MouseWheel;
        }

        private void ApplicationHook_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            clv.Civm_OnScrollWheelRequest(this, -e.Delta);
        }
    }
}
