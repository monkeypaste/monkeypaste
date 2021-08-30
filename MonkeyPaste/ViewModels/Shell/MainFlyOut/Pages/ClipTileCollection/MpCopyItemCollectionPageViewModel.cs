using FFImageLoading.Helpers.Exif;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemTileCollectionPageViewModel : MpViewModelBase {
        #region Private Variables        
        private int _itemsAdded = 0;
        private int _currentStartIndex = 0;
        private int _pageSize = 20;
        #endregion

        #region Statics
        public static bool IsAnyItemExpanded { get; set; } = false;
        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpCopyItemViewModel> CopyItemViewModels { get; set; } = new ObservableCollection<MpCopyItemViewModel>();

        public MpCopyItemViewModel SelectedCopyItemViewModel {
            get {
                return CopyItemViewModels.Where(x => x.IsSelected).FirstOrDefault();
            }
        }
        #endregion

        public string EmptyCollectionLableText { get; set; }

        public int TagId { get; set; }

        public bool IsEditButtonEnabled {
            get {
                return true;//SelectedCopyItemViewModel != null;
            }
        }

        public ImageSource ClipboardToolbarIcon {
            get {
                if (SelectedCopyItemViewModel == null) {
                    return Application.Current.Resources["HiddenIcon"] as FontImageSource;
                }
                if (SelectedCopyItemViewModel.WasSetToClipboard) {
                    return Application.Current.Resources["ClipboardSolidCheckedIcon"] as FontImageSource;
                }
                return Application.Current.Resources["ClipboardOutlineIcon"] as FontImageSource;
            }
        }
        #endregion

        #region Public Methods
        public MpCopyItemTileCollectionPageViewModel() : this(1) { }

        public MpCopyItemTileCollectionPageViewModel(int tagId) : base() {
            PropertyChanged += MpCopyItemCollectionViewModel_PropertyChanged;

            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            MpDb.Instance.OnItemUpdated += Db_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Db_OnItemDeleted;

            _ = Task.Run(() => Initialize(tagId));
        }

        public async Task SetTag(int tagId) {
            TagId = tagId; 
            await Device.InvokeOnMainThreadAsync(async () => {
                
                var clips = await MpCopyItem.GetPage(TagId, 0, _pageSize);
                CopyItemViewModels = new ObservableCollection<MpCopyItemViewModel>(clips.Select(x=>CreateCopyItemViewModel(x)));                
                if(clips.Count == 0) {
                    var tl = await MpDb.Instance.GetItemsAsync<MpTag>();
                    var t = tl.Where(x => x.Id == TagId).FirstOrDefault();
                    if(t != null) {
                        EmptyCollectionLableText = string.Format(@"No Clips could be found in '{0}' Collection", t.TagName);
                    }
                }
                CopyItemViewModels.CollectionChanged += CopyItemViewModels_CollectionChanged;
                OnPropertyChanged(nameof(CopyItemViewModels));
            });
        }

        public MpCopyItemViewModel CreateCopyItemViewModel(MpCopyItem c) {
            MpCopyItemViewModel ctvm = null;
            ctvm = new MpCopyItemViewModel(c);
            ctvm.PropertyChanged += CopyItemViewModel_PropertyChanged;
            return ctvm;
        }

        public void ClearSelection(MpCopyItemViewModel ignoreVm = null) {
            foreach (var civm in CopyItemViewModels) {
                if(civm == ignoreVm) {
                    continue;
                }
                civm.IsSelected = false;
            }
        }

        public void OnSearchQueryChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var civmsh = sender as MpCopyItemViewModelSearchHandler; 
            switch(e.PropertyName) {
                case nameof(civmsh.Query):
                    PerformSearchCommand.Execute(civmsh.Query);
                    break;
            }
        }
        #endregion

        #region Private Methods
        private async Task Initialize(int tagId) {
            IsBusy = true;
            while(!MpDb.Instance.IsLoaded) {
                Thread.Sleep(50);
            }

            await SetTag(tagId);
            await Task.Delay(300);
            IsBusy = false;
        }

        #region Event Handlers
        private void MpCopyItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedCopyItemViewModel):
                    OnPropertyChanged(nameof(IsEditButtonEnabled));
                    OnPropertyChanged(nameof(ClipboardToolbarIcon));
                    break;
            }
        }

        private void CopyItemViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //if (e.NewItems != null && e.NewItems.Count > 0) {
            //    IsBusy = false;
            //    CopyItemViewModels.CollectionChanged -= CopyItemViewModels_CollectionChanged;
            //}
            ClearSelection();
        }

        private void CopyItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpCopyItemViewModel civm) {
                switch (e.PropertyName) {
                    case nameof(civm.IsSelected):
                        if (civm.IsSelected) {
                            ClearSelection(civm);
                        }
                        OnPropertyChanged(nameof(SelectedCopyItemViewModel));
                        break;
                }
            }
        }

        //private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args) {
        //    foreach (MpCopyItem CopyItem in args.NewItems) {
        //        if(!CopyItemViewModels.Any(x=>x.CopyItem.Id == CopyItem.Id)) {
        //            _itemsAdded++;
        //            CopyItemViewModels.Add(CreateCopyItemViewModel(CopyItem));
        //        }
        //    }
        //    if (_itemsAdded == _pageSize) {
        //        var collection = (ObservableCollection<MpCopyItem>)sender;
        //        collection.CollectionChanged -= Collection_CollectionChanged;
        //    }
        //}

        private void SearchBarViewModel_SearchTextChanged(object sender, string e) {
            PerformSearchCommand.Execute(e);
        }

        private void Db_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpCopyItem ci) {
                var civm = CopyItemViewModels.Where(x => x.CopyItem.Id == ci.Id).FirstOrDefault();
                if(civm != null) {
                    CopyItemViewModels.Remove(civm);
                }
            }
        }

        private void Db_OnItemUpdated(object sender, MpDbModelBase e) {
            //if (e is MpCopyItem uci) {
            //    var uctvm = await CreateCopyItemViewModel(uci);
            //    var ctvm = CopyItemViewModels.Where(x => x.CopyItem.Id == uci.Id).FirstOrDefault();
            //    if(ctvm != null) {
            //        CopyItemViewModels[CopyItemViewModels.IndexOf(ctvm)] = uctvm;
            //    }
                
            //}
        }

        private void Db_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpCopyItemTag citg) {
                if(citg.TagId == TagId && !CopyItemViewModels.Any(x => x.CopyItem.Id == citg.CopyItemId)) {
                    _ = Task.Run(() => Initialize(TagId));
                }                
            }  
        }
        #endregion

        #endregion

        #region Commands
        public ICommand PerformSearchCommand => new Command<string>((string query) => {
            if(CopyItemViewModels == null) {
                return;
            }
            IEnumerable<MpCopyItemViewModel> searchResult = null;
            if (string.IsNullOrEmpty(query)) {
                searchResult = CopyItemViewModels;
            } else {
                searchResult = from civm in CopyItemViewModels
                               where civm.CopyItem.ItemText.ContainsByUserSensitivity(query)
                               select civm;//.Skip(2).Take(2);
            }
            foreach(var civm in CopyItemViewModels) {
                civm.IsVisible = searchResult.Contains(civm);
            }
        });

        public ICommand DeleteCopyItemCommand => new Command<object>(async (args) => {
            if(args == null || args is not MpCopyItemViewModel civm) {
                return;
            }
            CopyItemViewModels.Remove(civm);
            
            await MpDb.Instance.DeleteItemAsync(civm.CopyItem);
            await MpCopyItemTag.DeleteAllCopyItemTagsForCopyItemId(civm.CopyItem.Id);
        });

        public ICommand LoadMoreCopyItemsCommand => new Command(async () => {
            //_currentStartIndex += _pageSize;
            //_itemsAdded = 0;
            //var collection = await MpCopyItem.GetPage(TagId, _currentStartIndex, _pageSize);
            //collection.CollectionChanged += Collection_CollectionChanged;
        });

        public ICommand SelectionChangedCommand => new Command<object>((args) => {
            if (args != null && args is MpCopyItemViewModel ttvm) {
                (args as MpCopyItemViewModel).IsSelected = true;
            }
        });

        public ICommand EditSelectedCopyItemCommand => new Command(() => {
            if (SelectedCopyItemViewModel != null) {
                SelectedCopyItemViewModel.ExpandCommand.Execute(null);
            }
        });

        public ICommand SetClipboardToSelectedCopyItemCommand => new Command(() => {
            if (SelectedCopyItemViewModel != null) {
                SelectedCopyItemViewModel.SetClipboardToItemCommand.Execute(null);
            }
        });
        #endregion
    }
}
