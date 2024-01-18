using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PropertyChanged;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public enum MpTooltipHintType {
        Info,
        Warning,
        Error,
        Link
    }
    [DoNotNotify]
    public partial class MpAvToolTipHintView : UserControl {
        #region Constants
        public const string WARN_PREFIX = "#warning#";
        public const string ERROR_PREFIX = "#error#";
        public const string LINK_PREFIX = "#link#";
        public const string INFO_PREFIX = "#info#";

        static string[] _prefixes = [
            WARN_PREFIX,
            ERROR_PREFIX,
            LINK_PREFIX,
            INFO_PREFIX
        ];

        #endregion

        #region ToolTipText Property
        public string ToolTipText {
            get { return GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        public static readonly StyledProperty<string> ToolTipTextProperty =
            AvaloniaProperty.Register<MpAvToolTipHintView, string>(
                name: nameof(ToolTipText),
                defaultValue: string.Empty);

        #endregion

        #region IsHtml Property
        public bool IsHtml {
            get { return GetValue(IsHtmlProperty); }
            set { SetValue(IsHtmlProperty, value); }
        }

        public static readonly StyledProperty<bool> IsHtmlProperty =
            AvaloniaProperty.Register<MpAvToolTipHintView, bool>(
                name: nameof(IsHtml),
                defaultValue: false);

        #endregion


        #region Command Property
        public ICommand Command {
            get { return GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly StyledProperty<ICommand> CommandProperty =
            AvaloniaProperty.Register<MpAvToolTipHintView, ICommand>(
                name: nameof(Command),
                defaultValue: null);

        #endregion

        #region Command Property
        public object CommandParameter {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly StyledProperty<object> CommandParameterProperty =
            AvaloniaProperty.Register<MpAvToolTipHintView, object>(
                name: nameof(CommandParameter),
                defaultValue: null);

        #endregion 

        public MpAvToolTipHintView() {
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
            string prefix = INFO_PREFIX;
            if (_prefixes.FirstOrDefault(x => ToolTipText.StartsWith(x)) is string prefix_match) {
                prefix = prefix_match;
            }

            ToolTipText = ToolTipText.Replace(prefix, string.Empty);
            this.Classes.Add(prefix.Replace("#", string.Empty));
            if (ToolTip.GetTip(this) is not MpAvToolTipHintView ttv) {
                return;
            }
            ttv.ToolTipText = ToolTipText;
            ttv.Classes.AddRange(this.Classes.Where(x => _prefixes.Any(y => y.Contains(x))));
        }

        protected override void OnPointerEntered(PointerEventArgs e) {
            base.OnPointerEntered(e);

            if (ToolTip.GetTip(this) is not MpAvToolTipHintView ttv) {
                return;
            }
            ttv.ToolTipText = ToolTipText;
            ToolTip.SetIsOpen(this, true);
        }
        protected override void OnPointerExited(PointerEventArgs e) {
            base.OnPointerExited(e);

            ToolTip.SetIsOpen(this, false);
        }
        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            if (Command != null) {
                Command.Execute(CommandParameter);
            }
        }
    }
}
