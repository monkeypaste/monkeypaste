
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
using MonkeyPaste;
using GalaSoft.MvvmLight.CommandWpf;

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
        //private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsBackup = new ObservableCollection<MpShortcutViewModel>();
        //private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsToDelete = new ObservableCollection<MpShortcutViewModel>();
        //private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsToRegister = new ObservableCollection<MpShortcutViewModel>();
        #endregion

        #region Static Variables
        #endregion

        #region Properties

        #region View Models
        public MpShortcutCollectionViewModel ShortcutCollectionViewModel => MpShortcutCollectionViewModel.Instance;

        public MpAppCollectionViewModel AppCollectionViewModel => MpAppCollectionViewModel.Instance;

        public MpUrlCollectionViewModel UrlCollectionViewModel => MpUrlCollectionViewModel.Instance;

        public MpPreferencesMenuViewModel PreferencesViewModel { get; set; } =  new MpPreferencesMenuViewModel();
        #endregion

        #region Panel Visibility

        public Visibility SettingsPanel1Visibility { get; set; } = Visibility.Collapsed;
        public Visibility SettingsPanel2Visibility { get; set; } = Visibility.Collapsed;
        public Visibility SettingsPanel3Visibility { get; set; } = Visibility.Collapsed;
        public Visibility SettingsPanel4Visibility { get; set; } = Visibility.Collapsed;
        public Visibility SettingsPanel5Visibility { get; set; } = Visibility.Collapsed;
        public Visibility SettingsPanel6Visibility { get; set; } = Visibility.Collapsed;

        #endregion        
                
        #endregion

        #region Static Methods
        
        #endregion

        #region Public Methods
        public MpSettingsWindowViewModel() : base(null) {
            PreferencesViewModel = new MpPreferencesMenuViewModel(this);
            //SecurityViewModel = new MpSecurityViewModel(this);
        }


        public async Task InitializeAsync(int tabToShow = 1, object args = null) {
            await Task.Delay(1);

            ClickSettingsPanelCommand.Execute(tabToShow);

            if (args != null) {
                if (tabToShow == 1) {
                    if (args is MpApp app) {
                        PreferencesViewModel.PasteToAppPathViewModelCollection.AddPasteToAppPathCommand.Execute(app);
                    }
                }
            }
        }

        #endregion

        #region Private Methods    
        #endregion

        #region Commands
        public ICommand CancelSettingsCommand => new RelayCommand(
            () => {

            });

        public ICommand SaveSettingsCommand => new RelayCommand(
            () => {

            });

        public ICommand ResetSettingsCommand => new RelayCommand(
            () => {

            });

        public ICommand ClickSettingsPanelCommand => new RelayCommand<object>(
            (args) => {
                switch ((int)args) {
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
            },(args) => args != null);

        #endregion
    }
}