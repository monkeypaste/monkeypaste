using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSettingsView : MpAvUserControl<MpAvSettingsViewModel> {
        private static MpAvSettingsView _instance;
        public static MpAvSettingsView Instance => _instance ?? (_instance = new MpAvSettingsView());
        public MpAvSettingsView() {
            MpDebug.Assert(_instance == null, "Singleton error");
            InitializeComponent();
        }
    }
}
