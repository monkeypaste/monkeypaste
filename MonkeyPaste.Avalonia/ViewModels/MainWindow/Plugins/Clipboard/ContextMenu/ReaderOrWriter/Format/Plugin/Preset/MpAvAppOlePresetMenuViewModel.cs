using MonkeyPaste.Common;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOlePresetMenuViewModel : MpAvAppOleMenuViewModelBase {
        #region Overrides
        public override bool IsThreeState => false;
        public override bool? IsChecked {
            get {
                if (MenuArg is not MpAvAppViewModel avm) {
                    return ClipboardPresetViewModel.IsEnabled;
                }
                return avm.OleFormatInfos.IsFormatEnabledByPreset(ClipboardPresetViewModel);
            }
        }
        public override ICommand Command => new MpAsyncCommand(
            async () => {
                await ClipboardPresetViewModel.TogglePresetIsEnabledCommand.ExecuteAsync(MenuArg);
                RefreshChecks(true);
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
        }
        #endregion

    }
}
