using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpPasteToAppPathViewModelCollection : MpObservableCollectionViewModel<MpPasteToAppPathViewModel> {
        private static readonly Lazy<MpPasteToAppPathViewModelCollection> _Lazy = new Lazy<MpPasteToAppPathViewModelCollection>(() => new MpPasteToAppPathViewModelCollection());
        public static MpPasteToAppPathViewModelCollection Instance { get { return _Lazy.Value; } }

        #region Private Variables

        #endregion

        #region Properties
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel;
        public MpPasteToAppPathViewModel SelectedPasteToAppPathViewModel {
            get {
                return _selectedPasteToAppPathViewModel;
            }
            set {
                if (_selectedPasteToAppPathViewModel != value) {
                    _selectedPasteToAppPathViewModel = value;
                    OnPropertyChanged(nameof(SelectedPasteToAppPathViewModel));
                }
            }
        }

        private string _validationText = string.Empty;
        private string ValidationText {
            get {
                return _validationText;
            }
            set {
                if (_validationText != value) {
                    _validationText = value;
                    OnPropertyChanged(nameof(ValidationText));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpPasteToAppPathViewModelCollection() {
            foreach(var ptap in MpPasteToAppPath.GetAllPasteToAppPaths()) {
                this.Add(new MpPasteToAppPathViewModel(ptap));
            }
        }

        public new void Add(MpPasteToAppPathViewModel ptapvm) {
            //if(this.Contains(ptapvm)) {
            //    return;
            //}
            base.Add(ptapvm);
        }
        
        public new void Remove(MpPasteToAppPathViewModel ptapvm) {
            if(this.Contains(ptapvm)) {
                base.Remove(ptapvm);
                ptapvm.Dispose();
            }
        }
        #endregion

        #region Private Methods
        private bool Validate() {
            foreach (var ptapvm in this) {
                //clear validation before checking
                ptapvm.IsValid = true;
            }
            bool foundInvalid = false;
            foreach (var ptapvm in this) {
                foreach (var optapvm in this) {
                    if (optapvm != ptapvm && optapvm.AppPath == ptapvm.AppPath && optapvm.IsAdmin == ptapvm.IsAdmin) {
                        optapvm.IsValid = false;
                        ptapvm.IsValid = false;
                        foundInvalid = true;
                    }
                }
            }
            ValidationText = foundInvalid ? "Duplicate entries exist!" : string.Empty;

            return string.IsNullOrEmpty(ValidationText);
        }
        #endregion

        #region Commands
        private RelayCommand _deletePasteToAppPathCommand;
        public ICommand DeletePasteToAppPathCommand {
            get {
                if (_deletePasteToAppPathCommand == null) {
                    _deletePasteToAppPathCommand = new RelayCommand(DeletePasteToAppPath);
                }
                return _deletePasteToAppPathCommand;
            }
        }
        private void DeletePasteToAppPath() {
            this.Remove(SelectedPasteToAppPathViewModel);
        }

        private RelayCommand _addPasteToAppPathCommand;
        public ICommand AddPasteToAppPathCommand {
            get {
                if (_addPasteToAppPathCommand == null) {
                    _addPasteToAppPathCommand = new RelayCommand(AddPasteToAppPath);
                }
                return _addPasteToAppPathCommand;
            }
        }
        private void AddPasteToAppPath() {
            var openFileDialog = new OpenFileDialog() {
                Filter = "Applications|*.lnk;*.exe",
                Title = "Select application path"
            };
            bool? openResult = openFileDialog.ShowDialog();
            if (openResult != null && openResult.Value) {
                string terminalPath = openFileDialog.FileName;
                if (Path.GetExtension(openFileDialog.FileName).Contains("lnk")) {
                    terminalPath = MpHelpers.Instance.GetShortcutTargetPath(openFileDialog.FileName);
                }
                var nptapvm = new MpPasteToAppPathViewModel(new MpPasteToAppPath(terminalPath, string.Empty, false));
                nptapvm.PasteToAppPath.WriteToDatabase();
                this.Add(nptapvm);

                Validate();
            }
        }
        #endregion
    }
}
