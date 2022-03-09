using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpFileChooserParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Private Variables

        private string _defaultValue;

        #endregion

        #region Properties

        #region Model

        public bool IsDirectoryChooser {
            get {
                if(Parameter == null) {
                    return false;
                }
                return Parameter.parameterControlType == MpAnalyticItemParameterControlType.DirectoryChooser;
            }
        }
        public override string DefaultValue => _defaultValue;

        #endregion

        #endregion

        #region Constructors

        public MpFileChooserParameterViewModel() : base(null) { }

        public MpFileChooserParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aipf,MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            Parameter = aipf;
            ParameterValue = aipv;

            CurrentValue = _defaultValue = aipv.Value;

            OnPropertyChanged(nameof(DefaultValue));
            OnPropertyChanged(nameof(BoolValue));

            await Task.Delay(1);

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
