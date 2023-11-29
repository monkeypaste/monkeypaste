using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common;
using PropertyChanged;
using System.Linq;

namespace MonkeyPaste.Avalonia {

    public enum MpTooltipHintType {
        Info,
        Warning,
        Error
    }
    [DoNotNotify]
    public partial class MpAvToolTipInfoHintView : UserControl {
        #region Constants
        public const string WARN_PREFIX = "#warn#";
        public const string ERROR_PREFIX = "#error#";

        #endregion

        #region ToolTipText Direct Avalonia Property

        private string _ToolTipText = default;

        public static readonly DirectProperty<MpAvToolTipInfoHintView, string> ToolTipTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvToolTipInfoHintView, string>
            (
                nameof(ToolTipText),
                o => o.ToolTipText,
                (o, v) => o.ToolTipText = v);

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
            this.Classes.CollectionChanged += Classes_CollectionChanged;
            this.Loaded += MpAvToolTipInfoHintView_Loaded;
        }

        private void MpAvToolTipInfoHintView_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            Init();
        }

        private void Classes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            Init();
        }

        private void Init() {
            if (ToolTipText == null) {
                return;
            }
            if (ToolTipText.StartsWith(WARN_PREFIX)) {
                ToolTipText = ToolTipText.Replace(WARN_PREFIX, string.Empty);
                this.Classes.Add("warning");
            }
            if (ToolTipText.StartsWith(ERROR_PREFIX)) {
                ToolTipText = ToolTipText.Replace(ERROR_PREFIX, string.Empty);
                this.Classes.Add("error");
            }
            if (ToolTip.GetTip(this) is not MpAvToolTipView ttv) {
                return;
            }
            ttv.ToolTipText = ToolTipText;
            ttv.ToolTipHtml = ToolTipHtml;
            ttv.Classes.AddRange(this.Classes.Where(x => x == "warning" || x == "info" || x == "error"));
        }

        protected override void OnPointerEntered(PointerEventArgs e) {
            base.OnPointerEntered(e);

            if (ToolTip.GetTip(this) is not MpAvToolTipView ttv) {
                return;
            }
            ttv.ToolTipText = ToolTipText;
            ttv.ToolTipHtml = ToolTipHtml;
            ToolTip.SetIsOpen(this, true);
        }
        protected override void OnPointerExited(PointerEventArgs e) {
            base.OnPointerExited(e);

            ToolTip.SetIsOpen(this, false);
        }
    }
}
