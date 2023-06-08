using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvContextMenuView : ContextMenu {
        #region Overrides
        protected override Type StyleKeyOverride => typeof(ContextMenu);
        #endregion

        private static MpAvContextMenuView _instance;
        public static MpAvContextMenuView Instance => _instance ?? (_instance = new MpAvContextMenuView());


        public MpAvContextMenuView() : base() {
            AvaloniaXamlLoader.Load(this);
            this.Initialized += MpAvContextMenuView_Initialized;
        }

        private void MpAvContextMenuView_Initialized(object sender, EventArgs e) {
            if (this.VisualRoot is PopupRoot pr) {
                pr.AttachDevTools();
            }
        }

    }
}
