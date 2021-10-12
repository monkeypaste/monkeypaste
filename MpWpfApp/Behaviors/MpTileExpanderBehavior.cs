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
        private double deltaWidth, deltaHeight, deltaContentHeight;


        protected override void OnAttached() {
            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
        }

        private void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(AssociatedObject.DataContext != null && AssociatedObject.DataContext is MpClipTileViewModel ctvm) {
                ctvm.OnExpandRequest += Ctvm_OnExpandRequest;
                ctvm.OnUnExpandRequest += Ctvm_OnUnExpandRequest;
            }
        }

        private void Ctvm_OnUnExpandRequest(object sender, EventArgs e) {
            MpHelpers.Instance.RunOnMainThread(Unexpand);
        }

        private void Ctvm_OnExpandRequest(object sender, EventArgs e) {
            MpHelpers.Instance.RunOnMainThread(Expand);
        }

        private void Expand() {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            var mwvm = ctvm.MainWindowViewModel;


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
            maxDeltaHeight = Math.Min(maxDeltaHeight, SystemParameters.PrimaryScreenHeight / 2);

            //calculate the diff of the tiles total content height and its current height without letting it get smaller
            deltaContentHeight = Math.Max(0,ctvm.TotalExpandedSize.Height - ctvm.ContainerSize.Height);

            //adjust the difference for the toolbars shown after expansion
            if (ctvm.IsAnyPastingTemplate) {
                deltaContentHeight += MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
            }
            deltaContentHeight += MpMeasurements.Instance.ClipTileEditToolbarHeight;

            //make change in height so window doesn't get smaller but also doesn't extend past top of screen
            deltaHeight = Math.Max(MpMeasurements.Instance.MainWindowMinHeight, Math.Min(maxDeltaHeight, deltaContentHeight));
            deltaWidth =  mwvm.ClipTrayWidth - ctvm.TileBorderWidth - MpMeasurements.Instance.ClipTileExpandedMargin;

            mwvm.MainWindowTop -= deltaHeight;

            mwvm.ClipTrayHeight += deltaHeight;

            ctvm.TileBorderWidth += deltaWidth;
            ctvm.TileBorderHeight += deltaHeight;

            ctvm.TileContentWidth += deltaWidth;
            ctvm.TileContentHeight += deltaContentHeight;

            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            clv.EditToolbarView.Visibility = Visibility.Visible;
            clv.UpdateLayout();

            var civl = AssociatedObject.GetVisualDescendents<MpContentItemView>().ToList();
            foreach(var civ in civl) {
                var civm = civ.DataContext as MpContentItemViewModel;
                civm.OnPropertyChanged(nameof(civm.EditorHeight));
                civm.OnPropertyChanged(nameof(civm.EditorCursor));
                //civ.EditorView.Rtb.Width = ctvm.TileContentWidth;
                civ.EditorView.Rtb.FitDocToRtb();
            }

            var sv = clv.ContentListBox.GetScrollViewer();
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.InvalidateScrollInfo();

            clv.UpdateAdorner();

            AssociatedObject.UpdateLayout();

            //ctvm.OnPropertyChanged(nameof(ctvm.IsExpanded));

            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel += ApplicationHook_MouseWheel;
        }        

        public void Unexpand() {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            var mwvm = ctvm.MainWindowViewModel;

            //trigger app mode column to hide
            ctvm.OnPropertyChanged(nameof(ctvm.FlipButtonVisibility));
            ctvm.Parent.OnPropertyChanged(nameof(ctvm.Parent.IsAnyTileExpanded));
            mwvm.OnPropertyChanged(nameof(mwvm.AppModeButtonGridWidth));

            ctvm.Parent.Items
                .Where(x => x != ctvm)
                .ForEach(y => y.ItemVisibility = Visibility.Visible);

            mwvm.MainWindowTop += deltaHeight;
            
            mwvm.ClipTrayHeight -= deltaHeight;

            ctvm.TileBorderWidth -= deltaWidth;
            ctvm.TileBorderHeight -= deltaHeight;

            ctvm.TileContentWidth -= deltaWidth;
            ctvm.TileContentHeight -= deltaContentHeight;

            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
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
                //civ.EditorView.Rtb.Width = ctvm.TileContentWidth;
                civ.EditorView.Rtb.FitDocToRtb();
            }

            clv.UpdateAdorner();


            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel -= ApplicationHook_MouseWheel;
        }

        private void ApplicationHook_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            clv.Civm_OnScrollWheelRequest(this, -e.Delta);
        }
    }
}
