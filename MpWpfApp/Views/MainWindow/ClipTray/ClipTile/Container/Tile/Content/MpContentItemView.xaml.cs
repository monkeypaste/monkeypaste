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

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpContentItemView.xaml
    /// </summary>
    public partial class MpContentItemView : MpUserControl<MpContentItemViewModel> {
        public MpContentItemView() : base() {
            InitializeComponent();            
        }

        private void ContentListItemView_Loaded(object sender, RoutedEventArgs e) {
            var mwvm = Application.Current.MainWindow.DataContext as MpMainWindowViewModel;

            var civm = DataContext as MpContentItemViewModel;
            var scvml = MpShortcutCollectionViewModel.Instance.Shortcuts.Where(x => x.CopyItemId == civm.CopyItemId).ToList();
            if (scvml.Count > 0) {
                civm.ShortcutKeyString = scvml[0].KeyString;
            } else {
                civm.ShortcutKeyString = string.Empty;
            }
        }

        private void ContentListItemView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null && e.OldValue is MpContentItemViewModel ocivm) {
                ocivm.OnScrollWheelRequest -= Civm_OnScrollWheelRequest;
                ocivm.OnUiUpdateRequest -= Civm_OnUiUpdateRequest;
            }
            if (e.NewValue != null && e.NewValue is MpContentItemViewModel ncivm) {
                if (!ncivm.IsPlaceholder) {
                    ncivm.OnScrollWheelRequest += Civm_OnScrollWheelRequest;
                    ncivm.OnUiUpdateRequest += Civm_OnUiUpdateRequest;
                }
            }
        }

        private void ContentListItemView_MouseEnter(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            civm.IsHovering = true;
        }

        private void ContentListItemView_MouseLeave(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            civm.IsHovering = false;
        }
        #region Event Handlers

        #region View Model Ui Requests

        private void Civm_OnUiUpdateRequest(object sender, EventArgs e) {
            this.UpdateLayout();            
        }

        private void Civm_OnScrollWheelRequest(object sender, int e) {
            var civm = DataContext as MpContentItemViewModel;
            if(civm.IsEditingContent) {
                var sv = this.GetVisualAncestor<ScrollViewer>();
                sv.ScrollToVerticalOffset(sv.VerticalOffset + e);
            }            
        }

        #endregion

        #endregion

        private void Border_Unloaded(object sender, RoutedEventArgs e) {
            if(BindingContext != null) {
                BindingContext.OnScrollWheelRequest -= Civm_OnScrollWheelRequest;
                BindingContext.OnUiUpdateRequest -= Civm_OnUiUpdateRequest;
            }
        }
    }
}
