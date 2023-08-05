using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvFileChooserParameterViewModel : MpAvParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

        #region Model

        public bool IsDirectoryChooser {
            get {
                if (ParameterFormat == null) {
                    return false;
                }
                return ParameterFormat.controlType == MpParameterControlType.DirectoryChooser;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvFileChooserParameterViewModel() : base(null) { }

        public MpAvFileChooserParameterViewModel(MpAvViewModelBase parent) : base(parent) { }

        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            if (CurrentValue.IsStringMayContainEnvVars()) {
                CurrentValue = CurrentValue.ExpandEnvVars();
            }

            IsBusy = false;
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands

        public ICommand SelectFileSystemPathCommand => new MpAsyncCommand(
            async () => {
                string initDir = CurrentValue;
                //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;

                string result = null;
                if (IsDirectoryChooser) {
                    result = await Mp.Services.NativePathDialog
                        .ShowFolderDialogAsync($"Select {Label}", initDir);
                } else {
                    result = await Mp.Services.NativePathDialog
                        .ShowFileDialogAsync($"Select {Label}", initDir, null);
                }

                if (!string.IsNullOrEmpty(result)) {
                    CurrentValue = result;
                }

                //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
            });

        #endregion
    }
}
