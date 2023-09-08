using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvMultiSelectListBoxParameterView : MpAvUserControl<MpAvMultiEnumerableParameterViewModel> {
        public MpAvMultiSelectListBoxParameterView() {
            InitializeComponent();
            var mslb = this.FindControl<ListBox>("MultiSelectListBox");
            mslb.SelectionChanged += Mslb_SelectionChanged;
            //this.DataContextChanged += MpAvMultiSelectListBoxParameterView_DataContextChanged;
            //if (DataContext != null) {
            //    MpAvMultiSelectListBoxParameterView_DataContextChanged(mslb, null);
            //}
        }

        private void MpAvMultiSelectListBoxParameterView_DataContextChanged(object sender, System.EventArgs e) {
            if (DataContext == null) {
                return;
            }
            if (DataContext is MpIViewModel vm) {
                vm.PropertyChanged += BindingContext_PropertyChanged;
            }
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            Dispatcher.UIThread.Post(() => {
                var mslb = this.FindControl<ListBox>("MultiSelectListBox");
                var bc = DataContext as MpAvMultiEnumerableParameterViewModel;
                if (bc == null) {
                    mslb.SelectedItems.Clear();
                    return;
                }
                switch (e.PropertyName) {
                    case nameof(bc.CurrentValue):
                    case nameof(bc.Selection):
                        if (bc.SelectedItems.Any(x => !mslb.SelectedItems.Contains(x))) {
                            mslb.SelectedItems = bc.Selection.SelectedItems.ToList();
                        }
                        break;
                }
            });
        }

        private void Mslb_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var mslb = this.FindControl<ListBox>("MultiSelectListBox");
            var tbl = mslb.GetVisualDescendants<TextBlock>();
            tbl
            .ForEach(x => x.Background = (x.DataContext as MpAvEnumerableParameterValueViewModel).IsSelected ? Brushes.Blue : Brushes.Transparent);


            //var bc = DataContext as MpAvMultiEnumerableParameterViewModel;
            //if (bc == null) {
            //    return;
            //}
            //bc.Selection.BeginBatchUpdate();
            //if (e.RemovedItems != null) {
            //    bc.Items.Where(x => e.RemovedItems.Contains(x)).ForEach((x, idx) => bc.Selection.Deselect(idx));
            //}
            //if (e.AddedItems != null) {
            //    bc.Items.Where(x => e.AddedItems.Contains(x)).ForEach((x, idx) => bc.Selection.Select(idx));
            //}
            //bc.Selection.EndBatchUpdate();
            //bc.Items.Where(x => e.RemovedItems.Contains(x)).ForEach(x => x.IsSelected = false);
            //bc.Items.Where(x => e.AddedItems.Contains(x)).ForEach(x => x.IsSelected = true);

        }
    }
}
