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
    public class MpClipTileCollectionPageViewModel : MpViewModelBase {
        #region Private Variables        
        private int _itemsAdded = 0;
        private int _currentStartIndex = 0;
        private int _pageSize = 20;
        
        #endregion

        #region Properties
        
        #region View Models
        public ObservableCollection<MpClipTileViewModel> ClipViewModels { get; set; }

        public MpClipTileViewModel SelectedClipViewModel { get; set; }
        #endregion

        public int TagId { get; set; }
        #endregion

        #region Public Methods
        public MpClipTileCollectionPageViewModel() : this(1) { }

        public MpClipTileCollectionPageViewModel(int tagId) : base() {
            PropertyChanged += MpClipCollectionViewModel_PropertyChanged;

            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            MpDb.Instance.OnItemUpdated += Db_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Db_OnItemDeleted;

            _ = Task.Run(() => Initialize(tagId));
        }

        public async Task SetTag(int tagId) {
            TagId = tagId;
            await MainThread.InvokeOnMainThreadAsync(async () => {
                ClipViewModels = new ObservableCollection<MpClipTileViewModel>();
                var clips = await MpClip.GetPage(TagId, 0, _pageSize);
                foreach(var c in clips) {
                    var ctvm = await CreateClipViewModel(c);
                    ClipViewModels.Add(ctvm);
                    ctvm.OnPropertyChanged(nameof(ctvm.IconImageSource));

                    MpSocketClient.Instance.SendMessage(c.ToString());
                }
                
                ClipViewModels.CollectionChanged += ClipViewModels_CollectionChanged;
                OnPropertyChanged(nameof(ClipViewModels));
            });
        }

        public async Task<MpClipTileViewModel> CreateClipViewModel(MpClip c) {
            MpClipTileViewModel ctvm = null;
            await Device.InvokeOnMainThreadAsync(async () => {
                MpApp app = await MpApp.GetAppById(c.AppId);
                app.Icon = await MpIcon.GetIconById(app.IconId);
                app.Icon.IconImage = await MpDbImage.GetDbImageById(app.Icon.IconImageId);
                c.App = app;

                var color = await MpColor.GetColorById(c.ColorId);
                if(color != null) {
                    c.ItemColor = color;
                }
                ctvm = new MpClipTileViewModel(c);

                ctvm.PropertyChanged += ClipViewModel_PropertyChanged;
                //Routing.RegisterRoute(@"Clipdetails/" + ctvm, typeof(MpClipDetailPageView));
            });
            
            return ctvm;
        }

        public void ClearSelection() {
            foreach (var civm in ClipViewModels) {
                civm.IsSelected = false;
            }
        }

        public void OnSearchQueryChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var civmsh = sender as MpClipTileViewModelSearchHandler;
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
        private async void MpClipCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedClipViewModel):
                    ClearSelection();
                    if(SelectedClipViewModel != null) {
                        SelectedClipViewModel.IsSelected = true;
                    }
                    break;
            }
        }

        private void SearchBarViewModel_SearchTextChanged(object sender, string e) {
            PerformSearchCommand.Execute(e);
        }

        private void Db_OnItemDeleted(object sender, MpDbModelBase e) {
            //throw new NotImplementedException();
        }

        private void Db_OnItemUpdated(object sender, MpDbModelBase e) {
            //throw new NotImplementedException();
        }

        private async void Db_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpClip nci) {
                var ctvm = await CreateClipViewModel(nci);
                ClipViewModels.Add(ctvm);
            }
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args) {
            foreach (MpClip Clip in args.NewItems) {
                _itemsAdded++;
                ClipViewModels.Add(CreateClipViewModel(Clip).Result);
            }
            if (_itemsAdded == _pageSize) {
                var collection = (ObservableCollection<MpClip>)sender;
                collection.CollectionChanged -= Collection_CollectionChanged;
            }
        }

        private void ClipViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null && e.NewItems.Count > 0) {
                IsBusy = false;
                ClipViewModels.CollectionChanged -= ClipViewModels_CollectionChanged;
            }
        }

        private async void ClipViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpClipTileViewModel civm) {
                switch (e.PropertyName) {
                    case nameof(civm.IsSelected):
                        if (civm.IsSelected) {
                            if (SelectedClipViewModel == civm) {
                                //implies selection came from ui do nothing
                            } else {
                                if (SelectedClipViewModel != null) {
                                    SelectedClipViewModel.IsSelected = false;
                                }
                                SelectedClipViewModel = civm;
                            }
                        } else {
                            if (SelectedClipViewModel == civm) {
                                SelectedClipViewModel = null;
                            }
                        }
                        break;
                }

                await MpDb.Instance.UpdateItem<MpClip>(civm.Clip);
            }
        }

       
        #endregion
        #endregion

        #region Commands
        public ICommand PerformSearchCommand => new Command<string>((string query) => {
            if(ClipViewModels == null) {
                return;
            }
            IEnumerable<MpClipTileViewModel> searchResult = null;
            if (string.IsNullOrEmpty(query)) {
                searchResult = ClipViewModels;
            } else {
                searchResult = from civm in ClipViewModels
                               where civm.Clip.ItemPlainText.ContainsByUserSensitivity(query)
                               select civm;//.Skip(2).Take(2);
            }
            foreach(var civm in ClipViewModels) {
                civm.IsVisible = searchResult.Contains(civm);
            }
        });

        public ICommand DeleteClipCommand => new Command<object>(async (args) => {
            if(args == null || args is not MpClipTileViewModel civm) {
                return;
            }
            ClipViewModels.Remove(civm);
            
            await MpDb.Instance.DeleteItem(civm.Clip);
            await MpClipTag.DeleteAllClipTagsForClipId(civm.Clip.Id);
        });

        public ICommand LoadMoreClipsCommand => new Command(async () => {
            _currentStartIndex += _pageSize;
            _itemsAdded = 0;
            var collection = await MpClip.GetPage(1, _currentStartIndex, _pageSize);
            collection.CollectionChanged += Collection_CollectionChanged;
        });

        #endregion
    }
}
