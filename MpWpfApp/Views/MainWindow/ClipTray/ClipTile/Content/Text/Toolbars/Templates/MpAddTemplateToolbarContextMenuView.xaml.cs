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
            SetActiveRtb(rtb);            
        }

        public void SetActiveRtb(RichTextBox rtb) {
            Rtb = rtb;
        }

        private void AddTemplateContextMenu_Opened(object sender, RoutedEventArgs e) {
            var tc = (Rtb.DataContext as MpContentItemViewModel).TokenCollection.Tokens;

            var mil = new List<MenuItem>();
            foreach(var thvm in tc) {
                var mi = new MenuItem() {
                    Header = thvm.TemplateDisplayName,
                    Icon = new Border() { 
                        BorderBrush = Brushes.Black,
                        Background = thvm.TemplateBrush
                    },
                    DataContext = thvm
                };
                mil.Add(mi);
            }
            
            mil.Add(new MenuItem() {
                Header = "Add New...",
                Icon = (BitmapSource)new BitmapImage(new Uri(@"pack://application:,,,/Resources/Icons/Silk/icons/add.png"))
            });

            foreach (MenuItem mi in mil) {
                mi.Click += Template_Click;
            }

            AddButton.ContextMenu.ItemsSource = mil;
        }

        private void Template_Click(object sender, RoutedEventArgs e) {
            var thlcvm = DataContext as MpTokenCollectionViewModel;
            Rtb.Focus();

            var mi = sender as MenuItem;
            if(mi.DataContext == null) {
                //when clicking add new
                var thl = MpTemplateHyperlink.Create(Rtb.Selection,null);
                var thlvm = thl.DataContext as MpTokenViewModel;
                thlvm.EditTemplateCommand.Execute(null);
            } else if(mi.DataContext is MpTokenViewModel thlvm) {
                //when clicking a pre-existing template
                var nthl = MpTemplateHyperlink.Create(Rtb.Selection, thlvm.CopyItemTemplate);
                var nthlvm = nthl.DataContext as MpTokenViewModel;
                nthlvm.EditTemplateCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            if(Rtb == null || Rtb.DataContext == null) {
                MonkeyPaste.MpConsole.WriteTraceLine("No rtb or rtb context");
                return;
            }
            var rtbvm = Rtb.DataContext as MpContentItemViewModel;
            if(rtbvm.TokenCollection.Tokens.Count == 0) {
                //when no templates exist create a new default one
                var thl = MpTemplateHyperlink.Create(Rtb.Selection, null);
                var thlvm = thl.DataContext as MpTokenViewModel;
                thlvm.EditTemplateCommand.Execute(null);
            } else {
                //otherwise show template menu
                AddButton.ContextMenu.IsOpen = true;
            }
        }
    }
}
