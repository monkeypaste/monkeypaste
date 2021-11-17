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
        public List<Tuple<TextBlock,RichTextBox>> TitlesAndRtbs = new List<Tuple<TextBlock, RichTextBox>>();

        public MpClipTileView() {
            InitializeComponent();
        }
        private void ClipTileClipBorder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null && e.OldValue is MpClipTileViewModel octvm) {
                octvm.OnFocusRequest -= Nctvm_OnFocusRequest;
                octvm.OnSearchRequest -= Ctvm_OnSearchRequest;
                octvm.PropertyChanged -= Ctvm_PropertyChanged;
            }
            if (e.NewValue != null && e.NewValue is MpClipTileViewModel nctvm) {
                nctvm.OnFocusRequest += Nctvm_OnFocusRequest;
                nctvm.OnSearchRequest += Ctvm_OnSearchRequest;
                nctvm.PropertyChanged += Ctvm_PropertyChanged;
            }
        }

        private void Nctvm_OnFocusRequest(object sender, EventArgs e) {
            Keyboard.Focus(sender as FrameworkElement);
            bool result = Focus();
            //MpConsole.WriteLine($"{BindingContext.PrimaryItem.CopyItemTitle} Got Focus: {(result ? "TRUE" : "FALSE")}");
        }

        private void ClipTileClipBorder_Loaded(object sender, RoutedEventArgs e) {
            TitlesAndRtbs.Clear();
            var rtbvl = this.GetVisualDescendents<MpRtbView>();

            foreach (var rtbv in rtbvl) {
                TitlesAndRtbs.Add(new Tuple<TextBlock,RichTextBox>(
                    new TextBlock() {
                        Text = rtbv.BindingContext.CopyItemTitle
                    },
                    rtbv.Rtb));
            }
        }

        private void Ctvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var ctvm = sender as MpClipTileViewModel;
            switch (e.PropertyName) {
                case nameof(ctvm.IsBusy):
                    if(ctvm.IsBusy) {
                        //ShowBusySpinner();
                    } else {
                       // ContentListView.UpdateAdorner();
                    }
                    break;
                case nameof(ctvm.HeadItem):
                    return;
                    break;
            }
        }

        private void ShowBusySpinner() {
            //ClipTileBusyView.Visibility = Visibility.Visible;
            //ClipTileDockPanel.Visibility = Visibility.Hidden;
        }

        private void HideBusySpinner() {
           // ClipTileBusyView.Visibility = Visibility.Hidden;
           // ClipTileDockPanel.Visibility = Visibility.Visible;
        }

        #region Selection
        private void ClipTileClipBorder_MouseEnter(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsHovering = true;
        }

        private void ClipTileClipBorder_MouseLeave(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsHovering = false;
        }

        private void ClipTileClipBorder_LostFocus(object sender, RoutedEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;

            if (!ctvm.IsSelected) {
                ctvm.ClearEditing();
            }
        }
        #endregion


        private async void Ctvm_OnSearchRequest(object sender, string e) {
            if(BindingContext.IsPlaceholder) {
                return;
            }
            var result = await BindingContext.HighlightTextRangeViewModelCollection.PerformHighlightingAsync(e, TitlesAndRtbs);
        }

        private void ClipTileClipBorder_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(ClipTileClipBorder.Visibility == Visibility.Visible) {
               // ShowBusySpinner();
            }
        }

        private void ClipTileClipBorder_Unloaded(object sender, RoutedEventArgs e) {
            if(BindingContext == null) {
                return;
            }
            BindingContext.OnSearchRequest -= Ctvm_OnSearchRequest;
            BindingContext.PropertyChanged -= Ctvm_PropertyChanged;
        }
    }
}
