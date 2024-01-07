using MonkeyPaste.Common;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleFormatMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override MpIAsyncCommand<object> CheckCommand => new MpAsyncCommand<object>(
            async (args) => {
                // CASES:
                // false - the first plugins first preset will goto checked
                // true/null - all plugin formats will be toggled off

                if (IsChecked.IsFalse()) {
                    // format has no preset selected, select first in submenu
                    if (SubItems.FirstOrDefault() is MpAvAppOlePluginMenuViewModel pmvm &&
                        pmvm.SubItems.FirstOrDefault() is MpAvAppOlePresetMenuViewModel prmvm) {
                        await prmvm.CheckCommand.ExecuteAsync(this);
                    }
                } else {
                    // for either partial/checked deselect all
                    var presets_to_deselect =
                    SubItems
                    .OfType<MpAvAppOlePluginMenuViewModel>()
                    .SelectMany(x => x.SubItems)
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
            Format;

        public override object IconSourceObj {
            get {
                if (MpAvClipboardHandlerCollectionViewModel.Instance.FormatViewModels.FirstOrDefault(x => x.FormatName == Format)
                    is MpAvClipboardFormatViewModel cfvm) {
                    return cfvm.IconResourceObj;
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
