﻿using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MpWpfApp {
    // + Account
    //   - Email Address (tb)
    //   - Password (pb)
    //   - Remember (cb)
    //   - Register (link)
    //   - Log In/Out (b)
    //   - Devices (eg)
    //   - Sync (b)

    // + Preferences
    //   - load on login (cb)
    //   - Play sound notifiations (cb)
    //   - Show Copy Notifications (cb)
    //   - Show Append Change Notifications (cb)
    //   - Theme Selection (dd)

    // + Data
    //   - Excluded Apps (eg)
    //   - Import/Export Data (b)
    //   - Reset Data (b)
    //   - History Capacity (sb)
    //   - Clipboard data types (cb's)
    //   - Text recognition (msdb)
    //   - Always Paste as Plain Text (cb)
    //   - Db Statistics (labels)

    // + Shortcuts
    //   - Info box w/ tips & warnings
    //   - Hotkeys (eg)
    //   - Hotcorners (cc)

    // + Help
    //   - Keyboard shortcuts
    //   - Support
    //   - Send Feedback
    //   - About Monkey Paste

    public class MpSettingsWindowViewModel : MpViewModelBase {
        #region Private Variables
        private Window _windowRef;
        private int _tabToShow = 0;
        //private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsBackup = new ObservableCollection<MpShortcutViewModel>();
        //private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsToDelete = new ObservableCollection<MpShortcutViewModel>();
        //private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsToRegister = new ObservableCollection<MpShortcutViewModel>();
        #endregion

        #region Static Variables
        public static bool IsOpen = false;
        #endregion

        #region View Models
        public MpShortcutCollectionViewModel ShortcutCollectionViewModel {
            get {
                return MpShortcutCollectionViewModel.Instance;
            }
        }

        private MpPreferencesViewModel _preferencesViewModel = new MpPreferencesViewModel();
        public MpPreferencesViewModel PreferencesViewModel {
            get {
                return _preferencesViewModel;
            }
            set {
                if(_preferencesViewModel != value) {
                    _preferencesViewModel = value;
                    OnPropertyChanged(nameof(PreferencesViewModel));
                }
            }
        }

        private MpSecurityViewModel _securityViewModel = new MpSecurityViewModel();
        public MpSecurityViewModel SecurityViewModel {
            get {
                return _securityViewModel;
            }
            set {
                if (_securityViewModel != value) {
                    _securityViewModel = value;
                    OnPropertyChanged(nameof(SecurityViewModel));
                }
            }
        }
        #endregion

            #region Properties

            #region Panel Visibility
        private Visibility _settingsPanel1Visibility = Visibility.Collapsed;
        public Visibility SettingsPanel1Visibility {
            get { 
                return _settingsPanel1Visibility; 
            }
            set { 
                if(_settingsPanel1Visibility != value) {
                    _settingsPanel1Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel1Visibility));
                }
            }
        }

        private Visibility _settingsPanel2Visibility = Visibility.Collapsed;
        public Visibility SettingsPanel2Visibility {
            get {
                return _settingsPanel2Visibility;
            }
            set {
                if (_settingsPanel2Visibility != value) {
                    _settingsPanel2Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel2Visibility));
                }
            }
        }

        private Visibility _settingsPanel3Visibility = Visibility.Collapsed;
        public Visibility SettingsPanel3Visibility {
            get {
                return _settingsPanel3Visibility;
            }
            set {
                if (_settingsPanel3Visibility != value) {
                    _settingsPanel3Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel3Visibility));
                }
            }
        }

        private Visibility _settingsPanel4Visibility = Visibility.Collapsed;
        public Visibility SettingsPanel4Visibility {
            get {
                return _settingsPanel4Visibility;
            }
            set {
                if (_settingsPanel4Visibility != value) {
                    _settingsPanel4Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel4Visibility));
                }
            }
        }

        private Visibility _settingsPanel5Visibility = Visibility.Collapsed;
        public Visibility SettingsPanel5Visibility {
            get {
                return _settingsPanel5Visibility;
            }
            set {
                if (_settingsPanel5Visibility != value) {
                    _settingsPanel5Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel5Visibility));
                }
            }
        }
        #endregion        

        private int _selectedShortcutIndex;
        public int SelectedShortcutIndex {
            get {
                return _selectedShortcutIndex;
            }
            set {
                if (_selectedShortcutIndex != value) {
                    _selectedShortcutIndex = value;
                    OnPropertyChanged(nameof(SelectedShortcutIndex));
                }
            }
        }

        
        #endregion

        #region Static Methods
        
        #endregion

        #region Public Methods
        public MpSettingsWindowViewModel() : base() {
            PreferencesViewModel = new MpPreferencesViewModel();
            SecurityViewModel = new MpSecurityViewModel();
        }

        public MpSettingsWindowViewModel(int tabToShow) : this() {
            _tabToShow = tabToShow;
        }

        public void SettingsWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
            _windowRef.Closed += (s, e2) => {
                IsOpen = false;
            };
            IsOpen = true;
            ClickSettingsPanelCommand.Execute(_tabToShow);
            //var clonedList = MpShortcutCollectionViewModel.Instance.Select(x => (MpShortcutViewModel)x.Clone()).ToList();
            //_shortcutViewModelsBackup = new ObservableCollection<MpShortcutViewModel>(clonedList);

            
        }

        public bool ShowSettingsWindow(int tabToShow = 0) {
            var sw = new MpSettingsWindow(tabToShow);
            return sw.ShowDialog() ?? false;
        }
        #endregion

        #region Private Methods    
        #endregion

        #region Commands
        private RelayCommand _cancelSettingsCommand;
        public ICommand CancelSettingsCommand {
            get {
                if (_cancelSettingsCommand == null) {
                    _cancelSettingsCommand = new RelayCommand(CancelSettings);
                }
                return _cancelSettingsCommand;
            }
        }
        private void CancelSettings() {
            //ShortcutViewModels = _shortcutViewModelsBackup;
            _windowRef.Close();
        }

        private RelayCommand _saveSettingsCommand;
        public ICommand SaveSettingsCommand {
            get {
                if (_saveSettingsCommand == null) {
                    _saveSettingsCommand = new RelayCommand(SaveSettings);
                }
                return _saveSettingsCommand;
            }
        }
        private void SaveSettings() {
            _windowRef.Close();
        }

        private RelayCommand _resetSettingsCommand;
        public ICommand ResetSettingsCommand {
            get {
                if (_resetSettingsCommand == null) {
                    _resetSettingsCommand = new RelayCommand(ResetSettings);
                }
                return _resetSettingsCommand;
            }
        }
        private void ResetSettings() {

        }

        private RelayCommand _reassignShortcutCommand;
        public ICommand ReassignShortcutCommand {
            get {
                if (_reassignShortcutCommand == null) {
                    _reassignShortcutCommand = new RelayCommand(ReassignShortcut);
                }
                return _reassignShortcutCommand;
            }
        }
        private void ReassignShortcut() {
            var scvm = MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex];
            MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(
                scvm,
                scvm.ShortcutDisplayName,
                scvm.KeyString,
                scvm.Command,
                scvm.CommandParameter
            );
        }

        private RelayCommand _deleteShortcutCommand;
        public ICommand DeleteShortcutCommand {
            get {
                if (_deleteShortcutCommand == null) {
                    _deleteShortcutCommand = new RelayCommand(DeleteShortcut);
                }
                return _deleteShortcutCommand;
            }
        }
        private void DeleteShortcut() {
            Console.WriteLine("Deleting shortcut row: " + SelectedShortcutIndex);
            var scvm = MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex];
            MpShortcutCollectionViewModel.Instance.Remove(scvm);
        }

        private RelayCommand _resetShortcutCommand;
        public ICommand ResetShortcutCommand {
            get {
                if (_resetShortcutCommand == null) {
                    _resetShortcutCommand = new RelayCommand(ResetShortcut);
                }
                return _resetShortcutCommand;
            }
        }
        private void ResetShortcut() {
            Console.WriteLine("Reset row: " + SelectedShortcutIndex);
            MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex].KeyString = MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex].Shortcut.DefaultKeyString;
            MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex].Register();
            MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex].Shortcut.WriteToDatabase();
        }

        private RelayCommand<int> _clickSettingsPanelCommand;
        public ICommand ClickSettingsPanelCommand {
            get {
                if (_clickSettingsPanelCommand == null) {
                    _clickSettingsPanelCommand = new RelayCommand<int>(ClickSettingsPanel);
                }
                return _clickSettingsPanelCommand;
            }
        }
        private void ClickSettingsPanel(int panelClicked) {
            switch (panelClicked) {
                case 0:
                    SettingsPanel1Visibility = Visibility.Visible;
                    SettingsPanel2Visibility = Visibility.Collapsed;
                    SettingsPanel3Visibility = Visibility.Collapsed;
                    SettingsPanel4Visibility = Visibility.Collapsed;
                    SettingsPanel5Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    SettingsPanel2Visibility = Visibility.Visible;
                    SettingsPanel1Visibility = Visibility.Collapsed;
                    SettingsPanel3Visibility = Visibility.Collapsed;
                    SettingsPanel4Visibility = Visibility.Collapsed;
                    SettingsPanel5Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    SettingsPanel3Visibility = Visibility.Visible;
                    SettingsPanel2Visibility = Visibility.Collapsed;
                    SettingsPanel1Visibility = Visibility.Collapsed;
                    SettingsPanel4Visibility = Visibility.Collapsed;
                    SettingsPanel5Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    SettingsPanel4Visibility = Visibility.Visible;
                    SettingsPanel2Visibility = Visibility.Collapsed;
                    SettingsPanel3Visibility = Visibility.Collapsed;
                    SettingsPanel1Visibility = Visibility.Collapsed;
                    SettingsPanel5Visibility = Visibility.Collapsed;
                    break;
                case 4:
                    SettingsPanel5Visibility = Visibility.Visible;
                    SettingsPanel2Visibility = Visibility.Collapsed;
                    SettingsPanel3Visibility = Visibility.Collapsed;
                    SettingsPanel4Visibility = Visibility.Collapsed;
                    SettingsPanel1Visibility = Visibility.Collapsed;
                    break;
            }
        }
        #endregion
    }
}