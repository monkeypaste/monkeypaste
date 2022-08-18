using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvFilterMenuView : MpAvUserControl<MpAvFilterMenuViewModel> {

        public MpAvFilterMenuView() {
            InitializeComponent();
            var ic = this.FindControl<ItemsControl>("MainWindowFilterGrid");
            ic.ItemContainerGenerator.Materialized += ItemContainerGenerator_Materialized;
        }

        private void ItemContainerGenerator_Materialized(object sender, global::Avalonia.Controls.Generators.ItemContainerEventArgs e) {
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
