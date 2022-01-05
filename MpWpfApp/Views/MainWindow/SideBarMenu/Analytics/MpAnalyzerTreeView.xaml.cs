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
    public partial class MpAnalyzerTreeView : MpUserControl<MpAnalyticItemCollectionViewModel> {
        public bool IsWindowed { get; set; } = false;
        public MpAnalyzerTreeView() {
            InitializeComponent();
        }

        public void Close(bool isCancel) {
            if (IsWindowed) {
                this.GetVisualAncestor<Window>().DialogResult = !isCancel;
                this.GetVisualAncestor<Window>().Close();
            } 
        }

        private void GridSplitter_MouseEnter(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.ResizeWE;
        }

        private void GridSplitter_MouseLeave(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }
    }
}
