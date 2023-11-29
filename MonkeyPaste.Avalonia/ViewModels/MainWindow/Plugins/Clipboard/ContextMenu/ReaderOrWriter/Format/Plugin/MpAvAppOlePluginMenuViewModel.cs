using MonkeyPaste.Common;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOlePluginMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override MpIAsyncCommand<object> CheckCommand => new MpAsyncCommand<object>(
            async (args) => {
                // CASES:
                // false - all siblings are toggled off, then this plugins first preset will goto checked
                // true/null - all formats will be toggled off

                if (IsChecked.IsFalse()) {
                    // plugin has no select presets for this format

                    // deselect any selected sibling presets 
                    var parent_sib_presets_to_deselect =
                    (ParentObj as MpAvAppOleFormatMenuViewModel)
                    .SubItems
                    .Where(x => x != this)
                    .OfType<MpAvAppOlePluginMenuViewModel>()
                    .SelectMany(x => x.SubItems)
                    .OfType<MpAvAppOlePresetMenuViewModel>()
                    .Where(x => x.IsChecked.IsTrue());

                    foreach (var to_deselect in parent_sib_presets_to_deselect) {
                        await to_deselect.CheckCommand.ExecuteAsync(this);
                    }

                    //then select first child preset
                    await SubItems
                    .OfType<MpAvAppOlePresetMenuViewModel>()
                    .FirstOrDefault().CheckCommand.ExecuteAsync(this);
                } else {
                    // for true/null partial/checked deselect all
                    var presets_to_deselect =
                    SubItems
                    .OfType<MpAvAppOlePresetMenuViewModel>()
                    .Where(x => x.IsChecked.IsTrue());

                    foreach (var to_deselect in presets_to_deselect) {
                        await to_deselect.CheckCommand.ExecuteAsync(this);
                    }
                }

                if (args == null) {
                    // was click source
                    RefreshChecks(true);
                }
            });

        public override string Header =>
            ClipboardPluginViewModel.HandlerName;

        public override object IconSourceObj =>
            ClipboardPluginViewModel.PluginIconId;
        #endregion


        public MpAvClipboardHandlerItemViewModel ClipboardPluginViewModel { get; set; }


        #region Constructors
        public MpAvAppOlePluginMenuViewModel() : this(null, null) { }
        public MpAvAppOlePluginMenuViewModel(MpAvAppOleFormatMenuViewModel parent, MpAvClipboardHandlerItemViewModel clipboardPluginViewModel) : base(parent) {
            ClipboardPluginViewModel = clipboardPluginViewModel;

            string format = parent.Format;
            bool isReader = (parent.ParentObj as MpAvAppOleReaderOrWriterMenuViewModel).IsReader;

            var handlers =
                isReader ?
                    ClipboardPluginViewModel.Readers :
                    ClipboardPluginViewModel.Writers;
            var format_handler = handlers
                .FirstOrDefault(x => x.HandledFormat == format);

            var items =
                format_handler
                .Items
                .Select(x => new MpAvAppOlePresetMenuViewModel(this, x))
                .OrderBy(x => x.Header)
                .Cast<MpAvIMenuItemViewModel>()
                .ToList();

            items.Add(new MpAvMenuItemViewModel(this) {
                HasLeadingSeparator = items.Any(),
                Header = UiStrings.CommonManageHeader,
                IconSourceObj = "CogImage",
                Command = format_handler.ManageClipboardHandlerCommand
            });
            SubItems = items;
        }
        #endregion

    }
}
