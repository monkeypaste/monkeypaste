using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System;
using Avalonia.Input;

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

            var pdg = this.FindControl<DataGrid>("PresetDataGrid");
            //pdg.InvalidateAll();

        }

     


    }
}
