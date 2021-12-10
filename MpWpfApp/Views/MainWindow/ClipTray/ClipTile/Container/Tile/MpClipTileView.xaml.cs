using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileView.xaml
    /// </summary>
    public partial class MpClipTileView : MpUserControl<MpClipTileViewModel> {
       public MpClipTileView() {
            InitializeComponent();
        }
        private void ClipTileClipBorder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null && e.OldValue is MpClipTileViewModel octvm) {
                octvm.OnFocusRequest -= Nctvm_OnFocusRequest;
                octvm.OnSearchRequest -= Ctvm_OnSearchRequest;
            }
            if (e.NewValue != null && e.NewValue is MpClipTileViewModel nctvm) {
                nctvm.OnFocusRequest += Nctvm_OnFocusRequest;
                nctvm.OnSearchRequest += Ctvm_OnSearchRequest;
                UpdateLayout();
            }
        }

        private void Nctvm_OnFocusRequest(object sender, EventArgs e) {
            var ctcv = this.GetVisualAncestor<MpClipTileContainerView>();
            if(ctcv != null) {
                ctcv.Focus();
            }
        }

        #region Selection
        private void ClipTileClipBorder_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.IsHovering = true;
        }

        private void ClipTileClipBorder_MouseLeave(object sender, MouseEventArgs e) {
            BindingContext.IsHovering = false;
        }

        private void ClipTileClipBorder_LostFocus(object sender, RoutedEventArgs e) {
            if (!BindingContext.IsSelected) {
                BindingContext.ClearEditing();
            }
        }
        #endregion


        private void Ctvm_OnSearchRequest(object sender, string e) {
            if(BindingContext.IsPlaceholder) {
                return;
            }
            //var result = await BindingContext.HighlightTextRangeViewModelCollection.PerformHighlightingAsync(e, TitlesAndRtbs);
        }


        private void ClipTileClipBorder_Unloaded(object sender, RoutedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            BindingContext.OnSearchRequest -= Ctvm_OnSearchRequest;
        }

        private void ClipTileClipBorder_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            e.Handled = true;
        }
    }
}
