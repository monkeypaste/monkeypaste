using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleReaderOrWriterMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override MpIAsyncCommand<object> CheckCommand => new MpAsyncCommand<object>(
            async (args) => {
                // CASES:
                // false - for every format, the first plugins first preset will goto checked
                // true/null - all formats will be toggled off

                // NOTE this level doesn't care about check state it always just
                // passes behavior to formats
                var formats =
                    SubItems
                    .OfType<MpAvAppOleFormatMenuViewModel>();
                foreach (var format in formats) {
                    await format.CheckCommand.ExecuteAsync(this);
                }
                if (args == null) {
                    // was click source
                    RefreshChecks(true);
                }
            });

        public override string Header =>
            IsReader ? UiStrings.CommonReadLabel : UiStrings.CommonWriteLabel;

        public override object IconSourceObj =>
            IsReader ? new object[] { MpSystemColors.cyan1, "GlassesImage" } : new object[] { MpSystemColors.orange1, "PenImage" };

        #endregion

        public List<MpAvAppOlePresetMenuViewModel> Presets { get; } = new();
        public bool IsReader { get; set; }

        #region Constructors
        public MpAvAppOleReaderOrWriterMenuViewModel() : this(null, false) { }
        public MpAvAppOleReaderOrWriterMenuViewModel(MpAvAppOleRootMenuViewModel parent, bool isReader) : base(parent) {
            IsReader = isReader;

            var presets = MpAvClipboardHandlerCollectionViewModel.Instance.AllPresets.Where(x => x.IsReader == IsReader);

            var formats = presets
                .Select(x => x.ClipboardFormat.formatName)
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
