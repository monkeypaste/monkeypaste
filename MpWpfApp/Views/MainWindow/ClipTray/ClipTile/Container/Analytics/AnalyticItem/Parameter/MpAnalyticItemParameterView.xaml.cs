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
    /// Interaction logic for MpAnalyticItemParameterView.xaml
    /// </summary>
    public partial class MpAnalyticItemParameterView : MpUserControl<MpAnalyticItemParameterViewModel> {
        public MpAnalyticItemParameterView() {
            InitializeComponent();
        }
        private void ExecuteButton_OnClick(object sender, RoutedEventArgs e) {
            if (BindingContext == null || BindingContext.Parent == null) {
                return;
            }
            BindingContext.Parent.WasExecuteClicked = true;
        }
    }
}
