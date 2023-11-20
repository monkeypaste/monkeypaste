using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace MonkeyPaste.Avalonia {
    public class MpAvFileItemCollectionViewModel : MpAvViewModelBase<MpAvClipTileViewModel> {
        #region Statics

        public static async Task<string> CreateFileListEditorFragment(MpCopyItem ci) {
            var ficvm = new MpAvFileItemCollectionViewModel();
            await ficvm.InitializeAsync(ci);
            var fl_frag = new MpQuillFileListDataFragment() {
                fileItems = ficvm.Items.Select(x => new MpQuillFileListItemDataFragmentMessage() {
                    filePath = x.Path,
                    fileIconBase64 = x.IconBase64
                }).ToList()
            };
            var itemData = fl_frag.SerializeJsonObjectToBase64();
            return itemData;
        }
        #endregion

        #region Properties

        #region View Models 
        public ObservableCollection<MpAvFileDataObjectItemViewModel> Items { get; set; } = new ObservableCollection<MpAvFileDataObjectItemViewModel>();
        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion
        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvFileItemCollectionViewModel() : base(null) { }

        public MpAvFileItemCollectionViewModel(MpAvClipTileViewModel parent) : base(parent) {
            Items.CollectionChanged += Items_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpCopyItem ci) {
            IsBusy = true;
            if (ci == null || ci.ItemType != MpCopyItemType.FileList) {
                Items.Clear();
                IsBusy = false;
                return;
            }

            var ci_dobil = await MpDataModelProvider.GetDataObjectItemsForFormatByDataObjectIdAsync(ci.DataObjectId, MpPortableDataFormats.Files);
            if (ci.ItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter) is string[] fpl) {
                // NOTE presuming text format returned from editor on content change is the current order of file items
                // sort paths by text order
                if (fpl.Length != ci_dobil.Count) {
                    MpDebug.Break($"FileList count mismatch content {fpl.Length} item paths {ci_dobil.Count} for item '{ci.Title}'. Using db.", true);
                    // write item and reset files to current content
                    await ci.WriteToDatabaseAsync();
                    // use those newly written item
                    ci_dobil = await MpDataModelProvider.GetDataObjectItemsForFormatByDataObjectIdAsync(ci.DataObjectId, MpPortableDataFormats.Files);
                }
                ci_dobil = ci_dobil.OrderBy(x => fpl.IndexOf(x.ItemData)).ToList();
            }
            var fivml = await Task.WhenAll(ci_dobil.Select(x => CreateFileItemViewModel(x)));
            Items.Clear();
            fivml.ForEach(x => Items.Add(x));
            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private async Task<MpAvFileDataObjectItemViewModel> CreateFileItemViewModel(MpDataObjectItem dobjItem) {
            var fivm = new MpAvFileDataObjectItemViewModel(this);
            await fivm.InitializeAsync(dobjItem);
            return fivm;
        }


        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (Parent == null) {
                return;
            }
            Parent.OnPropertyChanged(nameof(Parent.IconResourceObj));
        }
        #endregion
    }
}
