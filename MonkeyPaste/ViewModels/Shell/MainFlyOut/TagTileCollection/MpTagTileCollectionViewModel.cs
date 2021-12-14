using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;
using Xamarin.Forms.Internals;
using FFImageLoading.Helpers.Exif;

namespace MonkeyPaste {
    public class MpTagTileCollectionViewModel : MpViewModelBase {
        #region Private Variables
        private int _resortStartIdx = -1;
        #endregion

        #region Properties

        #region View Models
        public MpCopyItemTileCollectionPageViewModel CopyItemCollectionViewModel { get; set; }

        public ObservableCollection<MpTagTileViewModel> TagViewModels { get; set; } = new ObservableCollection<MpTagTileViewModel>();

        public MpTagTileViewModel SelectedTagViewModel { get; set; }

        public MpTagTileViewModel RecentTagViewModel {
            get {
                return TagViewModels.Where(x => x.Tag.Id == 1).FirstOrDefault();
            }
        }

        public MpTagTileViewModel FavoritesTagViewModel {
            get {
                return TagViewModels.Where(x => x.Tag.Id == 3).FirstOrDefault();
            }
        }
        #endregion

        #endregion

        #region Events
        public event EventHandler<int> OnTagSelectionChanged;
        #endregion


        #region Public Methods
        public MpTagTileCollectionViewModel() : base() {
            PropertyChanged += MpTagCollectionViewModel_PropertyChanged;

            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            MpDb.Instance.OnItemUpdated += Db_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Db_OnItemDeleted;

            Task.Run(Initialize);
        }       

        public MpTagTileViewModel CreateTagViewModel(MpTag tag) {
            if(tag.TagSortIdx < 0) {
                tag.TagSortIdx = TagViewModels.Count;
            }
            MpTagTileViewModel ttvm = new MpTagTileViewModel(tag);
            ttvm.PropertyChanged += TagViewModel_PropertyChanged;
            return ttvm;
        }

        public void ClearSelection() {
            foreach (var tivm in TagViewModels) {
                tivm.IsSelected = false;
            }
        }
        #endregion

        #region Private Methods
        private async Task Initialize() {
            if(TagViewModels != null && TagViewModels.Count > 0) {
                //occurs when tags are reloaded, like after a delete
                await UpdateSort();
            }
            IsBusy = true;
            var tags = await MpDb.Instance.GetItemsAsync<MpTag>();
            var tvms = tags.Select(x => CreateTagViewModel(x)).OrderBy(x=>x.Tag.TagSortIdx);
            TagViewModels = new ObservableCollection<MpTagTileViewModel>(tvms);
            OnPropertyChanged(nameof(TagViewModels));
            CopyItemCollectionViewModel = new MpCopyItemTileCollectionPageViewModel();
            await Task.Delay(300);
            RecentTagViewModel.IsSelected = true;
            IsBusy = false;
        }

        private async Task UpdateSort(bool fromDb = false) {
            if(fromDb) {

            } else {
                foreach(var tvm in TagViewModels) {
                    tvm.Tag.TagSortIdx = TagViewModels.IndexOf(tvm);
                    await tvm.Tag.WriteToDatabaseAsync();
                }
            }
        }
        #endregion

        #region Event Handlers

