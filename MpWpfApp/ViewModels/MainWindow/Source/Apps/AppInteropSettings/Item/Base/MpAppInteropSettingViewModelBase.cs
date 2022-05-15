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
using MonkeyPaste;
using System.Collections.ObjectModel;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public abstract class MpAppInteropSettingViewModelBase : 
        MpViewModelBase<MpAppInteropSettingCollectionViewModel>,
        MpISelectableViewModel {
        #region Private Variables
        #endregion

        #region Properties

        #region MpISelectableViewModel Implementaton
        public bool IsSelected { get; set; }

        #endregion

        #region Model Properties

        public int AppInteropSettingId {
            get {
                if(AppInteropSetting == null) {
                    return 0;
                }
                return AppInteropSetting.Id;
            }
        }

        public int AppId {
            get {
                if (AppInteropSetting == null) {
                    return 0;
                }
                return AppInteropSetting.AppId;
            }
        }

        public MpAppInteropSetting AppInteropSetting { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpAppInteropSettingViewModelBase() : base(null) { }

        public MpAppInteropSettingViewModelBase(MpAppInteropSettingCollectionViewModel parent) : base(parent) {
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpAppInteropSetting ais) {
            IsBusy = true;

            await Task.Delay(1);
            AppInteropSetting = ais;

            IsBusy = false;
        }

        #endregion

        #region Commands

        #endregion
    }
}
