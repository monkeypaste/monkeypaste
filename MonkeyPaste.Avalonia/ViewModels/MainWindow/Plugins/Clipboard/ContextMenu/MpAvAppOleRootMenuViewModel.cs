using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleRootMenuViewModel : MpAvAppOleMenuViewModelBase {

        #region Properties

        #region Overrides

        public override MpIAsyncCommand<object> CheckCommand =>
            null;
        public override MpMenuItemType MenuItemType => MpMenuItemType.Default;

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

            List<MpAvIMenuItemViewModel> items = null;
            if (show_type == "full") {
                // currently only shown from clip paste bar
                items = new List<MpAvIMenuItemViewModel> {
                    new MpAvAppOleReaderOrWriterMenuViewModel(this, true),
                    new MpAvAppOleReaderOrWriterMenuViewModel(this, false)
                };
                var manage_mivm = new MpAvMenuItemViewModel() {
                    Header = UiStrings.CommonManageHeader,
                    IconResourceKey = "CogColorImage",
                    HasLeadingSeparator = true
                };
                if (MenuArg is MpAvAppViewModel avm) {
                    // app has custom formats (show copy/paste stettings tab w/ this app selected)
                    manage_mivm.Command = MpAvSettingsViewModel.Instance.ShowSettingsWindowCommand;
                    manage_mivm.CommandParameter = new object[] {
                        MpSettingsTabType.CopyAndPaste,
                        avm.ToProcessInfo().SerializeObject() };
                } else {
                    // app has default formats (show clipboard sidebar)
                    manage_mivm.Command = MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand;
                    manage_mivm.CommandParameter = MpAvClipboardHandlerCollectionViewModel.Instance;
                }
                items.Add(manage_mivm);
            } else if (show_type == "read") {
                items = new List<MpAvIMenuItemViewModel> {
                    new MpAvAppOleReaderOrWriterMenuViewModel(this, true)
                };
            } else if (show_type == "write") {
                items = new List<MpAvIMenuItemViewModel> {
                    new MpAvAppOleReaderOrWriterMenuViewModel(this, false)
                };
            }


            SubItems = items;
        }

        #endregion

        #region Public Methods

        public MpAvAppOlePresetMenuViewModel GetMenuPresetByPresetId(int presetId) {
            foreach (var opvm in SubItems.Cast<MpAvAppOleReaderOrWriterMenuViewModel>()) {
                if (opvm.Presets.FirstOrDefault(y => y.ClipboardPresetViewModel.PresetId == presetId)
                    is MpAvAppOlePresetMenuViewModel pvm) {
                    return pvm;
                }
            }
            return null;
        }
        #endregion
    }
}