        private void MpTagCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedTagViewModel):
                    if (SelectedTagViewModel != null && CopyItemCollectionViewModel != null) {
                        ClearSelection();
                        SelectedTagViewModel.IsSelected = true;
                        CopyItemCollectionViewModel.SetTag(SelectedTagViewModel.Tag.Id);
                    }
                    break;
            }
        }

        private void Db_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpTag t) {
                var dupTag = TagViewModels.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                if(dupTag == null) {
                    var ttvm = CreateTagViewModel(t);
                    TagViewModels.Add(ttvm);
                } else {
                    TagViewModels[TagViewModels.IndexOf(dupTag)].Tag = t;
                }                
            } 
        }

        private void Db_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag) {
                Device.InvokeOnMainThreadAsync(async () => {
                    await Task.Run(Initialize);
                });
            }
        }

        private void Db_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpTag t) {
                if(SelectedTagViewModel.Tag.Id == t.Id) {
                    if(CopyItemCollectionViewModel.CopyItemViewModels.Count == 0) {
                        CopyItemCollectionViewModel.EmptyCollectionLableText = string.Format(@"No Clips could be found in '{0}' Collection", t.TagName);                       
                    }
                }
            }
        }

        private void TagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpTagTileViewModel ttvm) {
                switch (e.PropertyName) {
                    case nameof(ttvm.IsSelected):
                        if(ttvm.IsSelected) {
                            SelectionChangedCommand.Execute(ttvm);
                        }
                        break;
                }
            }
        }
        #endregion

        #region Commands       

        #region Drag & Drop
        public ICommand ItemDragged => new Command<MpTagTileViewModel>((item) => {
            Debug.WriteLine($"OnItemDragged: {item?.Tag.TagName}");
            TagViewModels.ForEach(i => i.IsBeingDragged = item == i);
        });

        public ICommand ItemDraggedOver => new Command<MpTagTileViewModel>((item) => {
            Debug.WriteLine($"OnItemDraggedOver: {item?.Tag.TagName}");
            var itemBeingDragged = TagViewModels.FirstOrDefault(i => i.IsBeingDragged);
            TagViewModels.ForEach(i => i.IsBeingDraggedOver = item == i && item != itemBeingDragged);
        });

        public ICommand ItemDragLeave => new Command<MpTagTileViewModel>((item) => {
            Debug.WriteLine($"OnItemDragLeave: {item?.Tag.TagName}");
            TagViewModels.ForEach(i => i.IsBeingDraggedOver = false);
        });

        public ICommand ItemDropped => new Command<MpTagTileViewModel>(async (item) => {
            var itemToMove = TagViewModels.First(i => i.IsBeingDragged);
            var itemToInsertBefore = item;
            if (itemToMove == null || itemToInsertBefore == null || itemToMove == itemToInsertBefore) {
                return;
            }
            var insertFromIndex = TagViewModels.IndexOf(itemToMove);
            var insertAtIndex = TagViewModels.IndexOf(itemToInsertBefore);
            TagViewModels.Move(insertFromIndex, insertAtIndex);
            itemToMove.IsBeingDragged = false;
            itemToInsertBefore.IsBeingDraggedOver = false;

            itemToMove.Tag.TagSortIdx = TagViewModels.IndexOf(itemToMove);
            itemToInsertBefore.Tag.TagSortIdx = TagViewModels.IndexOf(itemToInsertBefore);

            await itemToMove.Tag.WriteToDatabaseAsync();
            await itemToInsertBefore.Tag.WriteToDatabaseAsync();
        });
        #endregion


        public ICommand AddTagCommand => new Command<object>(async (args) => {
            var newTag = new MpTag() {
                TagName = "Untitled",
                TagSortIdx = TagViewModels.Count,
                HexColor = MpHelpers.Instance.GetRandomColor().ToHex()
            };
            //await MpDb.Instance.AddItem<MpColor>(newTag.Color);
            //newTag.ColorId = newTag.Color.Id;
            //await MpDb.Instance.AddItemAsync<MpTag>(newTag);

            //trigger db_onItemAdded instead of actually adding item because renaming is called automatically
            Db_OnItemAdded(this, newTag);
            //delay to let DbItem_Added add tag to collection
            await Task.Delay(300);

            SelectedTagViewModel = TagViewModels[TagViewModels.Count - 1];
            SelectedTagViewModel.RenameTagCommand.Execute(null);
        });       

        public ICommand SelectionChangedCommand => new Command<object>( (args) => {
            if (args != null && args is MpTagTileViewModel ttvm) {
                SelectedTagViewModel = args as MpTagTileViewModel;
            }
        });
        #endregion
    }
}
