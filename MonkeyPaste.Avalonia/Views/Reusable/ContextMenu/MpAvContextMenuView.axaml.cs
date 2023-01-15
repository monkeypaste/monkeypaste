using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Styling;
using System;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
using Avalonia.Controls.Generators;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvContextMenuView : ContextMenu, IStyleable {
        Type IStyleable.StyleKey => typeof(ContextMenu);

        private static MpAvContextMenuView _instance;
        public static MpAvContextMenuView Instance => _instance ?? (_instance = new MpAvContextMenuView());
        

        public MpAvContextMenuView() {
            InitializeComponent();
            this.Initialized += MpAvContextMenuView_Initialized;
        }

        private void MpAvContextMenuView_Initialized(object sender, EventArgs e) {
            (this.VisualRoot as PopupRoot).AttachDevTools();
        }

        //public void CloseMenu() {
        //    if(IsInitialized && !IsShowingChildDialog) {
        //        IsOpen = false;
        //    }
        //}

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
    //[DoNotNotify]
    //public class MpAvMenuItem : MenuItem, IStyleable {
    //    //Type IStyleable.StyleKey => typeof(MenuItem);
    //}
}
