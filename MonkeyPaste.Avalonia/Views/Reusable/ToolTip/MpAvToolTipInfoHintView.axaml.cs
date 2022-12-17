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

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvToolTipInfoHintView : UserControl {
        #region Private Variables

        #endregion

        #region Statics

        static MpAvToolTipInfoHintView() {
            ToolTipTextProperty.Changed.AddClassHandler<Control>((s, e) => {
                if (s is MpAvToolTipInfoHintView ttihv) {
                    if (e.NewValue is string text && !string.IsNullOrEmpty(text)) {
                        ttihv.IsVisible = true;
                        var ttv = new MpAvToolTipView();
                        ttv.ToolTipText = text;
                        ToolTip.SetTip(ttihv.FindControl<Control>("HintContainerGrid"), ttv);
                    } else {
                        ttihv.IsVisible = false;
                    }
                }
            });
        }
        #endregion

        #region ToolTipText Direct Avalonia Property

        private string _ToolTipText = default;

        public static readonly DirectProperty<MpAvToolTipInfoHintView, string> ToolTipTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvToolTipInfoHintView, string>
            (
                nameof(ToolTipText),
                o => o.ToolTipText,
                (o, v) => o.ToolTipText = v
            );

        public string ToolTipText {
            get => _ToolTipText;
            set {
                SetAndRaise(ToolTipTextProperty, ref _ToolTipText, value);
            }
        }

        #endregion 
        public MpAvToolTipInfoHintView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
