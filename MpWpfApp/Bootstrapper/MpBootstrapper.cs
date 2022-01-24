using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpBootstrapper : MpBootstrapperBase {
        private List<MpBootstrappedItem> _items;

        
        public MpBootstrapper(MpINativeInterfaceWrapper niw) : base(niw) {
            MpHelpers.Init(new MpProcessHelper.MpProcessIconBuilder(niw.GetIconBuilder()));

            _items = new List<MpBootstrappedItem>() {
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

                new MpBootstrappedItem(typeof(MpAppModeViewModel)),
                new MpBootstrappedItem(typeof(MpAnalyticItemCollectionViewModel)),
                new MpBootstrappedItem(typeof(MpSideBarTreeCollectionViewModel)),

                new MpBootstrappedItem(typeof(MpClipTrayViewModel)),

                new MpBootstrappedItem(typeof(MpShortcutCollectionViewModel)),


                new MpBootstrappedItem(typeof(MpTagTrayViewModel)),
                new MpBootstrappedItem(typeof(MpMainWindowViewModel)),

                new MpBootstrappedItem(typeof(MpDragDropManager)),


            };
        }

        public static MpBootstrapper Init() {
            return new MpBootstrapper(new MpWpfWrapper());
        }

        public override async Task Initialize() {
            await base.Initialize();
            for (int i = 0; i < _items.Count; i++) {
                await _items[i].Register();
            }

            MpProcessHelper.MpProcessManager.Start(
                MpPreferences.FallbackProcessPath,
                MpAppCollectionViewModel.Instance.AppViewModels.Select(x => x.AppPath).ToArray(),
                new MpWpfIconBuilder());
        }
    }
}



            