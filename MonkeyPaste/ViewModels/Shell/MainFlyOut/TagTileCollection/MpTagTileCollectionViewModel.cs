using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste {
    public class MpTagTileCollectionViewModel : MpViewModelBase {
        #region Properties
        public MpClipTileCollectionPageViewModel ClipCollectionViewModel { get; set; }

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

        #region Public Methods
        public MpTagTileCollectionViewModel() : base() {
            PropertyChanged += MpTagCollectionViewModel_PropertyChanged;

            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            MpDb.Instance.OnItemUpdated += Db_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Db_OnItemDeleted;

            Task.Run(Initialize);
        }

        

        public MpTagTileViewModel CreateTagViewModel(MpTag tag) {
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
            IsBusy = true;
            var tags = await MpDb.Instance.GetItems<MpTag>();
            TagViewModels = new ObservableCollection<MpTagTileViewModel>(tags.Select(x => CreateTagViewModel(x)));
            OnPropertyChanged(nameof(TagViewModels));
            SelectedTagViewModel = RecentTagViewModel;
            ClipCollectionViewModel = new MpClipTileCollectionPageViewModel();
            await Task.Delay(300);
            IsBusy = false;
            
            return;
        }

        private async void MpTagCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedTagViewModel):
                    if (SelectedTagViewModel != null && ClipCollectionViewModel != null) {
                        ClearSelection();
                        SelectedTagViewModel.IsSelected = true;
                        await ClipCollectionViewModel.SetTag(SelectedTagViewModel.Tag.Id);
                    }
                    break;
            }
        }
        #endregion

        #region Event Handlers
        private void Db_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpTag t) {
                var ttvm = CreateTagViewModel(t);
                TagViewModels.Add(ttvm);          
            }
        }

        private void Db_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t) {
                var ttvmToRemove = TagViewModels.Where(x => x.Tag.Id == t.Id).FirstOrDefault();
                if(ttvmToRemove != null) {
                    //remove tag and update sort order
                    TagViewModels.Remove(ttvmToRemove);
                    int sortIdx = 0;
                    foreach(var ttvm in TagViewModels.OrderBy(x=>x.Tag.TagSortIdx)) {
                        ttvm.Tag.TagSortIdx = sortIdx++;
                        Task.Run(async()=> await MpDb.Instance.UpdateItem<MpTag>(ttvm.Tag));
                    }
                }
            }
        }

        private void Db_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpTag t) {
                var ttvmToUpdate = TagViewModels.Where(x => x.Tag.Id == t.Id).FirstOrDefault();
                if (ttvmToUpdate != null) {
                    if(TagViewModels.IndexOf(ttvmToUpdate) != t.TagSortIdx) {
                        TagViewModels.Move(TagViewModels.IndexOf(ttvmToUpdate), t.TagSortIdx);
                    }
                }
            }
        }

        private async void TagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpTagTileViewModel ttvm) {
                switch (e.PropertyName) {
                    case nameof(ttvm.IsSelected):
                        break;
                }
                await MpDb.Instance.UpdateItem<MpTag>(ttvm.Tag);
            }
        }
        #endregion

        #region Commands       
        public ICommand AddTagCommand => new Command<object>(async (args) => {
            var tagColor = new MpColor(MpHelpers.Instance.GetRandomColor());
            await MpDb.Instance.AddItem<MpColor>(tagColor);
            var newTag = new MpTag() {
                TagName = "Untitled",
                TagSortIdx = TagViewModels.Count,
                ColorId = tagColor.Id
            };
            //await MpDb.Instance.AddItem<MpColor>(newTag.TagColor);
            //newTag.ColorId = newTag.TagColor.Id;
            await MpDb.Instance.AddItem<MpTag>(newTag);
            //delay to let DbItem_Added add tag to collection
            await Task.Delay(300);
            SelectedTagViewModel = TagViewModels[TagViewModels.Count - 1];
        });

        public ICommand DeleteTagCommand => new Command<object>(async (args) => {
            if(args != null && args is MpTagTileViewModel ttvm) {
                if(TagViewModels.Contains(ttvm)) {
                    TagViewModels.Remove(ttvm);
                    await MpDb.Instance.DeleteItem<MpTag>(ttvm.Tag);                    
                }
            }
        });
        #endregion
    }
}
