using System.Collections.Generic;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleRootMenuViewModel : MpAvAppOleMenuViewModelBase {

        #region Properties

        #region Overrides

        public override MpMenuItemType MenuItemType => MpMenuItemType.Default;
        public override ICommand Command => null;

        public override string Header =>
            "Formats";

        public override object IconSourceObj =>
            null;

        #endregion

        #region Model
        public new object MenuArg { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvAppOleRootMenuViewModel(object menuArg) : this(menuArg, "full") { }
        public MpAvAppOleRootMenuViewModel(object menuArg, string show_type) : base(null) {
            MenuArg = menuArg;

            if (show_type == "full") {
                SubItems = new List<MpAvIMenuItemViewModel> {
                    new MpAvAppOleReaderOrWriterMenuViewModel(this, true),
                    new MpAvAppOleReaderOrWriterMenuViewModel(this, false)
                };
            } else if (show_type == "read") {
                SubItems = new List<MpAvIMenuItemViewModel> {
                    new MpAvAppOleReaderOrWriterMenuViewModel(this, true)
                };
            } else if (show_type == "write") {
                SubItems = new List<MpAvIMenuItemViewModel> {
                    new MpAvAppOleReaderOrWriterMenuViewModel(this, false)
                };
            }
        }
        #endregion
    }
}
