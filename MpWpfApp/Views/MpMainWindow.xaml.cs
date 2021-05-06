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
            SyncContext = SynchronizationContext.Current;
        }
    }
}
