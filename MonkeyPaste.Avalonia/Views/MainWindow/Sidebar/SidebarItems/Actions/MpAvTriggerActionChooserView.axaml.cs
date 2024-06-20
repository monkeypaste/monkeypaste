using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvTriggerActionChooserView : MpAvUserControl<MpAvTriggerCollectionViewModel>, MpAvISidebarContentView, MpAvIFocusHeaderMenuView {
        public MpAvTriggerActionChooserView() {
            InitializeComponent();
        }        
    }
}
