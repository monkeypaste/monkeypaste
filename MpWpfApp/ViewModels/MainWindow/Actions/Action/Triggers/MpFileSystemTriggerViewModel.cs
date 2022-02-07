using MonkeyPaste;
using System.IO;

namespace MpWpfApp {
    public class MpFileSystemTriggerViewModel : MpTriggerActionViewModelBase, MpIFileSystemEventHandler {
        #region Properties

        #region Model

        public string FileSystemPath {
            get {
                if (Action == null) {
                    return null;
                }
                return Action.Arg1;
            }
            set {
                if (FileSystemPath != value) {
                    Action.Arg1 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(FileSystemPath));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpFileSystemTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Public Methods

        public override void Enable() {
            if(!IsEnabled) {
                MpFileSystemWatcherViewModel.Instance.RegisterTrigger(this);
            }
            base.Enable();
        }

        public override void Disable() {
            if(IsEnabled) {
                MpFileSystemWatcherViewModel.Instance.UnregisterTrigger(this);
            }
            base.Disable();
        }

        #endregion

        #region MpIFileSystemWatcher Implementation

        void MpIFileSystemEventHandler.OnFileSystemItemChanged(object sender, FileSystemEventArgs e) {
            MpHelpers.RunOnMainThread(async () => {
                MpCopyItem ci = null;
                switch (e.ChangeType) {
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Created:
                        var app = MpPreferences.ThisAppSource.App;
                        var source = await MpSource.Create(app, null);
                        ci = await MpCopyItem.Create(source, e.FullPath, MpCopyItemType.FileList, true);
                        break;
                    case WatcherChangeTypes.Renamed:
                        RenamedEventArgs re = e as RenamedEventArgs;
                        ci = await MpDataModelProvider.GetCopyItemByData(re.OldFullPath);
                        ci.ItemData = re.FullPath;
                        await ci.WriteToDatabaseAsync();
                        break;
                }

                if (ci != null) {
                    await PerformAction(ci);
                }
            });
        }

        #endregion
    }
}
