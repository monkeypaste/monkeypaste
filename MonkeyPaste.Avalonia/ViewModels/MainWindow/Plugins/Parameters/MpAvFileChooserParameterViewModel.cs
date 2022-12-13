using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvFileChooserParameterViewModel : MpAvPluginParameterViewModelBase {
        #region Private Variables

        //private string _defaultValue;

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

        public MpAvFileChooserParameterViewModel() : base(null) { }

        public MpAvFileChooserParameterViewModel(MpIPluginComponentViewModel parent) : base(parent) { }

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

        public ICommand SelectFileSystemPathCommand => new MpAsyncCommand(
            async () => {
                string initDir = CurrentValue;
                if (string.IsNullOrEmpty(initDir)) {
                    initDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                } else if (initDir.IsFile()) {
                    initDir = Path.GetDirectoryName(initDir);
                }
                MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;


                if (IsDirectoryChooser) {
                    //var dlg = new MpFolderPicker() {
                    //    InputPath = initDir,
                    //    Title = "Select " + Label
                    //};
                    //bool result = dlg.ShowDialog() == true;
                    //if (result) {
                    //    CurrentValue = dlg.ResultPath;
                    //}

                    var selectedDir = await new OpenFolderDialog() {
                        Title = "Select " + Label,
                        Directory = initDir
                    }.ShowAsync(MpAvMainWindow.Instance);
                    if(!string.IsNullOrEmpty(selectedDir)) {
                        CurrentValue = selectedDir;
                    }
                } else {
                    var selectedFile = await new OpenFileDialog() {
                        Title = "Select " + Label,
                        Directory = initDir
                    }.ShowAsync(MpAvMainWindow.Instance);
                    if (selectedFile != null && selectedFile.Length == 1 && string.IsNullOrEmpty(selectedFile[0])) {
                        CurrentValue = selectedFile[0];
                    }
                }

                MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
            });

        #endregion
    }
}
