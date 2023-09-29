using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOlePresetMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides
        public override bool IsThreeState => false;
        public override bool? IsChecked {
            get {
                if (MenuArg is not MpAvAppViewModel avm) {
                    // stay lazy w/ unknown apps and reflect default state
                    return ClipboardPresetViewModel.IsEnabled;
                }
                bool is_default =
                    ClipboardPresetViewModel.IsReader ?
                        avm.OleFormatInfos.IsReaderDefault :
                        avm.OleFormatInfos.IsWriterDefault;
                if (is_default) {
                    //return true;

                    return ClipboardPresetViewModel.IsEnabled;
                }
                return avm.OleFormatInfos.IsFormatEnabledByPresetId(ClipboardPresetViewModel.PresetId);
            }
        }
        public override MpIAsyncCommand<object> CheckCommand => new MpAsyncCommand<object>(
            async (args) => {
                await ClipboardPresetViewModel.TogglePresetIsEnabledCommand.ExecuteAsync(MenuArg);
                if (args == null) {
                    // was click source
                    RefreshChecks(true);
                }
            });

        public override string Header =>
            ClipboardPresetViewModel.Label;

        public override object IconSourceObj =>
            ClipboardPresetViewModel.IconId;


        #endregion

        public MpAvClipboardFormatPresetViewModel ClipboardPresetViewModel { get; set; }

        #region Constructors
        public MpAvAppOlePresetMenuViewModel(MpAvAppOlePluginMenuViewModel parent, MpAvClipboardFormatPresetViewModel preset) : base(parent) {
            ClipboardPresetViewModel = preset;
            RelativeRoot.Presets.Add(this);
        }

        #endregion

        #region Private Methods


        #endregion

    }
}
