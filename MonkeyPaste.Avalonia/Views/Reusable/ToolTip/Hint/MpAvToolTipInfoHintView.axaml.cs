using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvToolTipInfoHintView : UserControl {
        #region Private Variables

        #endregion

        #region Statics

        static MpAvToolTipInfoHintView() {
            //ToolTipTextProperty.Changed.AddClassHandler<Control>((s, e) => {
            //    if (s is MpAvToolTipInfoHintView ttihv) {
            //        if (e.NewValue is string text && !string.IsNullOrEmpty(text)) {
            //            ttihv.IsVisible = true;
            //            var ttv = new MpAvToolTipView();
            //            ttv.ToolTipText = text;
            //            ToolTip.SetTip(ttihv.FindControl<Control>("HintContainerGrid"), ttv);
            //        } else {
            //            ttihv.IsVisible = false;
            //        }
            //    }
            //});
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
        #region ToolTipHtml Property

        private string _ToolTipHtml = string.Empty;

        public static readonly DirectProperty<MpAvToolTipView, string> ToolTipHtmlProperty =
            AvaloniaProperty.RegisterDirect<MpAvToolTipView, string>
            (
                nameof(ToolTipHtml),
                o => o.ToolTipHtml,
                (o, v) => o.ToolTipHtml = v,
                string.Empty
            );

        public string ToolTipHtml {
            get => _ToolTipHtml;
            set {
                SetAndRaise(ToolTipHtmlProperty, ref _ToolTipHtml, value);
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