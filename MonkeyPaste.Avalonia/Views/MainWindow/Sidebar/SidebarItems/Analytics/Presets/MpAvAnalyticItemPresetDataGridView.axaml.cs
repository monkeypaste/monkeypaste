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
using System.Diagnostics;
using System.Threading.Tasks;

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
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
            BindingContext.Items.CollectionChanged += Items_CollectionChanged;
            RefreshDataGrid();
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(BindingContext.IsSelected):
                    RefreshDataGrid();
                    break;
                case nameof(BindingContext.HasModelChanged):
                    if(!BindingContext.HasModelChanged) {
                        RefreshDataGrid();
                    }
                    break;
            }
        }

        private async void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            // wait for view to catchup ?
            await Task.Delay(300);
            RefreshDataGrid();
        }

        private void RefreshDataGrid() {
            // BUG can't get dataGrid to resize w/ row changes so hardsetting height (RowHeight=40)
            var pdg = this.FindControl<DataGrid>("PresetDataGrid");
            if (pdg == null) {
                return;
            }
            pdg.ApplyTemplate();
            double nh = 0;
            if(BindingContext != null) {
                nh = BindingContext.Items.Count * pdg.RowHeight;
            }
            pdg.Height = nh;
            pdg.InvalidateMeasure();
            var sv = pdg.GetVisualDescendant<ScrollViewer>();
            if (sv == null) {
                //Debugger.Break();
                return;
            }
            sv.ScrollByPointDelta(new MpPoint(0, 5));
            sv.ScrollByPointDelta(new MpPoint(0, -5));
        }


    }
}
