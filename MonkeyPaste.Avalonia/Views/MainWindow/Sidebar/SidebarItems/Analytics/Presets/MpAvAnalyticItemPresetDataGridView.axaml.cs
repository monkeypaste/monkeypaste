using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System;
using Avalonia.Input;
using Avalonia.Media;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAnalyticItemPresetDataGridView : MpAvUserControl<MpAvAnalyticItemViewModel> {

        public MpAvAnalyticItemPresetDataGridView() {
            InitializeComponent();
            this.DataContextChanged += MpAvAnalyticItemPresetDataGridView_DataContextChanged;
        }

        private void MpAvAnalyticItemPresetDataGridView_DataContextChanged(object sender, EventArgs e) {
            if(BindingContext == null) {
                return;
            }
            BindingContext.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            // BUG can't get dataGrid to resize w/ row changes so hardsetting height (RowHeight=40)
            var pdg = this.FindControl<DataGrid>("PresetDataGrid");
            if(pdg == null) {
                return;
            }
            pdg.Height = BindingContext.Items.Count * pdg.RowHeight;
            pdg.InvalidateMeasure();
        }

     


    }
}
