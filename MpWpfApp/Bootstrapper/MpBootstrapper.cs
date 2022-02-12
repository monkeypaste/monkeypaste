using System.Linq;
using System.Reflection;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace MpWpfApp {
    public class MpBootstrapper : MpBootstrapperBase {

        public MpBootstrapper(MpINativeInterfaceWrapper niw, MpLoaderBalloonViewModel loader) : base(niw,loader) {
            if(_items == null) {
                _items = new List<MpBootstrappedItem>();
            } 

            _items.AddRange(
                new List<MpBootstrappedItem>() {
                    new MpBootstrappedItem(typeof(MpThemeColors)),

                    new MpBootstrappedItem(typeof(MpMeasurements)),
                    new MpBootstrappedItem(typeof(MpFileSystemWatcherViewModel)),

                    new MpBootstrappedItem(typeof(MpCursorViewModel)),

                    new MpBootstrappedItem(typeof(MpIconCollectionViewModel)),
                    new MpBootstrappedItem(typeof(MpAppCollectionViewModel)),
                    new MpBootstrappedItem(typeof(MpUrlCollectionViewModel)),
                    new MpBootstrappedItem(typeof(MpSourceCollectionViewModel)),


                    new MpBootstrappedItem(typeof(MpSystemTrayViewModel)),

                    new MpBootstrappedItem(typeof(MpSoundPlayerGroupCollectionViewModel)),

                    new MpBootstrappedItem(typeof(MpClipTileSortViewModel)),
                    new MpBootstrappedItem(typeof(MpSearchBoxViewModel)),

                    new MpBootstrappedItem(typeof(MpSidebarViewModel)),
                    new MpBootstrappedItem(typeof(MpAnalyticItemCollectionViewModel)),

                    new MpBootstrappedItem(typeof(MpClipTrayViewModel)),

                    new MpBootstrappedItem(typeof(MpShortcutCollectionViewModel)),


                    new MpBootstrappedItem(typeof(MpTagTrayViewModel)),
                    new MpBootstrappedItem(typeof(MpMainWindowViewModel)),

                    new MpBootstrappedItem(typeof(MpActionCollectionViewModel)),

                    new MpBootstrappedItem(typeof(MpContextMenu)),

                    new MpBootstrappedItem(typeof(MpDragDropManager)),

                    new MpBootstrappedItem(typeof(MpClipboardHelper.MpClipboardManager)),
                    new MpBootstrappedItem(typeof(MpMouseHook))
                });
        }

        public static async Task Init() {
            _loader = new MpLoaderBalloonViewModel();

            MpLoaderBalloonView.Init(_loader);
            

            var bootstrapper = new MpBootstrapper(new MpWpfWrapper(), _loader);

            await bootstrapper.Initialize();
        }

        public override async Task Initialize() {
            for (int i = 0; i < _items.Count; i++) {
                ReportItemLoading(_items[i], i);
                await _items[i].Register();
            }


            MpProcessHelper.MpProcessManager.Init(
                MpPreferences.FallbackProcessPath,
                MpAppCollectionViewModel.Instance.Items.Select(x => x.AppPath).ToArray(),
                new MpWpfIconBuilder());
        }
    }
}



            