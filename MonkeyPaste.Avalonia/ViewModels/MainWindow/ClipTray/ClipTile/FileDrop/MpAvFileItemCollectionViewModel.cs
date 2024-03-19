using Avalonia.Controls;
using Avalonia.Controls.Selection;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvFileItemCollectionViewModel : MpAvViewModelBase<MpAvClipTileViewModel> {
        #region Statics
        static bool VALIDATE_FILE_MODELS = false;
        #endregion

        #region Properties

        #region View Models 
        public ObservableCollection<MpAvFileDataObjectItemViewModel> Items { get; set; } = new ObservableCollection<MpAvFileDataObjectItemViewModel>();

        public SelectionModel<MpAvFileDataObjectItemViewModel> Selection { get; }
        #endregion

        #region Appearance

        public object PrimaryIconSourceObj {
            get {
                if (Items.All(x => !x.IsAvailable)) {
                    return "FolderImage";
                }
                return Items.FirstOrDefault(x => x.IsAvailable).Path;
            }
        }

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        public bool HasMultiple =>
            Items.Count > 1;
        #endregion
        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvFileItemCollectionViewModel() : base(null) { }

        public MpAvFileItemCollectionViewModel(MpAvClipTileViewModel parent) : base(parent) {
            Selection = new SelectionModel<MpAvFileDataObjectItemViewModel>(Items);
            Selection.SelectionChanged += SelectionChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpCopyItem ci) {
            IsBusy = true;

            Items.Clear();
            ClearDetailInfo();
            if (ci == null ||
                ci.ItemType != MpCopyItemType.FileList ||
                ci.ItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter) is not string[] fpl) {
                IsBusy = false;
                return;
            }

            if (VALIDATE_FILE_MODELS) {
                var ci_dobil = await MpDataModelProvider.GetDataObjectItemsForFormatByDataObjectIdAsync(ci.DataObjectId, MpPortableDataFormats.Files);
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
            var fivml = await Task.WhenAll(fpl.Select(x => CreateFileItemViewModel(x)));
            fivml.ForEach(x => Items.Add(x));
            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }

        public void SetDetailInfo() {
            if (Parent == null) {
                return;
            }
            Parent.CopyItemSize1 = Items.Count;
            Parent.CopyItemSize2 = (int)MpFileIo.GetPathsSizeInMegaBytes(Items.Select(x => x.Path));
        }
        public void ClearDetailInfo() {
            if (Parent == null) {
                return;
            }
            Parent.CopyItemSize1 = 0;
            Parent.CopyItemSize2 = 0;
        }
        #endregion

        #region Private Methods

        private async Task<MpAvFileDataObjectItemViewModel> CreateFileItemViewModel(string path) {
            var fivm = new MpAvFileDataObjectItemViewModel(this);
            await fivm.InitializeAsync(path);
            return fivm;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (Parent == null) {
                return;
            }
            Parent.OnPropertyChanged(nameof(Parent.IconResourceObj));
            OnPropertyChanged(nameof(HasMultiple));
        }
        private void SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e) {
            if (e.DeselectedItems is { } unselected &&
                unselected.OfType<MpAvFileDataObjectItemViewModel>() is { } unselected_fivml) {
                foreach (var to_unselect in unselected_fivml) {
                    to_unselect.IsSelected = false;
                }
            }
            if (e.SelectedItems is { } selected &&
                selected.OfType<MpAvFileDataObjectItemViewModel>() is { } selected_fivml) {
                foreach (var to_select in selected_fivml) {
                    to_select.IsSelected = true;
                }
            }
            OnPropertyChanged(nameof(Selection));
        }

        #endregion

        #region Commands

        public ICommand RemoveFileItemCommand => new MpCommand<object>(
             (args) => {
                 if (Parent == null ||
                 args is not MpAvFileDataObjectItemViewModel fivm) {
                     return;
                 }
                 int to_remove_idx = Items.IndexOf(fivm);
                 if (to_remove_idx < 0) {
                     return;
                 }

                 Items.RemoveAt(to_remove_idx);
                 string new_data = string.Join(MpCopyItem.FileItemSplitter, Items.Select(x => x.Path));

                 Parent.IgnoreHasModelChanged = true;

                 Parent.SearchableText = new_data;
                 Parent.CopyItemData = new_data;

                 // just clear detail so it can get populated lazy
                 Parent.CopyItemSize1 = 0;
                 Parent.CopyItemSize2 = 0;

                 Parent.IgnoreHasModelChanged = false;

                 Parent.OnPropertyChanged(nameof(Parent.DetailText));
             },
            (args) => {
                return HasMultiple;
            });
        #endregion
    }
}
