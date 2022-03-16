using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpEditableListBoxParameterView : MpUserControl<MpEnumerableParameterViewModel> {
        public MpEditableListBoxParameterView() {
            InitializeComponent();
        }

        private void EditableList_Loaded(object sender, RoutedEventArgs e) {
            if(BindingContext.Items.Count == 0) {
                BindingContext.AddValueCommand.Execute(null);
            }
        }
    }
}
