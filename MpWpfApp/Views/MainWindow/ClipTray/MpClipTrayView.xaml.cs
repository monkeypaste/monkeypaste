using MonkeyPaste;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTrayView.xaml
    /// </summary>
    public partial class MpClipTrayView : MpUserControl<MpClipTrayViewModel> {
        public VirtualizingStackPanel TrayItemsPanel;
        //public static ScrollViewer ScrollViewer;

        public MpClipTrayView() : base() {
            InitializeComponent();
        }

        private void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            //MpClipboardManager.Instance.Init();
            ////MpClipboardManager.Instance.ClipboardChanged += BindingContext.ClipboardChanged;
            //MpClipboardHelper.MpClipboardMonitor.OnClipboardChange += BindingContext.ClipboardChanged;
            //MpClipboardHelper.MpClipboardMonitor.Start();

            
            if (MpPreferences.IsInitialLoad) {
                BindingContext.InitIntroItems();
            }

            BindingContext.OnScrollIntoViewRequest += Ctrvm_OnScrollIntoViewRequest;
            BindingContext.OnScrollToHomeRequest += Ctrvm_OnScrollToHomeRequest;
            BindingContext.OnFocusRequest += Ctrvm_OnFocusRequest;
            BindingContext.OnUiRefreshRequest += Ctrvm_OnUiRefreshRequest;
            BindingContext.OnScrollToXRequest += Ctrvm_OnScrollToXRequest;

            MpMessenger.Register<MpMessageType>(
                MpMainWindowViewModel.Instance, ReceivedMainWindowViewModelMessage);

            //MpHelpers.RunOnMainThread(async () => {
            //    var sv = ClipTray.GetScrollViewer();
            //    while (sv == null) {
            //        await Task.Delay(10);
            //        sv = ClipTray.GetScrollViewer();
            //    }
            //    PagingScrollViewer.RequestBringIntoView += ClipTray_RequestBringIntoView;
            //});
            PagingScrollViewer.RequestBringIntoView += ClipTray_RequestBringIntoView;
        }

        private void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowOpening: 
                    if(BindingContext.SelectedItems.Count >= 1 && 
                       BindingContext.SelectedItems[0].ItemIdx > 0) {
                        return;
                    }
                    //var sv = ClipTray.GetScrollViewer();
                    PagingScrollViewer.ScrollToHorizontalOffset(0);
                    PagingScrollViewer.ScrollToLeftEnd();
                    break;
                case MpMessageType.Expand:

                    break;
            }
        }


        private void Ctrvm_OnScrollToXRequest(object sender, double e) {
           // var sv = ClipTray.GetScrollViewer();
            PagingScrollViewer.ScrollToHorizontalOffset(e);
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
            PagingScrollViewer.ScrollToLeftEnd();
        }

        private void Ctrvm_OnScrollIntoViewRequest(object sender, object e) {
            ClipTray?.ScrollIntoView(e);
        }
        #endregion

        private void ClipTray_CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e) {
            return;
        }

        private void ClipTray_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            e.Handled = true;
        }
    }
}
