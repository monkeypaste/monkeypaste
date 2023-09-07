using MonkeyPaste.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOlePluginMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override ICommand Command => new MpAsyncCommand(
            async () => {
                await Task.WhenAll(SubItems
                .OfType<MpAvAppOlePresetMenuViewModel>()
                .Where(x => x.IsChecked.IsTrueOrNull())
                .Select(x => (x.Command as MpIAsyncCommand).ExecuteAsync()));

                RefreshChecks(true);
            },
            () => {
                return IsChecked.IsTrueOrNull();
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
