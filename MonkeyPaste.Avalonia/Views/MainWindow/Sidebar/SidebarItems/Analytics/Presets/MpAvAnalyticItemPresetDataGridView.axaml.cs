using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAnalyticItemPresetDataGridView : MpAvUserControl<MpAvAnalyticItemViewModel> {

        public MpAvAnalyticItemPresetDataGridView() {
            InitializeComponent();
            this.DataContextChanged += MpAvAnalyticItemPresetDataGridView_DataContextChanged;
            if (DataContext != null) {
                MpAvAnalyticItemPresetDataGridView_DataContextChanged(this, null);
            }
        }

        private void MpAvAnalyticItemPresetDataGridView_DataContextChanged(object sender, EventArgs e) {
            if (BindingContext == null) {
                return;
            }
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
            BindingContext.Items.CollectionChanged += Items_CollectionChanged;
            BindingContext.Items.ForEach(x => x.PropertyChanged += PresetVIewModel_PropertyChanged);
            RefreshDataGrid();
        }

        private void PresetVIewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var aipvm = sender as MpAvAnalyticItemPresetViewModel;
            switch (e.PropertyName) {
                case nameof(aipvm.IconId):
                    RefreshDataGrid();
                    break;
                case nameof(aipvm.IsSelected):
                    RefreshDataGrid();
                    break;
                case nameof(aipvm.HasModelChanged):
                    if (!aipvm.HasModelChanged) {
                        RefreshDataGrid();
                    }
                    break;
            }
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(BindingContext.SelectedItem):
                case nameof(BindingContext.IsSelected):
                    RefreshDataGrid();
                    break;
                case nameof(BindingContext.HasModelChanged):
                    if (!BindingContext.HasModelChanged) {
                        RefreshDataGrid();
                    }
                    break;
            }
        }

        private async void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            // wait for view to catchup ?
            await Task.Delay(300);
            if (e.NewItems != null) {
                e.NewItems.Cast<MpViewModelBase>().Where(x => x != null).ForEach(x => x.PropertyChanged += PresetVIewModel_PropertyChanged);
            }
            //if(e.OldItems != null) {
            //    e.OldItems.Cast<MpViewModelBase>().Where(x => x != null).ForEach(x => x.PropertyChanged -= PresetVIewModel_PropertyChanged);
            //}
            RefreshDataGrid();
        }

        private void RefreshDataGrid() {
            Dispatcher.UIThread.Post(() => {
                // BUG can't get dataGrid to resize w/ row changes so hardsetting height (RowHeight=40)
                var pdg = this.FindControl<DataGrid>("PresetDataGrid");
                if (pdg == null) {
                    return;
                }
                pdg.ApplyTemplate();
                double nh = 0;
                if (BindingContext != null) {
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
            });
        }


    }
}
