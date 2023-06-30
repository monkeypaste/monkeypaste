using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvHeaderedSeperator : Separator {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region HeaderText
        public string? HeaderText {
            get => GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }
        public static readonly StyledProperty<string?> HeaderTextProperty =
            AvaloniaProperty.Register<TextBlock, string?>(nameof(HeaderText));

        #endregion

        #endregion

        #region Constructors
        public MpAvHeaderedSeperator() : base() {
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            var test = VisualChildren;
            var test2 = this.GetLogicalDescendants();
            this.VisualChildren.Insert(0, new TextBlock() { Text = HeaderText });
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion


    }
}
