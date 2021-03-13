using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpPasteToAppPathViewModelCollection : MpObservableCollectionViewModel<MpPasteToAppPathViewModel> {
        private static readonly Lazy<MpPasteToAppPathViewModelCollection> _Lazy = new Lazy<MpPasteToAppPathViewModelCollection>(() => new MpPasteToAppPathViewModelCollection());
        public static MpPasteToAppPathViewModelCollection Instance { get { return _Lazy.Value; } }

        #region Private Variables

        #endregion

        #region View Models
        private MpObservableCollection<MpObservableCollection<MpPasteToAppPathViewModel>> _menuItemViewModels = null;
        public MpObservableCollection<MpObservableCollection<MpPasteToAppPathViewModel>> MenuItemViewModels {
            get {
                if(_menuItemViewModels == null) {
                    _menuItemViewModels = new MpObservableCollection<MpObservableCollection<MpPasteToAppPathViewModel>>();
                    foreach(var kvp in MpRunningApplicationManager.Instance.CurrentProcessWindowHandleStackDictionary) {
                        var appName = MpHelpers.Instance.GetProcessApplicationName(kvp.Value[0]);
                        if (kvp.Value.Count == 0 || string.IsNullOrEmpty(appName)) {
                            continue;
                        }
                        var processPath = kvp.Key;
                        var processHandles = new MpObservableCollection<MpPasteToAppPathViewModel>();
                        foreach(var handle in kvp.Value) {
                            processHandles.Add(
                                new MpPasteToAppPathViewModel(
                                    new MpPasteToAppPath(
                                        processPath,
                                        MpHelpers.Instance.GetProcessMainWindowTitle(handle),
                                        MpHelpers.Instance.IsProcessAdmin(handle)), 
                                    handle));
                        }
                        //check already created menu items and add handles to AppName if it already exists
                        int mivmIdx = -1;
                        foreach(var mivm in _menuItemViewModels) {
                            if(mivm.Count == 0) {
                                continue;
                            }
                            if(appName.ToLower() == MpHelpers.Instance.GetProcessApplicationName(mivm[0].Handle).ToLower()) {
                                mivmIdx = _menuItemViewModels.IndexOf(mivm);
                            }
                        }
                        if(mivmIdx >= 0) {
                            foreach(var ph in processHandles) {
                                _menuItemViewModels[mivmIdx].Add(ph);
                            }
                        } else {
                            _menuItemViewModels.Add(processHandles);
                        }
                    }
                    foreach(var ptapvm in this) {
                        _menuItemViewModels.Add(new MpObservableCollection<MpPasteToAppPathViewModel>() { ptapvm });
                    }
                }
                return _menuItemViewModels;
            }
        }
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
            MpRunningApplicationManager.Instance.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(MpRunningApplicationManager.Instance.CurrentProcessWindowHandleStackDictionary):
                        _menuItemViewModels = null;
                        OnPropertyChanged(nameof(MenuItemViewModels));
                        break;
                }
            };

            foreach(var ptap in MpPasteToAppPath.GetAllPasteToAppPaths()) {
                this.Add(new MpPasteToAppPathViewModel(ptap));
            }
        }
        public void PasteToAppPathDataGrid_Loaded(object sender, RoutedEventArgs args) {
            var dg = (DataGrid)sender;
            dg.SelectionChanged += (s, e) => {
                foreach(var ptapvm in this) {
                    ptapvm.IsSelected = ptapvm == SelectedPasteToAppPathViewModel ? true: false;
                }
            };
        }

        public ContextMenu UpdatePasteToMenuItem(ContextMenu cm) {
            MenuItem ptamir = null;
            foreach (MenuItem mi in cm.Items) {
                if (mi.Name == "PasteToAppPathMenuItem") {
                    ptamir = mi;
                }
            }
            if (ptamir == null) {
                return cm;
            }
            ptamir.Items.Clear();
            bool addedSeperator = false;
            foreach (var ptamivmc in MpPasteToAppPathViewModelCollection.Instance.MenuItemViewModels) {
                if (ptamivmc.Count == 0) {
                    continue;
                }
                if (ptamivmc[0].IsRuntime) {
                    bool areAllHidden = true;
                    foreach (var ptamivm in ptamivmc) {
                        if (!ptamivm.IsHidden) {
                            areAllHidden = false;
                        }
                    }
                    if (areAllHidden) {
                        continue;
                    }
                    var ptamip = new MenuItem();
                    ptamip.Header = MpHelpers.Instance.GetProcessApplicationName(ptamivmc[0].Handle);
                    ptamip.Icon = new Image() { Source = ptamivmc[0].AppIcon };
                    foreach (var ptamivm in ptamivmc) {
                        if (ptamivm.IsHidden) {
                            continue;
                        }
                        var ptami = new MenuItem();
                        var l = new Label();
                        l.Content = MpHelpers.Instance.GetProcessMainWindowTitle(ptamivm.Handle) + (ptamivm.IsAdmin ? " (Admin)" : string.Empty);

                        var eyeOpenImg = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/eye.png")) };
                        var eyeClosedImg = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/eye_closed.png")) };
                        var btn = new Button() { Cursor = Cursors.Hand, Content = eyeOpenImg, BorderThickness = new Thickness(0), Background = Brushes.Transparent, Width = 20, Height = 20, HorizontalAlignment = HorizontalAlignment.Right/*, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center*/ };
                        bool isOverButton = false;
                        btn.MouseEnter += (s, e2) => {
                            btn.Content = eyeClosedImg;
                            isOverButton = true;
                        };
                        btn.MouseLeave += (s, e2) => {
                            btn.Content = eyeOpenImg;
                            isOverButton = false;
                        };
                        btn.Click += (s, e2) => {
                            ptamivm.IsHidden = true;
                            ptamip.Items.Remove(ptami);
                            if (ptamip.Items.Count == 0) {
                                ptamir.Items.Remove(ptamip);
                            }
                        };

                        var sp = new StackPanel() { Orientation = Orientation.Horizontal };
                        sp.Children.Add(l);
                        sp.Children.Add(btn);

                        ptami.Header = sp;
                        ptami.Icon = new Image() { Source = ptamivm.AppIcon };
                        //ptami.Command = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
                        //ptami.CommandParameter = ptamivm.Handle;
                        ptami.Click += (s, e2) => {
                            if (!isOverButton) {
                                MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand.Execute(ptamivm.Handle);
                            }
                        };
                        ptamip.Items.Add(ptami);
                    }
                    ptamir.Items.Add(ptamip);
                } else {
                    if (!addedSeperator) {
                        ptamir.Items.Add(new Separator());
                        addedSeperator = true;
                    }
                    var ptaumi = new MenuItem();
                    ptaumi.Header = ptamivmc[0].AppName;// + (ptamivmc[0].IsAdmin ? " (Admin)" : string.Empty) + (ptamivmc[0].IsSilent ? " (Silent)" : string.Empty);
                    ptaumi.Icon = new Image() { Source = ptamivmc[0].AppIcon };
                    ptaumi.Command = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
                    ptaumi.CommandParameter = ptamivmc[0].PasteToAppPathId;

                    ptamir.Items.Add(ptaumi);
                }
            }
            var addNewMenuItem = new MenuItem();
            addNewMenuItem.Header = "Add Application...";
            addNewMenuItem.Icon = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Icons/Silk/icons/add.png")) };
            addNewMenuItem.Click += (s, e3) => {
                MainWindowViewModel.SystemTrayViewModel.ShowSettingsWindowCommand.Execute(1);
            };
            ptamir.Items.Add(addNewMenuItem);

            return cm;
        }
        public new void Add(MpPasteToAppPathViewModel ptapvm) {
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
            ValidationText = string.Empty;
            return true;
            //foreach (var ptapvm in this) {
            //    //clear validation before checking
            //    ptapvm.IsValid = true;
            //}
            //bool foundInvalid = false;
            //foreach (var ptapvm in this) {
            //    foreach (var optapvm in this) {
            //        if (optapvm != ptapvm && optapvm.AppPath == ptapvm.AppPath && optapvm.IsAdmin == ptapvm.IsAdmin) {
            //            optapvm.IsValid = false;
            //            ptapvm.IsValid = false;
            //            foundInvalid = true;
            //        }
            //    }
            //}
            //ValidationText = foundInvalid ? "Duplicate entries exist!" : string.Empty;

            //return string.IsNullOrEmpty(ValidationText);
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
                Title = "Select application path",                
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
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
