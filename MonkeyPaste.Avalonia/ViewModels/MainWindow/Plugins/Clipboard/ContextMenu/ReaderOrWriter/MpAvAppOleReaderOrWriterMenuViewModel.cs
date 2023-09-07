using MonkeyPaste.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleReaderOrWriterMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override ICommand Command => new MpAsyncCommand(
            async () => {
                await Task.WhenAll(SubItems
                .OfType<MpAvAppOleFormatMenuViewModel>()
                .Where(x => x.IsChecked.IsTrueOrNull())
                .Select(x => (x.Command as MpIAsyncCommand).ExecuteAsync()));

                RefreshChecks(true);
            },
            () => {
                return IsChecked.IsTrueOrNull();
            });

        public override string Header =>
            IsReader ? "Read" : "Write";

        public override object IconSourceObj =>
            IsReader ? "GlassesImage" : "PenImage";

        #endregion

        public bool IsReader { get; set; }

        #region Constructors
        public MpAvAppOleReaderOrWriterMenuViewModel() : this(null, false) { }
        public MpAvAppOleReaderOrWriterMenuViewModel(MpAvAppOleRootMenuViewModel parent, bool isReader) : base(parent) {
            IsReader = isReader;

            var presets = MpAvClipboardHandlerCollectionViewModel.Instance.AllPresets.Where(x => x.IsReader == IsReader);

            var formats = presets
                .Select(x => x.ClipboardFormat.clipboardName)
                .Distinct()
                .OrderBy(x => x);

            SubItems =
                formats
                .Select(x => new MpAvAppOleFormatMenuViewModel(this, x))
                .OrderBy(x => x.Format)
                .ToList();
        }
        #endregion

    }
}
