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

        #region Properties
        
        #region View Models
        public ObservableCollection<MpCopyItemViewModel> CopyItemViewModels { get; set; }

        public MpCopyItemViewModel SelectedCopyItemViewModel { get; set; }
        #endregion

        public string EmptyCollectionLableText { get; set; }

        public int TagId { get; set; }
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
            await MainThread.InvokeOnMainThreadAsync(async () => {
                CopyItemViewModels = new ObservableCollection<MpCopyItemViewModel>();
                var clips = await MpCopyItem.GetPage(TagId, 0, _pageSize);
                foreach(var c in clips) {
                    var ctvm = await CreateCopyItemViewModel(c);
                    CopyItemViewModels.Add(ctvm);
                    ctvm.OnPropertyChanged(nameof(ctvm.IconImageSource));
                }
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

        public async Task<MpCopyItemViewModel> CreateCopyItemViewModel(MpCopyItem c) {
            MpCopyItemViewModel ctvm = null;
            await Device.InvokeOnMainThreadAsync(async () => {
                MpApp app = await MpApp.GetAppById(c.AppId);
                app.Icon = await MpIcon.GetIconById(app.IconId);
                app.Icon.IconImage = await MpDbImage.GetDbImageById(app.Icon.IconImageId);
                c.App = app;

                var color = await MpColor.GetColorByIdAsync(c.ColorId);
                if(color != null) {
                    c.ItemColor = color;
                }
                ctvm = new MpCopyItemViewModel(c);

                ctvm.PropertyChanged += CopyItemViewModel_PropertyChanged;
                //Routing.RegisterRoute(@"CopyItemdetails/" + ctvm, typeof(MpCopyItemDetailPageView));
            });
            
            return ctvm;
        }

        public void ClearSelection() {
            foreach (var civm in CopyItemViewModels) {
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
                    var scivm = SelectedCopyItemViewModel;
                    ClearSelection();
                    if(scivm != null) {
                        SelectedCopyItemViewModel = scivm;
                    }
                    break;
            }
        }

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

        private async void Db_OnItemUpdated(object sender, MpDbModelBase e) {
            //if (e is MpCopyItem uci) {
            //    var uctvm = await CreateCopyItemViewModel(uci);
            //    var ctvm = CopyItemViewModels.Where(x => x.CopyItem.Id == uci.Id).FirstOrDefault();
            //    if(ctvm != null) {
            //        CopyItemViewModels[CopyItemViewModels.IndexOf(ctvm)] = uctvm;
            //    }
                
            //}
        }

        private async void Db_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpCopyItem nci) {
                var ctvm = await CreateCopyItemViewModel(nci);
                CopyItemViewModels.Add(ctvm);
            }
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args) {
            foreach (MpCopyItem CopyItem in args.NewItems) {
                _itemsAdded++;
                CopyItemViewModels.Add(CreateCopyItemViewModel(CopyItem).Result);
            }
            if (_itemsAdded == _pageSize) {
                var collection = (ObservableCollection<MpCopyItem>)sender;
                collection.CollectionChanged -= Collection_CollectionChanged;
            }
        }

        private void CopyItemViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null && e.NewItems.Count > 0) {
                IsBusy = false;
                CopyItemViewModels.CollectionChanged -= CopyItemViewModels_CollectionChanged;
            }
        }

        private async void CopyItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpCopyItemViewModel civm) {
                switch (e.PropertyName) {
                    case nameof(civm.IsSelected):
                        if (civm.IsSelected) {
                            if (SelectedCopyItemViewModel == civm) {
                                //implies selection came from ui do nothing
                                //SelectedCopyItemViewModel = civm;
                            } else {
                                if (SelectedCopyItemViewModel != null) {
                                    SelectedCopyItemViewModel.IsSelected = false;
                                }
                                SelectedCopyItemViewModel = civm;
                            }
                        } else {
                            if (SelectedCopyItemViewModel == civm) {
                                SelectedCopyItemViewModel = null;
                            }
                        }
                        break;
                }

                await MpDb.Instance.UpdateItemAsync<MpCopyItem>(civm.CopyItem);
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
            _currentStartIndex += _pageSize;
            _itemsAdded = 0;
            var collection = await MpCopyItem.GetPage(1, _currentStartIndex, _pageSize);
            collection.CollectionChanged += Collection_CollectionChanged;
        });

        public ICommand SelectionChangedCommand => new Command<object>((args) => {
            if (args != null && args is MpCopyItemViewModel ttvm) {
                SelectedCopyItemViewModel = args as MpCopyItemViewModel;
            }
        });
        #endregion
    }
}
