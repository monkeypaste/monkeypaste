using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvExternalDropView : MpAvUserControl<MpAvExternalDropWindowViewModel> {
        #region Private Variables
        #endregion

        #region Constructors

        public MpAvExternalDropView() : base() {
            InitializeComponent();

            var dilb = this.FindControl<ListBox>("DropItemListBox");
            dilb.EnableItemsControlAutoScroll();
        }
        #endregion
    }
}
