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

        public MpAvAppOleRootMenuViewModel(object menuArg) : base(null) {
            MenuArg = menuArg;

            SubItems = new List<MpAvIMenuItemViewModel> {
                new MpAvAppOleReaderOrWriterMenuViewModel(this, true),
                new MpAvAppOleReaderOrWriterMenuViewModel(this, false)
            };
        }
        #endregion
    }
}
