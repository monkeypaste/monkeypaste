using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSettingsFrameView : MpAvUserControl<MpAvSettingsFrameViewModel> {
        public static ObservableCollection<MpAvPluginParameterItemView> ParamViews { get; set; } = [];

        public MpAvSettingsFrameView() {
            InitializeComponent();

            this.ParameterCollectionView.ParamViews.CollectionChanged += ParamViews_CollectionChanged;
        }

        private void ParamViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if(e.NewItems != null &&
                e.NewItems.OfType<MpAvPluginParameterItemView>().Where(x=>!ParamViews.Contains(x)) is { } to_add_pivl) {
                ParamViews.AddRange(to_add_pivl);
            }
            if(e.OldItems != null &&
                e.OldItems.OfType<MpAvPluginParameterItemView>() is { } to_remove_pivl) {
                to_remove_pivl.ForEach(x => ParamViews.Remove(x));
            }
        }
    }
}
