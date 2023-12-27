using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

        #region ToolTipText Property
        public string ToolTipText {
            get { return GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        public static readonly StyledProperty<string> ToolTipTextProperty =
            AvaloniaProperty.Register<MpAvToolTipInfoHintView, string>(
                name: nameof(ToolTipText),
                defaultValue: string.Empty);

        #endregion

        #region IsHtml Property
        public bool IsHtml {
            get { return GetValue(IsHtmlProperty); }
            set { SetValue(IsHtmlProperty, value); }
        }

        public static readonly StyledProperty<bool> IsHtmlProperty =
            AvaloniaProperty.Register<MpAvToolTipInfoHintView, bool>(
                name: nameof(IsHtml),
                defaultValue: false);

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
            if (ToolTip.GetTip(this) is not MpAvToolTipInfoHintView ttv) {
                return;
            }
            ttv.ToolTipText = ToolTipText;
            ttv.Classes.AddRange(this.Classes.Where(x => x == "warning" || x == "info" || x == "error"));
        }

        protected override void OnPointerEntered(PointerEventArgs e) {
            base.OnPointerEntered(e);

            if (ToolTip.GetTip(this) is not MpAvToolTipInfoHintView ttv) {
                return;
            }
            ttv.ToolTipText = ToolTipText;
            ToolTip.SetIsOpen(this, true);
        }
        protected override void OnPointerExited(PointerEventArgs e) {
            base.OnPointerExited(e);

            ToolTip.SetIsOpen(this, false);
        }
    }
}
