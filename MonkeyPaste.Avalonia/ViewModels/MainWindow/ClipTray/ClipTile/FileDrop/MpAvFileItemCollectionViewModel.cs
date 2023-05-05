using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace MonkeyPaste.Avalonia {
    public class MpAvFileItemCollectionViewModel : MpViewModelBase<MpAvClipTileViewModel> {
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


            // BUG will need to check source here... pretty much most places using env.newLine to parse right i think
            //  or substitute for 'portableNewLine' where necessary
            var ci_dobil = await MpDataModelProvider.GetDataObjectItemsForFormatByDataObjectIdAsync(ci.DataObjectId, MpPortableDataFormats.AvFileNames);
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
