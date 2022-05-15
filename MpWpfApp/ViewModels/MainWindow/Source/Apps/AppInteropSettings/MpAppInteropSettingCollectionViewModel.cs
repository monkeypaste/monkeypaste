using GalaSoft.MvvmLight.CommandWpf;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MpProcessHelper;
using System.Web.UI;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpAppInteropSettingCollectionViewModel : 
        MpSelectorViewModelBase<MpAppViewModel, MpAppInteropSettingViewModelBase> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpClipboardFormatPriorityViewModel> ClipboardFormats => 
            Items
                .Where(x => x is MpClipboardFormatPriorityViewModel)
                .Cast<MpClipboardFormatPriorityViewModel>();

        public MpPasteShortcutViewModel PasteShortcutViewModel =>
            Items
                .FirstOrDefault(x => x is MpPasteShortcutViewModel) as MpPasteShortcutViewModel;

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors       

        public MpAppInteropSettingCollectionViewModel(MpAppViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task Init(int appId) {
            IsBusy = true;
            
            var aisl = await MpDataModelProvider.GetInteropSettingsByAppId(appId);
            foreach(var ais in aisl) {
                var aisvm = await CreateAppInteropSettingViewModel(ais);
                Items.Add(aisvm);
            }

            while(Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        public async Task<MpAppInteropSettingViewModelBase> CreateAppInteropSettingViewModel(MpAppInteropSetting ais) {
            MpAppInteropSettingViewModelBase aisvm = null;
            switch(ais.SettingType) {
                case MpAppInteropSettingType.ClipboardFormatPriority:
                    aisvm = new MpClipboardFormatPriorityViewModel(this);
                    break;
                case MpAppInteropSettingType.PasteShortcut:
                    aisvm = new MpPasteShortcutViewModel(this);
                    break;
            }
            await aisvm.InitializeAsync(ais);
            return aisvm;
        }

        #endregion

        #region Commands

        public ICommand DeleteClipboardFormatTypeCommand => new RelayCommand<object>(
            async (cfaisvmArg) => {                
                var cfaisvm = cfaisvmArg as MpClipboardFormatPriorityViewModel;
                if(cfaisvm == null) {
                    return;
                }
                IsBusy = true;

                Items.Remove(cfaisvm);

                await cfaisvm.AppInteropSetting.DeleteFromDatabaseAsync();

                OnPropertyChanged(nameof(ClipboardFormats));

                IsBusy = false;

            });

        public ICommand AddClipboardFormatTypeCommand => new RelayCommand(
            async () => {
                IsBusy = true;

                var cfais = await MpAppInteropSetting.Create(
                    appId: Parent.AppId,
                    settingType: MpAppInteropSettingType.ClipboardFormatPriority,
                    arg1: ((int)MpClipboardFormatType.None).ToString(),
                    arg2: "0");

                var cfaisvm = await CreateAppInteropSettingViewModel(cfais);
                Items.Add(cfaisvm);

                while(cfaisvm.IsBusy) {
                    await Task.Delay(100);
                }

                OnPropertyChanged(nameof(ClipboardFormats));

                cfaisvm.IsSelected = true;

                IsBusy = false;

            });
        #endregion
    }
}
