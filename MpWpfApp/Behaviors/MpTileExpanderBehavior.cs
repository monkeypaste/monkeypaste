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
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTileExpanderBehavior : Behavior<MpClipTileContainerView> {
        private double _originalMainWindowTop = 0;
        private double _initialExpandedMainWindowTop = 0;

        public bool IsExpandingOrUnexpanding { get; set; } = false;

        protected override void OnAttached() {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e) {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            MpMainWindowViewModel.Instance.OnMainWindowHide -= MainWindowViewModel_OnMainWindowHide;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            MpMessenger.Instance.Register<MpMessageType>(AssociatedObject.DataContext, ReceiveClipTileMessage, AssociatedObject.DataContext);

            MpMainWindowViewModel.Instance.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
        }

        private void ReceiveClipTileMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.Expand:
                    Expand();
                    break;
                case MpMessageType.Unexpand:
                    Unexpand();
                    break;
            }
        }

        public void Resize(double deltaHeight) {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            if(!ctvm.IsExpanded) {
                return;
            }
            
            var mwvm = MpMainWindowViewModel.Instance;

            mwvm.MainWindowTop -= deltaHeight;
            mwvm.ClipTrayHeight += deltaHeight;
            ctvm.TileBorderHeight += deltaHeight;
            ctvm.TileContentHeight += deltaHeight;

            double boundAdjust = 0;
            if(mwvm.MainWindowTop < MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding) {
                boundAdjust = mwvm.MainWindowTop - MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding;
            } else if (mwvm.MainWindowTop > mwvm.MainWindowHeight - MpMeasurements.Instance.MainWindowMinHeight) {
                boundAdjust = mwvm.MainWindowTop - (mwvm.MainWindowHeight - MpMeasurements.Instance.MainWindowMinHeight);
            }

            mwvm.MainWindowTop -= boundAdjust;
            mwvm.ClipTrayHeight += boundAdjust;
            ctvm.TileBorderHeight += boundAdjust;
            ctvm.TileContentHeight += boundAdjust;

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
            if(ctvm.IsAnyPastingTemplate) {
                return;
            }
            ctvm.IsExpanded = false;
        }

        private void Ctvm_OnUnExpandRequest(object sender, EventArgs e) {
            MpHelpers.Instance.RunOnMainThread(Unexpand);
        }

        private void Ctvm_OnExpandRequest(object sender, EventArgs e) {
            MpHelpers.Instance.RunOnMainThread(Expand);
        }

        private void Expand() {
            //need to do this so listboxitem matches w/ datacontext or it will expand to another tiles size
            IsExpandingOrUnexpanding = true;

            var _deltaSize = new Point();

            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            var mwvm = MpMainWindowViewModel.Instance;

            _originalMainWindowTop = mwvm.MainWindowTop;

            if(ctvm.ItemViewModels.Count > 1 && ctvm.SelectedItems.Count == ctvm.ItemViewModels.Count) {
                ctvm.ResetSubSelection(false);
            }

            //trigger app mode column to hide
            ctvm.OnPropertyChanged(nameof(ctvm.FlipButtonVisibility));
            ctvm.Parent.OnPropertyChanged(nameof(ctvm.Parent.IsAnyTileExpanded));
            mwvm.OnPropertyChanged(nameof(mwvm.AppModeButtonGridWidth));

            ctvm.IsSelected = true;

            //find max change in y so main window doesn't go past top of screen
            double maxDeltaHeight = SystemParameters.PrimaryScreenHeight - MpMeasurements.Instance.MainWindowDefaultHeight;
            //add padding so user can click away from window
            maxDeltaHeight -= MpMeasurements.Instance.ClipTileExpandedMaxHeightPadding;

            //only make tile larger otherwise leave standard height
            if(ctvm.TotalExpandedContentSize.Height > ctvm.TileContentHeight) {
                double actualHeightDiff = ctvm.TotalExpandedContentSize.Height - ctvm.TileContentHeight;

                //adjust the difference for the toolbars shown after expansion
                if (ctvm.IsAnyPastingTemplate) {
                    _deltaSize.Y += MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                }
                _deltaSize.Y += MpMeasurements.Instance.ClipTileEditToolbarHeight;

                _deltaSize.Y = Math.Min(maxDeltaHeight, actualHeightDiff);
            }            

            //make change in height so window doesn't get smaller but also doesn't extend past top of screen
            //_deltaSize.Height = Math.Min(maxDeltaHeight, deltaContentHeight);
            //sanity check so heights are the same after all that
            _deltaSize.X =  mwvm.ClipTrayWidth - ctvm.TileBorderWidth - MpMeasurements.Instance.ClipTileExpandedMargin;

            mwvm.MainWindowTop -= _deltaSize.Y;
            _initialExpandedMainWindowTop = mwvm.MainWindowTop;

            mwvm.ClipTrayHeight += _deltaSize.Y;

            ctvm.TileBorderWidth += _deltaSize.X;
            ctvm.TileBorderHeight += _deltaSize.Y;

            ctvm.TileContentWidth += _deltaSize.X;
            ctvm.TileContentHeight += _deltaSize.Y;

            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            if(clv != null) {
                clv.EditToolbarView.Visibility = Visibility.Visible;
                clv.UpdateLayout();
            } else {
                Debugger.Break();
            }

            var civl = AssociatedObject.GetVisualDescendents<MpContentItemView>().ToList();
            foreach(var civ in civl) {
                var civm = civ.DataContext as MpContentItemViewModel;
                civm.OnPropertyChanged(nameof(civm.EditorHeight));
                civm.OnPropertyChanged(nameof(civm.IsEditingContent));
                if(ctvm.IsTextItem) {
                    var rtbv = civ.GetVisualDescendent<MpRtbView>();
                    rtbv.Rtb.FitDocToRtb();
                    if (civm.IsSelected) {
                        rtbv.Focus();
                        rtbv.Rtb.CaretPosition = rtbv.Rtb.Document.ContentStart;
                    }
                }
            }

            if(clv != null) {
                var sv = clv.ContentListBox.GetScrollViewer();
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                clv.UpdateAdorner();
            }
            AssociatedObject.UpdateLayout();

            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel += ApplicationHook_MouseWheel;

            IsExpandingOrUnexpanding = false;
        }

        public void Unexpand() {
            MpConsole.WriteLine("Unexpanding...");
            IsExpandingOrUnexpanding = true;

            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            var mwvm = MpMainWindowViewModel.Instance;

            //trigger app mode column to hide
            ctvm.OnPropertyChanged(nameof(ctvm.FlipButtonVisibility));
            ctvm.Parent.OnPropertyChanged(nameof(ctvm.Parent.IsAnyTileExpanded));
            mwvm.OnPropertyChanged(nameof(mwvm.AppModeButtonGridWidth));

            double temp = mwvm.MainWindowTop;
            //this resets window top to standard 
            MpMainWindowViewModel.Instance.SetupMainWindowRect();
            double deltaHeight = _originalMainWindowTop - temp;

            mwvm.MainWindowTop = _originalMainWindowTop;
            mwvm.ClipTrayHeight -= deltaHeight;

            var ctrvm = MpClipTrayViewModel.Instance;

            ctvm.TileBorderWidth = MpMeasurements.Instance.ClipTileBorderMinSize;
            ctvm.TileBorderHeight = MpMeasurements.Instance.ClipTileMinSize;

            ctvm.TileContentWidth = MpMeasurements.Instance.ClipTileContentMinWidth;
            ctvm.TileContentHeight = MpMeasurements.Instance.ClipTileContentHeight;

            ctrvm.Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsPlaceholder)));

            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            if(clv != null) {
                clv.EditToolbarView.Visibility = Visibility.Collapsed;
                clv.UpdateLayout();

                var sv = clv.ContentListBox.GetScrollViewer();
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

                for (int i = 0; i < clv.ContentListBox.Items.Count; i++) {
                    var civ = clv.ContentListBox.GetListBoxItem(i).GetVisualDescendent<MpContentItemView>();
                    MpHelpers.Instance.RunOnMainThread(async () => {
                        await civ.GetVisualDescendent<MpRtbView>().SyncModelsAsync();
                    });
                    var civm = civ.BindingContext;
                    civm.ClearEditing();
                    civm.OnPropertyChanged(nameof(civm.EditorHeight));
                    civm.OnPropertyChanged(nameof(civm.IsEditingContent));
                    if(ctvm.IsTextItem) {
                        var rtbv = civ.GetVisualDescendent<MpRtbView>();                        
                        rtbv.Rtb.FitDocToRtb();
                    }                    
                }

                clv.UpdateAdorner();
            }

            //AssociatedObject.GetVisualAncestor<MpClipTrayView>().ClipTray.Items.Refresh();

            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseWheel -= ApplicationHook_MouseWheel;

            IsExpandingOrUnexpanding = false;
        }

        private void ApplicationHook_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            var clv = AssociatedObject.GetVisualDescendent<MpContentListView>();
            clv.Civm_OnScrollWheelRequest(this, -e.Delta);
        }

        private void MpTileExpanderBehavior_MouseWheel(object sender, MouseWheelEventArgs e) {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta / 10));
            e.Handled = true;
        }
    }
}
