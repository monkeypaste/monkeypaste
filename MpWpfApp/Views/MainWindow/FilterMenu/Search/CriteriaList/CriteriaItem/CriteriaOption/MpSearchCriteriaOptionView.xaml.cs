
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
    /// Interaction logic for MpCriteriaItemOptionView.xaml
    /// </summary>
    public partial class MpSearchCriteriaOptionView : MpUserControl<MpSearchCriteriaOptionViewModel> {
        public MpSearchCriteriaOptionView() {
            InitializeComponent();
        }


        private void DatePicker_Loaded(object sender, RoutedEventArgs e) {
            var dtp = sender as DatePicker;
            dtp.DisplayDateEnd = DateTime.Now;
        }
    }
}
