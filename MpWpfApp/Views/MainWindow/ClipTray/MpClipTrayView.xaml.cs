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

            ClipTray.ScrollViewer.Margin = new Thickness(5, 0, 5, 0);

            //ClipTray.PreviewMouseLeftButtonDown += (s, e2) => { ShiftLeft(); e2.Handled = true; };

            //ClipTray.PreviewMouseRightButtonDown += (s, e3) => { ShiftRight(); e3.Handled = true; };

            if (ClipTray.Items.Count > 0) {
                ClipTray.GetListBoxItem(0).Focus();
            }
        }

        private void ShiftLeft() {
            var ctrvm = DataContext as MpClipTrayViewModel;
            ctrvm.ClipTileViewModels.Move(0, ctrvm.ClipTileViewModels.Count - 1);
            return;
            if(ClipTray.Items.Count <= 1) {
                return;
            }

            var firstItem = ctrvm.ClipTileViewModels[0];
            for (int i = 0; i < ClipTray.Items.Count; i++) {
                var lbi = ClipTray.GetListBoxItem(i).GetVisualDescendent<MpClipTileView>();
                if(i == ClipTray.Items.Count - 1) {
                    lbi.DataContext = firstItem;
                } else {
                    lbi.DataContext = ctrvm.ClipTileViewModels[i + 1];
                }
            }
        }

        private void ShiftRight() {
            var ctrvm = DataContext as MpClipTrayViewModel;
            ctrvm.ClipTileViewModels.Move(0, 1);
            return;

            if (ClipTray.Items.Count <= 1) {
                return;
            }

            int count = ctrvm.ClipTileViewModels.Count;
            var lastItem = ctrvm.ClipTileViewModels[count-1];
            for (int i = count - 1; i >= 0; i--) {
                var lbi = ClipTray.GetListBoxItem(i);
                if (i == 0) {
                    lbi.DataContext = lastItem;
                } else {
                    lbi.DataContext = ctrvm.ClipTileViewModels[i - 1];
                }
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

        public List<Rect> GetSelectedContentItemViewRects(FrameworkElement relativeTo) {
            var scivml = MpClipTrayViewModel.Instance.SelectedContentItemViewModels;
            var civl = new List<Rect>();
            for (int i = 0; i < ClipTray.Items.Count; i++) {
                var clv = ClipTray.GetListBoxItem(i).GetVisualDescendent<MpContentListView>();
                for (int j = 0; j < clv.ContentListBox.Items.Count; j++) {
                    int idx = scivml.IndexOf(clv.ContentListBox.Items[j] as MpContentItemViewModel);
                    if(idx >= 0) {
                        var rect = clv.ContentListBox.GetListBoxItemRect(j);
                        rect.Location = clv.ContentListBox.TranslatePoint(rect.Location, relativeTo);
                        civl.Add(rect);
                    }
                }
            }
            return civl;
        }

        #region Selection
        private void ClipTray_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            ctrvm.MergeClipsCommandVisibility = ctrvm.MergeSelectedClipsCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;

            MpTagTrayViewModel.Instance.UpdateTagAssociation();

            if (ctrvm.PrimaryItem != null) {
                ctrvm.PrimaryItem.OnPropertyChanged_old(nameof(ctrvm.PrimaryItem.TileBorderBrush));
            }

            MpAppModeViewModel.Instance.RefreshState();
        }

        private void ClipTray_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            if (!ctrvm.IsAnyTileExpanded) {
                return;
            }
            var selectedClipTilesHoveringOnMouseDown = ctrvm.SelectedItems.Where(x => x.IsHovering).ToList();
            if (selectedClipTilesHoveringOnMouseDown.Count == 0 &&
               !MpSearchBoxViewModel.Instance.IsTextBoxFocused) {
                ctrvm.ClearClipEditing();
            }
        }
        #endregion

        #region View Model Requests (should be able to refactor these away

        private void Ctrvm_OnUiRefreshRequest(object sender, EventArgs e) {
            ClipTray?.Items.Refresh();
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

            int leftIdx = -1;
            for (int i = 0; i < ClipTray.Items.Count; i++) {                
                var lbir = ClipTray.GetListBoxItemRect(i,true);
                if(lbir.Right < 0) {
                    _remainingItems--;
                    leftIdx = i + 1;
                }
            }
            if(_remainingItems <= 1 && !MpMainWindowViewModel.IsMainWindowLoading) {
               // ctrvm.RefreshClips(true, "CopyDateTime", leftIdx, MpMeasurements.Instance.TotalVisibleClipTiles * 2);
            }
        }

        #endregion

        private void ClipTrayVirtualizingStackPanel_Loaded(object sender, RoutedEventArgs e) {
            TrayItemsPanel = sender as VirtualizingStackPanel;
        }
    }
}
