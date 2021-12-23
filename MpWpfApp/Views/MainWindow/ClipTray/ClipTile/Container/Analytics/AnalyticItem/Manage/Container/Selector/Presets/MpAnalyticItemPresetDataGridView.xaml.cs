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
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAnalyticItemPresetDataGridView : MpUserControl<MpAnalyticItemViewModel> {
        public MpAnalyticItemPresetDataGridView() {
            InitializeComponent();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e) {            
            var pvm = BindingContext.SelectedPresetViewModel;
            if (pvm != null && pvm.IsDefault) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Invalid;
            } else {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }
    }
}
