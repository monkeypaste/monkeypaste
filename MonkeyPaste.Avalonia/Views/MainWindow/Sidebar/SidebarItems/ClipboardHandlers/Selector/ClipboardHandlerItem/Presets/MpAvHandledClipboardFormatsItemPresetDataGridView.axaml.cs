using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvHandledClipboardFormatsItemPresetDataGridView : MpAvUserControl<MpAvClipboardHandlerItemViewModel> {
        public MpAvHandledClipboardFormatsItemPresetDataGridView() {
            AvaloniaXamlLoader.Load(this);
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
            BindingContext.Items.ForEach(x => x.Items.CollectionChanged += Items_CollectionChanged);
            BindingContext.Items.ForEach(x => x.PropertyChanged += PresetVIewModel_PropertyChanged);
            RefreshDataGrid();
        }

        private void PresetVIewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var aipvm = sender as MpAvClipboardHandlerItemViewModel;
            if (aipvm == null) {
                return;
            }
            switch (e.PropertyName) {
                case nameof(aipvm.PluginIconId):
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
                e.NewItems.Cast<MpAvViewModelBase>().Where(x => x != null).ForEach(x => x.PropertyChanged += PresetVIewModel_PropertyChanged);
            }
            //if(e.OldItems != null) {
            //    e.OldItems.Cast<MpViewModelBase>().Where(x => x != null).ForEach(x => x.PropertyChanged -= PresetVIewModel_PropertyChanged);
            //}
            RefreshDataGrid();
        }

        private void RefreshDataGrid() {
            Dispatcher.UIThread.Post(() => {
                // BUG can't get dataGrid to resize w/ row changes so hardsetting height (RowHeight=40)
                var pdg = this.FindControl<DataGrid>("ClipboardFormatPresetDatagrid");
                if (pdg == null) {
                    return;
                }
                pdg.ApplyTemplate();
                double nh = 0;
                if (BindingContext != null &&
                    BindingContext.SelectedItem != null) {
                    nh = BindingContext.SelectedItem.Items.Count * pdg.RowHeight;
                }
                pdg.Height = nh;
                pdg.InvalidateMeasure();
                var sv = pdg.GetVisualDescendant<ScrollViewer>();
                if (sv == null) {
                    //MpDebug.Break();
                    return;
                }
                sv.ScrollByPointDelta(new MpPoint(0, 5));
                sv.ScrollByPointDelta(new MpPoint(0, -5));
            });
        }
    }
}
