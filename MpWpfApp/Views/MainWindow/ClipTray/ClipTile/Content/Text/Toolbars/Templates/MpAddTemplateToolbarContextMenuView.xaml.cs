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
    /// Interaction logic for MpAddTemplateToolbarContextMenuView.xaml
    /// </summary>
    public partial class MpAddTemplateToolbarButton : UserControl {
        RichTextBox Rtb;
        public MpAddTemplateToolbarButton() {
            InitializeComponent();
        }

        public MpAddTemplateToolbarButton(RichTextBox rtb) : this() {
            Rtb = rtb;
            DataContext = (Rtb.DataContext as MpRtbItemViewModel).TemplateHyperlinkCollectionViewModel;
            AddButton.ContextMenu.PlacementTarget = AddButton;
        }

        private void AddTemplateContextMenu_Opened(object sender, RoutedEventArgs e) {
            DataContext = (Rtb.DataContext as MpRtbItemViewModel).TemplateHyperlinkCollectionViewModel;

            var addNewMenuItem = new MenuItem() {
                Header = "Add New...",
                Icon = (BitmapSource)new BitmapImage(new Uri(@"pack://application:,,,/Resources/Icons/Silk/icons/add.png"))
            };

            AddButton.ContextMenu.Items.Add(addNewMenuItem);

            foreach (MenuItem mi in AddButton.ContextMenu.Items) {
                mi.Click += Template_Click;
            }
        }

        private void Template_Click(object sender, RoutedEventArgs e) {
            var thlcvm = DataContext as MpTemplateHyperlinkCollectionViewModel;
            Rtb.Focus();

            var mi = sender as MenuItem;
            if(mi.DataContext == null) {
                var thl = MpTemplateHyperlink.Create(Rtb.Selection,null);
                var thlvm = thl.DataContext as MpTemplateHyperlinkViewModel;
                thlvm.EditTemplateCommand.Execute(null);
            } else if(mi.DataContext is MpTemplateHyperlinkViewModel thlvm) {
                thlvm.EditTemplateCommand.Execute(null);
            }
        }
    }
}
