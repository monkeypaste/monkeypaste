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
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpAssignHotkeyModalWindow.xaml
    /// </summary>
    public partial class MpTemplateTokenEditModalWindow : Window {
        public MpTemplateTokenEditModalWindow(RichTextBox rtb, Hyperlink templateLink, bool isEditMode) {
            this.DataContext = new MpTemplateTokenEditModalWindowViewModel(rtb,templateLink, isEditMode);
            InitializeComponent();
        }
        //public MpTemplateTokenEditModalWindow(RichTextBox rtb, MpTemplateHyperlinkViewModel thlvm, bool isEditMode) {
        //    this.DataContext = new MpTemplateTokenEditModalWindowViewModel(rtb, thlvm, isEditMode);
        //    InitializeComponent();
        //}
    }
}
