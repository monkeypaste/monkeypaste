using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSettingsFrameView : MpAvUserControl<MpAvSettingsFrameViewModel> {
        public static ObservableCollection<MpAvPluginParameterItemView> ParamViews { get; set; } = new ObservableCollection<MpAvPluginParameterItemView>();

        public MpAvSettingsFrameView() {
            InitializeComponent();

            var pplb = this
                .FindControl<MpAvParameterCollectionView>("ParameterCollectionView")
                .FindControl<ListBox>("PluginParameterListBox");
            if (pplb == null) {
                return;
            }

            pplb.ContainerPrepared += Pplb_ContainerPrepared;
            pplb.ContainerClearing += Pplb_ContainerClearing;
        }

        private void Pplb_ContainerClearing(object sender, ContainerClearingEventArgs e) {
            if (e.Container is not ListBoxItem lbi ||
                lbi.GetVisualDescendant<MpAvPluginParameterItemView>()
                    is not MpAvPluginParameterItemView ppiv) {
                return;
            }
            bool success = ParamViews.Remove(ppiv);
            if (ppiv.DataContext is MpAvParameterViewModelBase pvmb) {
                MpConsole.WriteLine($"param '{pvmb.ParamId}' removed. success: {success}");
            } else {

                MpConsole.WriteLine($"unknown param removed. success: {success}");
            }
        }

        private void Pplb_ContainerPrepared(object sender, ContainerPreparedEventArgs e) {
            if (e.Container is not ListBoxItem lbi) {
                return;
            }
            lbi.Loaded += Lbi_Loaded;
            if (lbi.IsLoaded) {
                Lbi_Loaded(lbi, null);
            }
        }

        private void Lbi_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not ListBoxItem lbi) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var ppiv = await lbi.GetVisualDescendantAsync<MpAvPluginParameterItemView>();
                if (ppiv == null) {
                    return;
                }
                ppiv.Loaded += Ppiv_Loaded;
                if (ppiv.IsLoaded) {
                    Ppiv_Loaded(ppiv, null);
                }
            });
        }

        private void Ppiv_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not MpAvPluginParameterItemView ppiv ||
                ParamViews.Contains(ppiv)) {
                return;
            }

            ParamViews.Add(ppiv);
            //if (ppiv.DataContext is MpAvParameterViewModelBase pvmb) {
            //    MpConsole.WriteLine($"param '{pvmb.ParamId}' added");
            //} else {
            //    MpConsole.WriteLine($"unknown param added");
            //}
        }
    }
}
