using MonkeyPaste.Common;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOlePluginMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override ICommand Command => new MpAsyncCommand<object>(
            async (args) => {
                if (IsChecked.IsTrueOrNull()) {
                    var to_uncheck =
                    SubItems
                    .OfType<MpAvAppOlePresetMenuViewModel>()
                    .Where(x => x.IsChecked.IsTrue());

                    foreach (var pvm in to_uncheck) {
                        await pvm.ClipboardPresetViewModel.TogglePresetIsEnabledCommand.ExecuteAsync(MenuArg);
                    }
                } else {
                    // when false enable first item
                    var to_check =
                        SubItems
                        .OfType<MpAvAppOlePresetMenuViewModel>()
                        .FirstOrDefault(x => x.ClipboardPresetViewModel.IsDefault);
                    MpDebug.Assert(to_check != null, $"Clipboard plugins not determining default right, maybe should fall back to first item?");

                    await to_check.ClipboardPresetViewModel.TogglePresetIsEnabledCommand.ExecuteAsync(MenuArg);
                }
                RefreshChecks(true);
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
                .OrderBy(x => x)
                .Cast<MpAvIMenuItemViewModel>()
                .ToList();

            items.Add(new MpAvMenuItemViewModel(this) {
                Header = "Manage...",
                IconSourceObj = "CogImage",
                Command = format_handler.ManageClipboardHandlerCommand
            });
            SubItems = items;
        }
        #endregion

    }
}
