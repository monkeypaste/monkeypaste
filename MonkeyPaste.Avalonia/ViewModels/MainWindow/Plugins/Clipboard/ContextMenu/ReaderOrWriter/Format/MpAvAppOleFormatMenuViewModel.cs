using MonkeyPaste.Common;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleFormatMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override ICommand Command => new MpAsyncCommand<object>(
            async (args) => {

                if (IsChecked.IsFalse()) {
                    // format has no preset selected, select first in submenu
                    if (SubItems.FirstOrDefault() is MpAvAppOlePluginMenuViewModel pmvm &&
                        pmvm.SubItems.FirstOrDefault() is MpAvAppOlePresetMenuViewModel prmvm) {
                        prmvm.Command.Execute(null);
                    }
                    return;
                }
                // for either partial/checked deselect all
                SubItems
                .OfType<MpAvAppOlePluginMenuViewModel>()
                .Select(x => x.SubItems)
                .OfType<MpAvAppOlePresetMenuViewModel>()
                .Where(x => x.IsChecked.IsTrue())
                .ForEach(x => x.Command.Execute(null));

                if (args == null) {
                    // was click source
                    RefreshChecks(true);
                }
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
                .Where(x => x.IsReader == parent.IsReader && x.ClipboardFormat.formatName == Format);
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
