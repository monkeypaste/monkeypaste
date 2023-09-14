using Avalonia.Controls;
using MonkeyPaste.Common;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvShortcutDataGridView : MpAvUserControl<object> {
        public MpAvShortcutDataGridView() {
            InitializeComponent();
        }

        private void Dg_Sorting(object sender, DataGridColumnEventArgs e) {
            var sccvm = MpAvShortcutCollectionViewModel.Instance;
            sccvm.OnPropertyChanged(nameof(sccvm.FilteredItems));
            sccvm.FilteredItems.ForEach(x => x.OnPropertyChanged(nameof(x.SelectedRoutingTypeIdx)));
        }
    }
}
