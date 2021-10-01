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
    public partial class MpClipTileView : UserControl {
        public List<TextBlock> Titles = new List<TextBlock>();

        public event EventHandler OnExpandCompleted;
        public event EventHandler OnUnexpandCompleted;

        public MpClipTileView() {
            InitializeComponent();
        }

        public void NotifyExpandCompleted() {
            OnExpandCompleted?.Invoke(this, null);
        }

        public void NotifyUnexpandCompleted() {
            OnUnexpandCompleted?.Invoke(this, null);
        }

        private void ClipTileClipBorder_Loaded(object sender, RoutedEventArgs e) {
            var mwvm = Application.Current.MainWindow.DataContext as MpMainWindowViewModel;

            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsBusy = false;
            //ClipTileBusyView.Visibility = Visibility.Hidden;
            //ClipTileDockPanel.Visibility = Visibility.Visible;
        }

        private void ClipTileClipBorder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            //ClipTileDockPanel.Visibility = Visibility.Hidden;
            //ClipTileBusyView.Visibility = Visibility.Visible;

            if (DataContext != null && DataContext is MpClipTileViewModel ctvm) {
                //ctvm.IsBusy = true;
                ctvm.ViewModelLoaded += Ctvm_ViewModelLoaded;
                ctvm.OnSearchRequest += Ctvm_OnSearchRequest;

                Titles.Clear();
                foreach (var civm in ctvm.ItemViewModels) {
                    Titles.Add(new TextBlock() {
                        Text = civm.CopyItem.Title
                    });
                }

                //Task.Run(async () => {
                //    await Task.Delay(500);
                //    ctvm.IsBusy = false;
                //});
            }
        }

        private void Ctvm_ViewModelLoaded(object sender, EventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
          //  ctvm.IsBusy = false;
        }

        public async Task<MpHighlightTextRangeViewModelCollection> Search(string hlt) {
            var ctvm = DataContext as MpClipTileViewModel;
            var hltrcvm = new MpHighlightTextRangeViewModelCollection();

            var rtbl = this.GetVisualDescendents<RichTextBox>();
            var tl = new List<Tuple<TextBlock, RichTextBox>>();
            foreach (var rtb in rtbl) {
                var rtbvm = rtb.DataContext as MpContentItemViewModel;
                var tb = Titles.Where(x => x.Text == rtbvm.CopyItem.Title).FirstOrDefault();
                tl.Add(new Tuple<TextBlock, RichTextBox>(tb, rtb));
            }
            await hltrcvm.PerformHighlightingAsync(hlt, tl);

            return hltrcvm;
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


        private void Ctvm_OnSearchRequest(object sender, string e) {
           
        }

    }
}
