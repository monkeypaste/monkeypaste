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
using System.Windows.Media.Effects;
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
            if(ContentListView != null && ContentListView.ContentListBox != null) {
                bool result = ContentListView.ContentListBox.Focus();
                MpConsole.WriteLine($"Tile {BindingContext.HeadItem.CopyItemTitle} {(result ? "SUCCESSFULLY" : "UNSUCESSFULLY")} received focus");
            }
        }

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

        private void ClipTileClipBorder_MouseMove(object sender, MouseEventArgs e) {
            //0 = Left
            //90 = Top
            //180 = Right
            //270 = Bottom
            var dropShadow = this.GetVisualDescendent<DropShadowEffect>();
            if (dropShadow == null) {
                return;
            }
            var mp = e.GetPosition(this);
            Point center = new Point(ActualWidth / 2, ActualHeight / 2);

            double xDiff = mp.X - center.X;
            double yDiff = mp.Y - center.Y;

            double angle =  Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;

            dropShadow.Direction = 360.0 - angle + 180;
        }
    }
}
