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

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTrayView.xaml
    /// </summary>
    public partial class MpClipTrayView : UserControl {
        public VirtualizingStackPanel TrayItemsPanel;

        private int _remainingItems = int.MaxValue;

        public MpClipTrayView() : base() {
            InitializeComponent();
        }

        private void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;

            MpClipboardManager.Instance.Init();
            MpClipboardManager.Instance.ClipboardChanged += (s, e1) => ctrvm.AddItemFromClipboard();

            _remainingItems = ctrvm.ClipTileViewModels.Count - MpMeasurements.Instance.TotalVisibleClipTiles;

            if (MpPreferences.Instance.IsInitialLoad) {
                ctrvm.InitIntroItems();
            }

           // ClipTray.ScrollViewer.Margin = new Thickness(5, 0, 5, 0);

            //ClipTray.PreviewMouseLeftButtonDown += (s, e2) => { ShiftLeft(); e2.Handled = true; };

            //ClipTray.PreviewMouseRightButtonDown += (s, e3) => { ShiftRight(); e3.Handled = true; };

            if (ClipTray.Items.Count > 0) {
                ClipTray.GetListBoxItem(0).Focus();
            }
        }


        private void ClipTray_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (DataContext != null) {
                var ctrvm = DataContext as MpClipTrayViewModel;
                ctrvm.OnScrollIntoViewRequest += Ctrvm_OnScrollIntoViewRequest;
                ctrvm.OnScrollToHomeRequest += Ctrvm_OnScrollToHomeRequest;
                ctrvm.OnFocusRequest += Ctrvm_OnFocusRequest;
                ctrvm.OnUiRefreshRequest += Ctrvm_OnUiRefreshRequest;
            }
        }

        #region Selection
        private void ClipTray_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            ctrvm.MergeClipsCommandVisibility = ctrvm.MergeSelectedClipsCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;

            MpTagTrayViewModel.Instance.UpdateTagAssociation();

            if (ctrvm.PrimaryItem != null) {
                ctrvm.PrimaryItem.OnPropertyChanged(nameof(ctrvm.PrimaryItem.TileBorderBrush));
            }

            MpAppModeViewModel.Instance.RefreshState();
        }

        #endregion

        #region View Model Requests (should be able to refactor these away

        private void Ctrvm_OnUiRefreshRequest(object sender, EventArgs e) {
            //ClipTray?.Items.Refresh();
            UpdateLayout();
        }

        private void Ctrvm_OnFocusRequest(object sender, object e) {
            ClipTray?.GetListBoxItem(ClipTray.Items.IndexOf(e)).Focus();
        }

        private void Ctrvm_OnScrollToHomeRequest(object sender, EventArgs e) {
            ClipTray?.GetScrollViewer().ScrollToHome();
        }

        private void Ctrvm_OnScrollIntoViewRequest(object sender, object e) {
            ClipTray?.ScrollIntoView(e);
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
                    ctrvm.RecycleLeftItem();
                }
            } else if(e.HorizontalChange < 0) {

            }
        }

        #endregion

        private void ClipTrayVirtualizingStackPanel_Loaded(object sender, RoutedEventArgs e) {
            //TrayItemsPanel = sender as VirtualizingStackPanel;
        }

        private void ClipTray_CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e) {
            return;
        }
    }
}
