using System.Linq;
using System.Reflection;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using System.IO;
using System.Collections;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvBootstrapperViewModel : MpBootstrapperViewModelBase {

        public MpAvBootstrapperViewModel() : base() {
            if(_items == null) {
                _items = new List<MpBootstrappedItemViewModel>();
            }

            _items.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    //new MpBootstrappedItemViewModel(this,typeof(MpDocumentHtmlExtension)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpProcessManager), Properties.Settings.Default.IgnoredProcessNames),
                    ////new MpBootstrappedItemViewModel(this,typeof(MpProcessAutomation)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpScreenInformation)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpThemeColors)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpMeasurements)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpFileSystemWatcher)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvIconCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvAppCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvUrlCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSourceCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpAvSystemTrayViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpSoundPlayerGroupCollectionViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTileSortViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSearchBoxViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpAnalyticItemCollectionViewModel)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpClipboardHandlerCollectionViewModel)),

                    ////new MpBootstrappedItemViewModel(this,typeof(MpClipboardManager)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSettingsWindowViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTrayViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvShortcutCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpAvTagTrayViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvMainWindowViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpActionCollectionViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpContextMenuView)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpDragDropManager)),


                    //new MpBootstrappedItemViewModel(this,typeof(MpWpfDataObjectHelper)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpQuillHtmlToRtfConverter)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpTooltipInfoCollectionViewModel))
                    ////new MpBootstrappedItem(typeof(MpMouseHook))
                });
        }

        public override async Task InitAsync() {
            await MpNotificationCollectionViewModel.Instance.InitAsync(null);

            var nw = new MpAvNotificationWindow();
            nw.Opened += Nw_Opened;

            await MpNotificationCollectionViewModel.Instance.RegisterWithWindowAsync(nw);
            await MpNotificationCollectionViewModel.Instance.BeginLoaderAsync(this);
            while (IsLoaded == false) {
                await Task.Delay(100);
            }
        }

        private async void Nw_Opened(object sender, EventArgs e) {
            for (int i = 0; i < _items.Count; i++) {
                await LoadItemAsync(_items[i], i);
                while(IsBusy) {
                    await Task.Delay(100);
                }
            }
            while (_items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
            await Task.Delay(1000);


            MpNotificationCollectionViewModel.Instance.FinishLoading();
            MpAvClipTrayViewModel.Instance.OnPostMainWindowLoaded();
            IsLoaded = true;
        }

        protected override async Task LoadItemAsync(MpBootstrappedItemViewModel item, int index) {
            IsBusy = true;
            var sw = Stopwatch.StartNew();

            await item.LoadItemAsync();
            sw.Stop();

            LoadedCount++;
            MpConsole.WriteLine("Loaded " + item.Label + " at idx: " + index + " Load Count: " + LoadedCount + " Load Percent: " + PercentLoaded + " Time(ms): " + sw.ElapsedMilliseconds);

            OnPropertyChanged(nameof(PercentLoaded));

            OnPropertyChanged(nameof(Detail));

            Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

            int dotCount = index % 4;
            Title = "LOADING";
            for (int i = 0; i < dotCount; i++) {
                Title += ".";
            }

            IsBusy = false;
        }
    }
}