using MonkeyPaste.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleFormatMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override ICommand Command => new MpAsyncCommand(
            async () => {
                await Task.WhenAll(SubItems
                .OfType<MpAvAppOlePluginMenuViewModel>()
                .Where(x => x.IsChecked.IsTrueOrNull())
                .Select(x => (x.Command as MpIAsyncCommand).ExecuteAsync()));

                RefreshChecks(true);
            },
            () => {
                return IsChecked.IsTrueOrNull();
            });

        public override string Header =>
            Format;

        public override object IconSourceObj {
            get {
                if (MpAvClipboardHandlerCollectionViewModel.Instance.FormatViewModels.FirstOrDefault(x => x.FormatName == Format)
                    is MpAvClipboardFormatViewModel cfvm) {
                    return cfvm.IconResourceKeyStr;
                }
                return "QuestionMarkImage";
            }
        }

        #endregion

        public string Format { get; set; }

        #region Constructors
        public MpAvAppOleFormatMenuViewModel() : this(null, string.Empty) { }
        public MpAvAppOleFormatMenuViewModel(MpAvAppOleReaderOrWriterMenuViewModel parent, string format) : base(parent) {
            Format = format;
            var presets =
                MpAvClipboardHandlerCollectionViewModel.Instance.AllPresets
                .Where(x => x.IsReader == parent.IsReader && x.ClipboardFormat.clipboardName == Format);
            SubItems =
                presets
                .Select(x => x.Parent.Parent)
                .Distinct()
                .Select(x => new MpAvAppOlePluginMenuViewModel(this, x))
                .OrderBy(x => x.ClipboardPluginViewModel.HandlerName)
                .ToList();

        }
        #endregion

    }
}
