using MonkeyPaste.Common;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOlePluginMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override ICommand Command => new MpAsyncCommand<object>(
            async (args) => {
                if (IsChecked.IsFalse()) {
                    // plugin has no select presets for this format

                    // deselect any selected sibling presets 
                    (ParentObj as MpAvAppOleFormatMenuViewModel)
                    .SubItems
                    .Where(x => x != this)
                    .OfType<MpAvAppOlePluginMenuViewModel>()
                    .SelectMany(x => x.SubItems)
                    .OfType<MpAvAppOlePresetMenuViewModel>()
                    .Where(x => x.IsChecked.IsTrue())
                    .ForEach(x => x.Command.Execute(null));

                    //then select first child preset
                    SubItems
                    .OfType<MpAvAppOlePresetMenuViewModel>()
                    .FirstOrDefault().Command.Execute(null);

                    if (args == null) {
                        // was click source
                        RefreshChecks(true);
                    }
                    return;
                }

                // for true/null partial/checked deselect all
                SubItems
                .OfType<MpAvAppOlePresetMenuViewModel>()
                .Where(x => x.IsChecked.IsTrue())
                .ForEach(x => x.Command.Execute(null));
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
                Header = "Manage...",
                IconSourceObj = "CogImage",
                Command = format_handler.ManageClipboardHandlerCommand
            });
            SubItems = items;
        }
        #endregion

    }
}
