using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MpWpfApp {
    public partial class MpMainWindow : Window {
        public readonly SynchronizationContext SyncContext;
        public MpMainWindow() {
            InitializeComponent();
            //SyncContext = SynchronizationContext.Current;
        }

        private void MainWindow_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            return;
        }

        private void MainWindow_Initialized(object sender, EventArgs e) {
            if (DataContext != null) {
                (DataContext as MpMainWindowViewModel).MainWindow_Loaded(this, null);
            }
        }
    }
}
