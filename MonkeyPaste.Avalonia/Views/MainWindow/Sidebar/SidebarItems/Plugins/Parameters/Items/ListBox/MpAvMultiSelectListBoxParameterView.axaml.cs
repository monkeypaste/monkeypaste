using Avalonia.Controls;
using System.Linq;
using MonkeyPaste.Common;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvMultiSelectListBoxParameterView : MpAvUserControl<MpAvEnumerableParameterViewModel> {
        public MpAvMultiSelectListBoxParameterView() {
            InitializeComponent();
            var mslb = this.FindControl<ListBox>("MultiSelectListBox");
            mslb.SelectionChanged += Mslb_SelectionChanged;
            this.DataContextChanged += MpAvMultiSelectListBoxParameterView_DataContextChanged;
            if(DataContext != null) {
                MpAvMultiSelectListBoxParameterView_DataContextChanged(mslb, null);
            }
        }

        private void MpAvMultiSelectListBoxParameterView_DataContextChanged(object sender, System.EventArgs e) {
            if(DataContext == null) {
                return;
            }
            if(DataContext is MpIViewModel vm) {
                vm.PropertyChanged += BindingContext_PropertyChanged;
            }
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            Dispatcher.UIThread.Post(() => {
                var mslb = this.FindControl<ListBox>("MultiSelectListBox");
                var bc = DataContext as MpAvEnumerableParameterViewModel;
                if (bc == null) {
                    mslb.SelectedItems.Clear();
                    return;
                }
                switch (e.PropertyName) {
                    case nameof(bc.SelectedItems):
                        if (bc.SelectedItems.Any(x => !mslb.SelectedItems.Contains(x))) {
                            mslb.SelectedItems = (System.Collections.IList)bc.SelectedItems;
                        }
                        break;
                }
            });
        }

        private void Mslb_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var mslb = this.FindControl<ListBox>("MultiSelectListBox");
            var bc = DataContext as MpAvEnumerableParameterViewModel;
            if (bc == null) {
                return;
            }
            bc.Items.Where(x => e.RemovedItems.Contains(x)).ForEach(x => x.IsSelected = false);
            bc.Items.Where(x => e.AddedItems.Contains(x)).ForEach(x => x.IsSelected = true);

        }
    }
}
