using System;
using System.Windows;
using System.Windows.Threading;

namespace MpWpfApp {
    public partial class MpMainWindow : Window {

        public MpMainWindow() {
            InitializeComponent();
            var mwvm = (MpMainWindowViewModel)DataContext;
            //mwvm.InitData();
        }
    }
}
