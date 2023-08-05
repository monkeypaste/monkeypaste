using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using PropertyChanged;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvToolTipInfoHintView : MpAvUserControl<object> {
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
            AvaloniaXamlLoader.Load(this);
            this.Classes.CollectionChanged += Classes_CollectionChanged;
            this.AttachedToVisualTree += MpAvToolTipInfoHintView_AttachedToVisualTree;
        }

        private void MpAvToolTipInfoHintView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            Init();
        }

        private void Classes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            Init();
        }

        private void Init() {
            if (ToolTipText == null) {
                return;
            }
            if (ToolTipText.StartsWith("#warn#")) {
                ToolTipText = ToolTipText.Replace("#warn#", string.Empty);
                this.Classes.Add("warning");
            }
            if (ToolTipText.StartsWith("#error#")) {
                ToolTipText = ToolTipText.Replace("#error#", string.Empty);
                this.Classes.Add("error");
            }
            if (ToolTip.GetTip(this) is ToolTip tt) {
                tt.Classes.AddRange(this.Classes.Where(x => x == "warning" || x == "info" || x == "error"));
            }
        }
    }
}
