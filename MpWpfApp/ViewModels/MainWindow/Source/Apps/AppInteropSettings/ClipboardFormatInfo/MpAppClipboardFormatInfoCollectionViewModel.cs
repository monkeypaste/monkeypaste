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
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MpWpfApp {
    public class MpAppClipboardFormatInfoCollectionViewModel : 
        MpSelectorViewModelBase<MpAppViewModel, MpAppClipboardFormatInfoViewModel> {
        #region Private Variables

        private static readonly MpClipboardFormatType[] _DefaultFormats = new MpClipboardFormatType[] {
            MpClipboardFormatType.Text,
            MpClipboardFormatType.Html,
            MpClipboardFormatType.Rtf,
            MpClipboardFormatType.Csv,
            MpClipboardFormatType.Bitmap,
            MpClipboardFormatType.FileDrop
        };

        #endregion

        #region Properties

        #region View Models

        //public ObservableCollection<MpAppClipboardFormatInfoViewModel> Items =>
        //    new ObservableCollection<MpAppClipboardFormatInfoViewModel>(
        //        Items.Where(x => !x.IsFormatIgnored).OrderByDescending(x=>x.Priority));

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || base.Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors       

        public MpAppClipboardFormatInfoCollectionViewModel(MpAppViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task Init(int appId) {
            IsBusy = true;

            var aisl = await MpDataModelProvider.GetAppClipboardFormatInfosByAppId(appId);

            for (int i = 0; i < _DefaultFormats.Length; i++) {
                MpClipboardFormatType defType = _DefaultFormats[i];
                if(aisl.Any(x=>x.FormatType == defType)) {
                    continue;
                }

                var defInfo = new MpAppClipboardFormatInfo() {
                    AppId = appId,
                    FormatType = defType,
                    IgnoreFormatValue = aisl.Count
                };
                aisl.Add(defInfo);
            }

            foreach (var ais in aisl.OrderBy(x=>x.IgnoreFormatValue)) {
                var aisvm = await CreateAppClipboardFormatViewModel(ais);
                base.Items.Add(aisvm);
            }

            while(base.Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        public async Task<MpAppClipboardFormatInfoViewModel> CreateAppClipboardFormatViewModel(MpAppClipboardFormatInfo ais) {
            MpAppClipboardFormatInfoViewModel aisvm = new MpAppClipboardFormatInfoViewModel(this);
            await aisvm.InitializeAsync(ais);
            return aisvm;
        }

        #endregion

        #region Commands

        public ICommand DeleteClipboardFormatTypeCommand => new RelayCommand<object>(
            async (cfaisvmArg) => {                
                var cfaisvm = cfaisvmArg as MpAppClipboardFormatInfoViewModel;
                if(cfaisvm == null) {
                    return;
                }
                IsBusy = true;

                base.Items.Remove(cfaisvm);

                await cfaisvm.AppClipboardFormatInfo.DeleteFromDatabaseAsync();

                OnPropertyChanged(nameof(Items));

                IsBusy = false;

            });

        public ICommand AddClipboardFormatTypeCommand => new RelayCommand(
            async () => {
                IsBusy = true;

                var cfais = await MpAppClipboardFormatInfo.Create(
                    appId: Parent.AppId);

                var cfaisvm = await CreateAppClipboardFormatViewModel(cfais);
                base.Items.Add(cfaisvm);

                while(cfaisvm.IsBusy) {
                    await Task.Delay(100);
                }

                OnPropertyChanged(nameof(Items));

                cfaisvm.IsSelected = true;

                IsBusy = false;

            });
        #endregion
    }
}
