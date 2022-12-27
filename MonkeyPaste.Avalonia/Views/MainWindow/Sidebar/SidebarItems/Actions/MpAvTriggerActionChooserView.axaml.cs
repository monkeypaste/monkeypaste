

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using MonoMac.ImageKit;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvTriggerActionChooserView : MpAvUserControl<MpAvTriggerCollectionViewModel> {
        public MpAvTriggerActionChooserView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
