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
    public partial class MpAvOptionsButton : UserControl {
        #region Overrides

        #endregion

        #region Properties

        #endregion
        public MpAvOptionsButton() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
