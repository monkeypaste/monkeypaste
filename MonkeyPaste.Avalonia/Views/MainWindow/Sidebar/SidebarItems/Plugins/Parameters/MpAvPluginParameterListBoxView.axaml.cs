using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System;
using Avalonia.Input;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPluginParameterListBoxView : MpAvUserControl<MpAvIParameterCollectionViewModel> {

        public MpAvPluginParameterListBoxView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }        
    }
}
