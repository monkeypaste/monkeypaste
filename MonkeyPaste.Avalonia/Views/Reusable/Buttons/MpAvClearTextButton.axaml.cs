using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using PropertyChanged;
using System.Diagnostics;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;
using Avalonia.Input;
using Avalonia.Styling;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvClearTextButton : Button, IStyleable {
        #region Overrides
        Type IStyleable.StyleKey => typeof(Button);

        #endregion
        public MpAvClearTextButton() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
