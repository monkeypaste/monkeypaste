using Avalonia.Markup.Xaml;
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

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAvActionPropertyListBoxView.xaml
    /// </summary>
    public partial class MpAvActionPropertyListBoxView : MpAvUserControl<MpAvActionCollectionViewModel> {
        public MpAvActionPropertyListBoxView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void ActionPropertyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var lb = sender as ListBox;
            lb.ScrollIntoView(MpAvActionCollectionViewModel.Instance.PrimaryAction);
        }
    }
}
