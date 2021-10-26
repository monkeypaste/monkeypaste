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
using System.Windows.Controls.Primitives;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTrayView.xaml
    /// </summary>
    public partial class MpClipTrayView : MpUserControl<MpClipTrayViewModel> {
        public VirtualizingStackPanel TrayItemsPanel;

        public MpClipTrayView() : base() {
            InitializeComponent();
        }

        private void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;

            MpClipboardManager.Instance.Init();
            MpClipboardManager.Instance.ClipboardChanged += ctrvm.OnClipboardChanged;
         
            if (MpPreferences.Instance.IsInitialLoad) {
                ctrvm.InitIntroItems();
            }

            ctrvm.OnScrollIntoViewRequest += Ctrvm_OnScrollIntoViewRequest;
            ctrvm.OnScrollToHomeRequest += Ctrvm_OnScrollToHomeRequest;
            ctrvm.OnFocusRequest += Ctrvm_OnFocusRequest;
            ctrvm.OnUiRefreshRequest += Ctrvm_OnUiRefreshRequest;
            ctrvm.OnScrollToXRequest += Ctrvm_OnScrollToXRequest;

            var sv = ClipTray.GetScrollViewer() as AnimatedScrollViewer;
            sv.PreviewMouseDown += Sv_PreviewMouseDown;

            MpMessenger.Instance.Register<MpMessageType>(MpMainWindowViewModel.Instance, ReceivedMainWindowViewModelMessage);

            MpMessenger.Instance.Register<MpMessageType>(ctrvm, ReceivedClipTrayViewModelMessage);
        }

        private void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowOpening:
                    ClipTray.GetScrollViewer().ScrollToHorizontalOffset(0);
                    break;
            }
        }


        private void Ctrvm_OnScrollToXRequest(object sender, double e) {
            var sv = ClipTray.GetScrollViewer() as AnimatedScrollViewer;
            //sv.ScrollToHorizontalOffset(e);
            sv.TargetHorizontalOffset = e;
        }

        private void Sb_MouseUp(object sender, MouseButtonEventArgs e) {
            //throw new NotImplementedException();
        }

        private void Sv_PreviewMouseMove(object sender, MouseEventArgs e) {
            //throw new NotImplementedException();
        }

        private void Sv_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var sv = sender as AnimatedScrollViewer;
            var hsb = sv.HorizontalScrollBar;
            var htrack = hsb.Track;
            var hthumb = htrack.Thumb;

            var hsb_mp = e.GetPosition(hsb);
            if(hsb_mp.Y < 0) {
                return;
            }
            var hthumb_rect = hthumb.Bounds();
            if(hthumb_rect.Contains(e.GetPosition(hthumb))) {
                return;
            }
            e.Handled = true;
            double norm_x = e.GetPosition(sv).X / sv.ActualWidth;

            int targetTileIdx = (int)Math.Floor(norm_x * MpClipTrayViewModel.Instance.TotalItemsInQuery);

            MpClipTrayViewModel.Instance.LoadToIdxCommand.Execute(targetTileIdx);
        }

        private void Sv_MouseWheel(object sender, MouseWheelEventArgs e) {
            //does nothing but without setting this up in load the tray will always load in the center of the list!!
            return;
        }

        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.Requery:
                    //await RefreshContext();
                    var sv = ClipTray.GetScrollViewer() as AnimatedScrollViewer;
                    if(sv != null) {
                        double tw = MpMeasurements.Instance.ClipTileBorderMinSize;
                        double ttw = tw * BindingContext.TotalItemsInQuery;
                        sv.HorizontalScrollBar.Maximum = ttw;
                        sv.HorizontalScrollBar.Minimum = 0;
                        sv.ScrollToLeftEnd();
                        UpdateLayout();
                    }
                    break;
                case MpMessageType.Expand:
                    //TrayItemsPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case MpMessageType.Unexpand:
                    //TrayItemsPanel.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
            }
        }
        public async Task RefreshContext() {
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                for (int i = 0; i < ClipTray.Items.Count; i++) {
                    var lbi = ClipTray.GetListBoxItem(i);
                    if (lbi != null) {
                        if (i < MpClipTrayViewModel.Instance.Items.Count) {
                            lbi.DataContext = MpClipTrayViewModel.Instance.Items[i];
                        }
                    }
                }

                //UpdateLayout();
                //InvalidateArrange();
                //ClipTray.InvalidateArrange();
                //ClipTray.GetScrollViewer().ScrollToHorizontalOffset(double.MinValue);
            });
        }


        #region Selection
        private async void ClipTray_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            MpAppModeViewModel.Instance.RefreshState();

            BindingContext.RefreshAllCommands();

            await MpTagTrayViewModel.Instance.UpdateTagAssociation();
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
            ClipTray?.GetScrollViewer().ScrollToLeftEnd();
        }

        private void Ctrvm_OnScrollIntoViewRequest(object sender, object e) {
            ClipTray?.ScrollIntoView(e);
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
