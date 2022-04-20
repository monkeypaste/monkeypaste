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
using MonkeyPaste.Plugin;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileView.xaml
    /// </summary>
    public partial class MpClipTileView : MpUserControl<MpClipTileViewModel> {
       public MpClipTileView() {
            InitializeComponent();
        }

        private void ClipTileClipBorder_Loaded(object sender, RoutedEventArgs e) {
            //HighlightSelectorBehavior.Attach(this);
            

        }

        private void ClipTileClipBorder_Unloaded(object sender, RoutedEventArgs e) {
            //HighlightSelectorBehavior.Detach();

        }
        private void ClipTileClipBorder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {

            UpdateLayout();
        }

        private void Nctvm_OnFocusRequest(object sender, EventArgs e) {
            Focus();
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
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null && e.OldValue is MpClipTileViewModel octvm) {
                octvm.OnUiUpdateRequest -= Rtbcvm_OnUiUpdateRequest;
                octvm.OnScrollToHomeRequest -= Rtbcvm_OnScrollToHomeRequest;
                octvm.PropertyChanged -= Rtbcvm_PropertyChanged;
            }
            if (e.NewValue != null && e.NewValue is MpClipTileViewModel nctvm) {
                nctvm.OnUiUpdateRequest += Rtbcvm_OnUiUpdateRequest;
                nctvm.OnScrollToHomeRequest += Rtbcvm_OnScrollToHomeRequest;
                nctvm.PropertyChanged += Rtbcvm_PropertyChanged;
            }
        }
        private void Rtbcvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var ctvm = sender as MpClipTileViewModel;
            switch (e.PropertyName) {
                case nameof(ctvm.IsSelected):
                    break;
                case nameof(ctvm.IsBusy):
                    if (!ctvm.IsBusy) {
                        UpdateLayout();
                    }
                    break;
            }
        }

        private void Rtbcvm_OnUiUpdateRequest(object sender, EventArgs e) {
            UpdateLayout();
        }

        private void Rtbcvm_OnScrollToHomeRequest(object sender, EventArgs e) {
            ContentView.ScrollToHome();
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

        private void ClipTileClipBorder_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(!(bool)e.NewValue) {
                return;
            }

            MpMarqueeExtension.Init(TileTitleView.ClipTileTitleMarqueeCanvas);
        }
    }
}
