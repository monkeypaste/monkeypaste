using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvTooltipView : UserControl {
        #region ToolTipText Direct Avalonia Property

        private string _ToolTipText = default;

        public static readonly DirectProperty<MpAvTooltipView, string> ToolTipTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvTooltipView, string>
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

        #endregion ToolTipText Direct Avalonia Property  

        static MpAvTooltipView() {
            ToolTipTextProperty.Changed.AddClassHandler<Control>((s, e) => {
                if (s is MpAvTooltipView ttv) {
                    if (e.NewValue is string text && !string.IsNullOrEmpty(text)) {
                        ttv.IsVisible = true;
                        var tb = ttv.FindControl<TextBlock>("ToolTipTextBlock");
                        tb.Text = text;
                    } else {
                        ttv.IsVisible = false;
                    }
                }
            });
        }

        public MpAvTooltipView() {
            InitializeComponent();
        }

        private void UserControl_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var tooltip = this.FindAncestorOfType<ToolTip>();
            tooltip.BorderThickness = new Thickness(0);
            tooltip.Background = Brushes.Transparent;
            if (tooltip.Parent is Popup popup) {
                
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
