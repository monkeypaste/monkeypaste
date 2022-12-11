
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Input;


using MonkeyPaste;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;


using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 

namespace MonkeyPaste.Avalonia {
    public class MpAppClipboardFormatInfoCollectionViewModel : 
        MpAvSelectorViewModelBase<MpAvAppViewModel, MpAppClipboardFormatInfoViewModel> {
        #region Private Variables

        //private static readonly MpClipboardFormatType[] _DefaultFormats = new MpClipboardFormatType[] {
        //    MpClipboardFormatType.Text,
        //    MpClipboardFormatType.Html,
        //    MpClipboardFormatType.Rtf,
        //    MpClipboardFormatType.Csv,
        //    MpClipboardFormatType.Bitmap,
        //    MpClipboardFormatType.FileDrop
        //};

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

        public MpAppClipboardFormatInfoCollectionViewModel(MpAvAppViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int appId) {
            IsBusy = true;

            var overrideInfos = await MpDataModelProvider.GetAppClipboardFormatInfosByAppIdAsync(appId);
            Items.Clear();
            foreach(var format in MpPortableDataFormats.RegisteredFormats) {
                if (overrideInfos.Any(x => x.FormatType == format)) {
                    continue;
                }

                var defInfo = new MpAppClipboardFormatInfo() {
                    AppId = appId,
                    FormatType = format,
                    IgnoreFormatValue = overrideInfos.Count
                };
                overrideInfos.Add(defInfo);
            }

            foreach (var ais in overrideInfos.OrderBy(x=>x.IgnoreFormatValue)) {
                var aisvm = await CreateAppClipboardFormatViewModel(ais);
                Items.Add(aisvm);
            }

            while(Items.Any(x => x.IsBusy)) {
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

        public ICommand DeleteClipboardFormatTypeCommand => new MpCommand<object>(
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

        public ICommand AddClipboardFormatTypeCommand => new MpCommand(
            async () => {
                IsBusy = true;

                var cfais = await MpAppClipboardFormatInfo.CreateAsync(
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
