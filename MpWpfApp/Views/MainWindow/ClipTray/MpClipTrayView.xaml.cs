using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Specialized;
using System.Diagnostics;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTrayView.xaml
    /// </summary>
    public partial class MpClipTrayView : MpUserControl<MpClipTrayViewModel> {
        public VirtualizingStackPanel TrayItemsPanel;

        private int _remainingItems = int.MaxValue;

        public MpClipTrayView() : base() {
            InitializeComponent();
        }

        private void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;

            MpMessenger.Instance.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceiveViewModelMessage);

            MpClipboardManager.Instance.Init();
            MpClipboardManager.Instance.ClipboardChanged += ctrvm.AddItemFromClipboard;
            _remainingItems = ctrvm.Items.Count - MpMeasurements.Instance.TotalVisibleClipTiles;

            if (MpPreferences.Instance.IsInitialLoad) {
                ctrvm.InitIntroItems();
            }

            ctrvm.OnScrollIntoViewRequest += Ctrvm_OnScrollIntoViewRequest;
            ctrvm.OnScrollToHomeRequest += Ctrvm_OnScrollToHomeRequest;
            ctrvm.OnFocusRequest += Ctrvm_OnFocusRequest;
            ctrvm.OnUiRefreshRequest += Ctrvm_OnUiRefreshRequest;
            ctrvm.Items.CollectionChanged += Items_CollectionChanged;

            var sv = ClipTray.GetScrollViewer();
            sv.MouseWheel += Sv_MouseWheel;

            Items_CollectionChanged(sender, null);
        }

        private void Sv_MouseWheel(object sender, MouseWheelEventArgs e) {
            //does nothing but without setting this up in load the tray will always load in the center of the list!!
            return;
        }

        private void ReceiveViewModelMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.Requery:
                    UpdateLayout();
                    InvalidateArrange();
                    ClipTray.InvalidateArrange();
                    ClipTray.GetScrollViewer().ScrollToHorizontalOffset(double.MinValue);


                    break;
            }
        }

        public void RefreshContext() {
            for (int i = 0; i < ClipTray.Items.Count; i++) {
                var lbi = ClipTray.GetListBoxItem(i);
                if (lbi != null) {
                    if (i < MpClipTrayViewModel.Instance.Items.Count) {
                        lbi.DataContext = MpClipTrayViewModel.Instance.Items[i];
                    }
                }
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(e != null) {
                MpConsole.WriteLine("Tray collection changed event: " + Enum.GetName(typeof(NotifyCollectionChangedAction), e.Action));
            }
            //when items are 'added' the tail view thinks it has the new head view model
            //so set the tail view to the actual tail view model
            //MpConsole.WriteLine("");
            //MpConsole.WriteLine("Before dc refresh");
            //foreach (var ctvm in BindingContext.Items) {
            //    if (ctvm.IsPlaceholder)
            //        continue;
            //    int vmIdx = BindingContext.Items.IndexOf(ctvm);
            //    List<int> d = new List<int>();
            //    for (int i = 0; i < ClipTray.Items.Count; i++) {
            //        var lbi = ClipTray.GetListBoxItem(i);
            //        if(lbi?.DataContext  == ctvm) {
            //            d.Add(i);
            //        }
            //    }
            //    MpConsole.WriteLine($"Item {ctvm.HeadItem?.CopyItemTitle} view count: {d.Count} views: {string.Join(",", d)}");
            //}

            RefreshContext();

            //MpConsole.WriteLine("After dc refresh");
            //foreach (var ctvm in BindingContext.Items) {
            //    if (ctvm.IsPlaceholder)
            //        continue;
            //    int vmIdx = BindingContext.Items.IndexOf(ctvm);
            //    List<int> d = new List<int>();
            //    for (int i = 0; i < ClipTray.Items.Count; i++) {
            //        var lbi = ClipTray.GetListBoxItem(i);
            //        if (lbi?.DataContext == ctvm) {
            //            d.Add(i);
            //        }
            //    }
            //    MpConsole.WriteLine($"Item {ctvm.HeadItem?.CopyItemTitle} view count: {d.Count} views: {string.Join(",", d)}");
            //}
            //MpConsole.WriteLine("");
        }


        #region Selection
        private void ClipTray_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            //ctrvm.MergeClipsCommandVisibility = ctrvm.MergeSelectedClipsCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;

            MpTagTrayViewModel.Instance.UpdateTagAssociation();

            //if (ctrvm.PrimaryItem != null) {
            //    ctrvm.PrimaryItem.OnPropertyChanged(nameof(ctrvm.PrimaryItem.TileBorderBrush));
            //}

            MpAppModeViewModel.Instance.RefreshState();
        }

        #endregion

        #region View Model Requests (should be able to refactor these away

        private void Ctrvm_OnUiRefreshRequest(object sender, EventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            //ClipTray.ItemsSource = ctrvm.Items;
            //ClipTray?.Items.Refresh();
            UpdateLayout();
        }

        private void Ctrvm_OnFocusRequest(object sender, object e) {
            ClipTray?.GetListBoxItem(ClipTray.Items.IndexOf(e)).Focus();
        }

        private void Ctrvm_OnScrollToHomeRequest(object sender, EventArgs e) {
            //ClipTray?.GetScrollViewer().ScrollToHome();
        }

        private void Ctrvm_OnScrollIntoViewRequest(object sender, object e) {
            //ClipTray?.ScrollIntoView(e);
        }

        private void ClipTray_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            double min_thresh = 18;
            if(e.HorizontalChange > 0) {
                //scrolling higher
                var left_lbir = ClipTray.GetListBoxItemRect(0);
                if(left_lbir.Right < min_thresh) {
                    //left item off screen:
                    //-move to end of list
                    //-set datacontext to next item
                    //ctrvm.RecycleLeftItem();
                }
            } else if(e.HorizontalChange < 0) {

            }
        }

        #endregion

        private void ClipTrayVirtualizingStackPanel_Loaded(object sender, RoutedEventArgs e) {
            TrayItemsPanel = sender as VirtualizingStackPanel;
        }

        private void ClipTray_CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e) {
            return;
        }

        private void ClipTray_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            e.Handled = true;
        }
    }
}
