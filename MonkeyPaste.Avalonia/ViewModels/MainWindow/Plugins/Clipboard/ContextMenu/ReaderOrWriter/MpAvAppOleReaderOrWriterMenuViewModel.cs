using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleReaderOrWriterMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides

        public override ICommand Command => new MpAsyncCommand<object>(
            async (args) => {

                MpAvAppViewModel avm = MenuArg as MpAvAppViewModel;
                if (avm == null &&
                    MenuArg is MpPortableProcessInfo pi) {
                    avm = await MpAvAppCollectionViewModel.Instance.AddOrGetAppByProcessInfoAsync(pi);
                }
                int no_op_id = IsReader ? MpAppOlePreset.NO_OP_READER_ID : MpAppOlePreset.NO_OP_WRITER_ID;
                if (IsChecked.IsFalse()) {
                    // must be no op

                    await avm.OleFormatInfos.RemoveAppOlePresetViewModelByPresetIdAsync(no_op_id);
                } else {
                    // check if custom
                    if (!avm.OleFormatInfos.IsDefault) {
                        // remove custom formats
                        var to_uncheck =
                            Presets
                            .Where(x => x.IsChecked.IsTrue())
                            .Distinct();

                        foreach (var pmvm in to_uncheck) {
                            if (pmvm.Command is MpAsyncCommand<object> acmd) {
                                await acmd.ExecuteAsync(false);
                            }
                        }
                    }
                    // add no op
                    await avm.OleFormatInfos.AddAppOlePresetViewModelByPresetIdAsync(no_op_id);
                }

                if (args == null) {
                    // was click source
                    RefreshChecks(true);
                }
            });

        public override string Header =>
            IsReader ? "Read" : "Write";

        public override object IconSourceObj =>
            IsReader ? "GlassesImage" : "PenImage";

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
