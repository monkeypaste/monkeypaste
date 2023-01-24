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
        #region CanShowSaveOrCancel Property

        private bool _CanShowSaveOrCancel = default;

        public static readonly DirectProperty<MpAvPluginParameterListBoxView, bool> CanShowSaveOrCancelProperty =
            AvaloniaProperty.RegisterDirect<MpAvPluginParameterListBoxView, bool>
            (
                nameof(CanShowSaveOrCancel),
                o => o.CanShowSaveOrCancel,
                (o, v) => o.CanShowSaveOrCancel = v
            );

        public bool CanShowSaveOrCancel {
            get => _CanShowSaveOrCancel;
            set {
                SetAndRaise(CanShowSaveOrCancelProperty, ref _CanShowSaveOrCancel, value);
            }
        }

        #endregion 

        public MpAvPluginParameterListBoxView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }        
    }
}
