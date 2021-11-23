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
using System.Windows.Threading;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTrayView.xaml
    /// </summary>
    public partial class MpClipTrayView : MpUserControl<MpClipTrayViewModel> {
        public VirtualizingStackPanel TrayItemsPanel;
        public static ScrollViewer ScrollViewer;

        public MpClipTrayView() : base() {
            InitializeComponent();
        }

        private void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            MpClipboardManager.Instance.Init();
            MpClipboardManager.Instance.ClipboardChanged += BindingContext.OnClipboardChanged;
         
            if (MpPreferences.Instance.IsInitialLoad) {
                BindingContext.InitIntroItems();
            }

            BindingContext.OnScrollIntoViewRequest += Ctrvm_OnScrollIntoViewRequest;
            BindingContext.OnScrollToHomeRequest += Ctrvm_OnScrollToHomeRequest;
            BindingContext.OnFocusRequest += Ctrvm_OnFocusRequest;
            BindingContext.OnUiRefreshRequest += Ctrvm_OnUiRefreshRequest;
            BindingContext.OnScrollToXRequest += Ctrvm_OnScrollToXRequest;

            MpMessenger.Instance.Register<MpMessageType>(MpMainWindowViewModel.Instance, ReceivedMainWindowViewModelMessage);


            MpHelpers.Instance.RunOnMainThread(async () => {
                var sv = ClipTray.GetScrollViewer();
                while(sv == null) {
                    await Task.Delay(10);
                    sv = ClipTray.GetScrollViewer();
                }
                ScrollViewer = sv;
            });
        }

        private void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowOpening:
                    if(BindingContext.SelectedItems.Count >= 1 && 
                       BindingContext.SelectedItems[0].ItemIdx > 0) {
                        return;
                    }
                    var sv = ClipTray.GetScrollViewer();
                    sv.ScrollToHorizontalOffset(0);
                    sv.ScrollToLeftEnd();
                    break;
            }
        }


        private void Ctrvm_OnScrollToXRequest(object sender, double e) {
            var sv = ClipTray.GetScrollViewer();
            sv.ScrollToHorizontalOffset(e);
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
            if(ScrollViewer == null) {
                return;
            }
            ScrollViewer.ScrollToLeftEnd();
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
