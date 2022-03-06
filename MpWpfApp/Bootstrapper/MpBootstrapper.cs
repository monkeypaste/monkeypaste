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

        public MpBootstrapper(MpINativeInterfaceWrapper niw) : base(niw) {
            if(_items == null) {
                _items = new List<MpBootstrappedItem>();
            } 

            _items.AddRange(
                new List<MpBootstrappedItem>() {
                    new MpBootstrappedItem(typeof(MpThemeColors)),

                    new MpBootstrappedItem(typeof(MpMeasurements)),
                    new MpBootstrappedItem(typeof(MpFileSystemWatcher)),

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
            Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr = string.Empty;
            Properties.Settings.Default.Save();

            List<int> doNotShowNotifications = null;
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr)) {
                doNotShowNotifications = Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).ToList();
            }

            await MpNotificationCollectionViewModel.Instance.Init(doNotShowNotifications);

            var nbv = new MpNotificationWindow();
            await MpNotificationCollectionViewModel.Instance.RegisterWithWindow(nbv.NotificationBalloon);
            nbv.DataContext = MpNotificationCollectionViewModel.Instance;

            MpNotificationCollectionViewModel.Instance.BeginLoader();

            var bootstrapper = new MpBootstrapper(new MpWpfWrapper());

            await bootstrapper.Initialize();

            MpNotificationCollectionViewModel.Instance.FinishLoading();
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