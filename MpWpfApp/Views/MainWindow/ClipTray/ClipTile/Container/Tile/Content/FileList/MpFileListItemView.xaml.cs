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
    /// Interaction logic for MpFileListItemView.xaml
    /// </summary>
    public partial class MpFileListItemView : MpContentUserControl<MpContentItemViewModel> {
        public MpFileListItemView() : base() {
            InitializeComponent();
        }

        protected override void RegisterAllBehaviors() {
            RegisterBehavior(FileListItemHighlightBehavior);
        }

        private void FileListItemPanel_Loaded(object sender, RoutedEventArgs e) {
            base.OnLoad();

            //BindingContext.UnformattedContentSize = new Size(ActualWidth, ActualHeight);
            
            if (BindingContext.ItemIdx == BindingContext.Parent.Count - 1) {
                MpHelpers.RunOnMainThread(async () => {
                    var clv = this.GetVisualAncestor<MpContentListView>();
                    while (clv == null) {
                        clv = this.GetVisualAncestor<MpContentListView>();
                        await Task.Delay(100);
                    }
                    clv.RegisterMouseWheel();
                });
            }
        }

        private void FileListItemPanel_Unloaded(object sender, RoutedEventArgs e) {
            base.OnUnload();
        }
    }
}
