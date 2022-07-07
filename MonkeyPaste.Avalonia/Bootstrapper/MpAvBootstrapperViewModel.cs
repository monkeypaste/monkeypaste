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

namespace MonkeyPaste.Avalonia {
    public class MpAvBootstrapperViewModel : MpBootstrapperViewModelBase {

        public MpAvBootstrapperViewModel() : base() {
            if(_items == null) {
                _items = new List<MpBootstrappedItemViewModel>();
            }

            _items.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpAvGlobalMouseHook))
                    //new MpBootstrappedItemViewModel(this,typeof(MpDocumentHtmlExtension)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpProcessManager), Properties.Settings.Default.IgnoredProcessNames),
                    ////new MpBootstrappedItemViewModel(this,typeof(MpProcessAutomation)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpScreenInformation)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpThemeColors)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpMeasurements)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpFileSystemWatcher)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpIconCollectionViewModel)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpAppCollectionViewModel)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpUrlCollectionViewModel)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpSourceCollectionViewModel)),


                    //new MpBootstrappedItemViewModel(this,typeof(MpSystemTrayViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpSoundPlayerGroupCollectionViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpClipTileSortViewModel)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpSearchBoxViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpAnalyticItemCollectionViewModel)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpClipboardHandlerCollectionViewModel)),

                    ////new MpBootstrappedItemViewModel(this,typeof(MpClipboardManager)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpClipTrayViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpShortcutCollectionViewModel)),


                    //new MpBootstrappedItemViewModel(this,typeof(MpTagTrayViewModel)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpMainWindowViewModel)),

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

            for (int i = 0; i < _items.Count; i++) {
                await LoadItemAsync(_items[i], i);
            }
            while(_items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }

            IsLoaded = true;
        }
    }
}