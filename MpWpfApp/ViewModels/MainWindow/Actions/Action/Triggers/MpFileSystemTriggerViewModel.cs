using FFImageLoading.Helpers.Exif;
using MonkeyPaste;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        #region Protected Methods

        protected override async Task Enable() {
            await base.Enable();
            MpFileSystemWatcherViewModel.Instance.RegisterTrigger(this);
        }

        protected override async Task Disable() {
            await base.Disable();
            MpFileSystemWatcherViewModel.Instance.UnregisterTrigger(this);
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
                        ci = await MpCopyItem.Create(
                            source: source,
                            itemType: MpCopyItemType.FileList,
                            data: e.FullPath,
                            suppressWrite: true);
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
