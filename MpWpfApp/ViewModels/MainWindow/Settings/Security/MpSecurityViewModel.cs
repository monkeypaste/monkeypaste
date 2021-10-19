using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSecurityViewModel : MpViewModelBase<MpSettingsWindowViewModel> {
        #region Private Variables

        #endregion

        #region View Models
        //public MpObservableCollection<MpAppViewModel> ExcludedAppViewModels {
        //    get {
        //        var cvs = CollectionViewSource.GetDefaultView(MpAppCollectionViewModel.Instance);
        //        cvs.Filter += item => {
        //            var avm = (MpAppViewModel)item;
        //            return avm.IsAppRejected;
        //        };
        //        var eavms = new ObservableCollection<MpAppViewModel>(cvs.Cast<MpAppViewModel>().ToList());
        //        //this adds empty row
        //        eavms.Add(new MpAppViewModel(null));
        //        return eavms;
        //    }
        //}
        public ObservableCollection<MpAppViewModel> AppViewModels {
            get {
                return MpAppCollectionViewModel.Instance.AppViewModels;
            }
        }
        #endregion

        #region Properties
        private int _selectedExcludedAppIndex;
        public int SelectedExcludedAppIndex {
            get {
                return _selectedExcludedAppIndex;
            }
            set {
                if (_selectedExcludedAppIndex != value) {
                    _selectedExcludedAppIndex = value;
                    OnPropertyChanged(nameof(SelectedExcludedAppIndex));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpSecurityViewModel() : base(null) { }

        public MpSecurityViewModel(MpSettingsWindowViewModel parent) : base(parent) {
            MpAppCollectionViewModel.Instance.Init();
        }

        public void AppCollectionDataGrid_Loaded(object sender, RoutedEventArgs args) {
            var dg = (DataGrid)sender;
            //add empty row
            AppViewModels.Add(new MpAppViewModel());
        }
        #endregion

        #region Commands
        private RelayCommand _deleteExcludedAppCommand;
        public ICommand DeleteExcludedAppCommand {
            get {
                if (_deleteExcludedAppCommand == null) {
                    _deleteExcludedAppCommand = new RelayCommand(DeleteExcludedApp);
                }
                return _deleteExcludedAppCommand;
            }
        }
        private void DeleteExcludedApp() {
            MonkeyPaste.MpConsole.WriteLine("Deleting excluded app row: " + SelectedExcludedAppIndex);
            var eavm = AppViewModels[SelectedExcludedAppIndex];
            AppViewModels[AppViewModels.IndexOf(eavm)].IsAppRejected = false;
            AppViewModels[AppViewModels.IndexOf(eavm)].App.WriteToDatabase();
            OnPropertyChanged(nameof(AppViewModels));
        }

        private RelayCommand _addExcludedAppCommand;
        public ICommand AddExcludedAppCommand {
            get {
                if (_addExcludedAppCommand == null) {
                    _addExcludedAppCommand = new RelayCommand(AddExcludedApp);
                }
                return _addExcludedAppCommand;
            }
        }
        private void AddExcludedApp() {
            MonkeyPaste.MpConsole.WriteLine("Add excluded app : ");
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                Filter = "Applications|*.lnk;*.exe",
                Title = "Select an application to exclude",                
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            bool? openResult = openFileDialog.ShowDialog();
            if (openResult != null && openResult.Value) {
                string appPath = openFileDialog.FileName;
                if (Path.GetExtension(openFileDialog.FileName).Contains("lnk")) {
                    appPath = MpHelpers.Instance.GetShortcutTargetPath(openFileDialog.FileName);
                }
                var neavm = MpAppCollectionViewModel.Instance.GetAppViewModelByProcessPath(appPath);
                if (neavm == null) {
                    //if unknown app just add it with rejection flag
                    neavm = new MpAppViewModel(MpAppCollectionViewModel.Instance,MpApp.Create(appPath,string.Empty,null));
                    MpAppCollectionViewModel.Instance.Add(neavm);
                } else if (neavm.IsAppRejected) {
                    //if app is already rejected set it to selected in grid
                    MessageBox.Show(neavm.AppName + " is already being rejected");
                    neavm.IsSelected = true;
                } else {
                    //otherwise update rejection and prompt about current clips
                    MpAppCollectionViewModel.Instance.UpdateRejection(neavm, true);
                }
                MpAppCollectionViewModel.Instance.Refresh();
            }
            //OnPropertyChanged(nameof(AppViewModels));
        }
        #endregion
    }
}
