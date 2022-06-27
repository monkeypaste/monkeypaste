using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpFileChooserParameterViewModel : MpPluginParameterViewModelBase {
        #region Private Variables

        private string _defaultValue;

        #endregion

        #region Properties

        #region Model

        public bool IsDirectoryChooser {
            get {
                if(ParameterFormat == null) {
                    return false;
                }
                return ParameterFormat.controlType == MpPluginParameterControlType.DirectoryChooser;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpFileChooserParameterViewModel() : base(null) { }

        public MpFileChooserParameterViewModel(MpViewModelBase parent) : base(parent) { }

        public override async Task InitializeAsync(MpPluginPresetParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            if (CurrentValue.IsStringMayContainEnvVars()) {
                CurrentValue = CurrentValue.ExpandEnvVars();
            }

            IsBusy = false;
        }

        #endregion

        #region Commands

        public MpIAsyncCommand SelectFileSystemPathCommand => new MpAsyncCommand(
            async () => {
                string initDir = CurrentValue;
                if (string.IsNullOrEmpty(initDir)) {
                    initDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                } else if (initDir.IsFile()) {
                    initDir = Path.GetDirectoryName(initDir);
                }
                MpMainWindowViewModel.Instance.IsShowingDialog = true;

                if(IsDirectoryChooser) {
                    var dlg = new MpFolderPicker() {
                        InputPath = initDir,
                        Title = "Select " + Label
                    };
                    bool result = dlg.ShowDialog() == true;
                    if (result) {
                        CurrentValue = dlg.ResultPath;
                    }
                } else {
                    var openFileDialog = new Microsoft.Win32.OpenFileDialog() {
                        Title = "Select " + Label,
                        InitialDirectory = initDir
                    };
                    bool? openResult = openFileDialog.ShowDialog();
                    if (openResult != null && openResult.Value) {
                        CurrentValue = openFileDialog.FileName;
                    }
                }

                MpMainWindowViewModel.Instance.IsShowingDialog = false;
            });

        #endregion
    }
}
