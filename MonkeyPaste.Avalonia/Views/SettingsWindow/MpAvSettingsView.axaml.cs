using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSettingsView : MpAvUserControl<MpAvSettingsViewModel> {
        public MpAvSettingsView() {
            InitializeComponent();
        }
    }
}
