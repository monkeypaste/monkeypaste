using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvSettingsFrameCollectionView : MpAvUserControl<object> {
        public MpAvSettingsFrameCollectionView() {
            InitializeComponent();
        }
    }
}
