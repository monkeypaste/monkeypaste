using FFImageLoading.Forms;
using FFImageLoading.Helpers.Exif;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpCopyItemViewModel : MpViewModelBase {
        #region Private Variables
        public event EventHandler ItemStatusChanged;
        private string _orgTitle = string.Empty;
        private const double _collapsedHeight = 100;
        #endregion

        #region Properties

        #region Models
        public MpCopyItem CopyItem { get; set; }
        #endregion

        #region View Models
        public MpContextMenuViewModel ContextMenuViewModel { get; set; }

        //public ObservableCollection<MpTemp>
        #endregion

        public Func<string, Task<string>> EvaluateEditorJavaScript { get; set; }

        public event EventHandler OnEditorLoaded;

        public string EditorHtml { get; set; }

        public string edHtml { get; set; }
        public string edText { get; set; }
        public string edTemplates { get; set; }
        public string edConsoleLog { get; set; }

        public double EditorHeight { get; set; } = _collapsedHeight;

        public System.Timers.Timer UpdateTimer { get; set; }

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        public bool IsTitleVisible {
            get {
                return true;

                //if (CopyItem == null) {
                //    return false;
                //}
                //return IsExpanded || CopyItem.Title != "Untitled";
            }
        }

        public bool ShowLeftMenu {
            get {
                return !IsExpanded;
            }
        }

        public bool WasSetToClipboard { get; set; } = false;

        public bool HasTemplates {
            get {
                return false;
            }
        }

        public bool IsFavorite {
            get {
                if (CopyItem == null) {
                    return false;
                }
                var favTagList = MpDb.QueryAsync<MpTag>("select * from MpTag where TagName=?", "Favorites").Result;

                if (favTagList != null && favTagList.Count > 0) {
                    var result = MpDb.QueryAsync<MpCopyItemTag>("select * from MpCopyItemTag where CopyItemId=? and TagId=?", CopyItem.Id, favTagList[0].Id).Result;
                    return result != null && result.Count > 0;
                }
                return false;
            }
        }

        //public MpApp App {
        //    get {
        //        if(CopyItem == null || CopyItem.Source == null || CopyItem.Source.App == null) {
        //            return null;
        //        }
        //        return CopyItem.Source.App;
        //    }
        //}

        public ImageSource IconImageSource {
            get {
                if (CopyItem == null || CopyItem.SourceId <= 0) {
                    return null;
                }
                var s = MpDb.GetItem<MpSource>(CopyItem.SourceId);
                var imgStr = MpDb.GetItem<MpDbImage>(
                                MpDb.GetItem<MpIcon>(
                                   s.IsUrlPrimarySource ?
                                        MpDb.GetItem<MpUrl>(s.PrimarySourceId).IconId :
                                        MpDb.GetItem<MpApp>(s.PrimarySourceId).IconId).IconImageId);

                return (StreamImageSource)new MpImageConverter().Convert(imgStr, typeof(ImageSource));
            }
        }

        public bool IsTitleReadOnly { get; set; } = true;

        public bool IsVisible { get; set; } = true;
        #endregion

        #region Constructors

        public MpCopyItemViewModel() : base(){ }

        public MpCopyItemViewModel(MpCopyItem item) : base() {
            PropertyChanged += MpCopyItemViewModel_PropertyChanged;
            OnEditorLoaded += MpCopyItemViewModel_OnEditorLoaded;
            MpDb.OnItemUpdated += MpDb_OnItemUpdated;
            CopyItem = item;
            //Routing.RegisterRoute("CopyItemdetails", typeof(MpCopyItemDetailPageView));
            Routing.RegisterRoute("CopyItemTagAssociations", typeof(MpCopyItemTagAssociationPageView));

            Device.BeginInvokeOnMainThread(Initialize);

            //Initialize();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        private void Initialize() {
            ContextMenuViewModel = new MpContextMenuViewModel();
            ContextMenuViewModel.Items.Add(new MpContextMenuItemViewModel() {
                Title = "Change Tags",
                Command = ShowTagAssociationsCommand,
                IconImageResourceName = "StarOutlineIcon"
            });
            ContextMenuViewModel.Items.Add(new MpContextMenuItemViewModel() {
                Title = "Rename",
                Command = RenameCopyItemCommand,
                IconImageResourceName = "EditIcon"
            });
            ContextMenuViewModel.Items.Add(new MpContextMenuItemViewModel() {
                Title = "Delete",
                Command = DeleteCopyItemCommand,
                IconImageResourceName = "DeleteIcon"
            });
            InitEditor();
        }

        private void InitEditor() {
            var html = MpHelpers.LoadTextResource("MonkeyPaste.Resources.Html.Editor.index.html");
            
            string contentTag = @"<div id='editor'>";
            var data = CopyItem.ItemData; //string.IsNullOrEmpty(CopyItem.ItemHtml) ? CopyItem.ItemText : CopyItem.ItemHtml;
            html = html.Replace(contentTag, contentTag + data);

            string envTag = @"var envName = '';";
            string envVal = @"var envName = 'android';";
            html = html.Replace(envTag, envVal);

            EditorHtml = html;
        }
                

        private async Task<string> EvalJs(string js) {
            while(EvaluateEditorJavaScript == null) {
                await Task.Delay(100);
            }
            string result = string.Empty;
            await Device.InvokeOnMainThreadAsync(async () => {
                try {
                    result = await EvaluateEditorJavaScript(js);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("EvalJs exception:" + ex);
                }
            });
            return result;
        }
        #region Event Handlers
        private void MpCopyItemViewModel_OnEditorLoaded(object sender, EventArgs e) {
            UpdateTimer = new System.Timers.Timer();
            UpdateTimer.Interval = 100;
            UpdateTimer.AutoReset = true;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private async void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            //var nl = await EvalJs($"getLog()");
            //if(nl != edConsoleLog) {
            //    edConsoleLog = edConsoleLog.Replace(edConsoleLog, nl);
            //    MpConsole.WriteLine(edConsoleLog);
            //}

            //edText = await EvalJs($"getText()");
            //edHtml = await EvalJs($"getHtml()");
            //edTemplates = await EvalJs($"getTemplates()");

            var heightStr = await EvalJs($"getTotalHeight()");
            if(!IsExpanded || string.IsNullOrEmpty(heightStr) || heightStr == "null") {
                heightStr = _collapsedHeight.ToString();
            }
            EditorHeight = Math.Max(_collapsedHeight,Convert.ToDouble(heightStr));
            //MpConsole.WriteLine("Editor Height: " + heightStr + " " + EditorHeight);
        }

        private void MpCopyItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(CopyItem):
                    break;
                case nameof(IsExpanded):
                    OnPropertyChanged(nameof(IsTitleVisible));
                    if(IsExpanded) {
                        IsTitleReadOnly = false;
                        UpdateTimer.Start();
                    } else {
                        IsTitleReadOnly = true;
                        UpdateTimer.Stop();
                    }
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        //Device.InvokeOnMainThreadAsync(async () => {
                        //    await Shell.Current.GoToAsync($"CopyItemdetails?CopyItemId={CopyItem.Id}");
                        //});
                    } else {
                        //WasSetToClipboard = false;
                        UnexpandItemCommand.Execute(null);
                    }
                    break;
                case nameof(IsTitleReadOnly):
                    if (IsTitleReadOnly && CopyItem.Title != _orgTitle) {
                        _orgTitle = CopyItem.Title;
                        Task.Run(async()=>{
                            await CopyItem.WriteToDatabaseAsync();
                        });
                        
                    }
                    break;
                case nameof(EvaluateEditorJavaScript):
                    if(EvaluateEditorJavaScript != null) {
                        OnEditorLoaded?.Invoke(this, null);
                    }
                    break;
            }
        }

        private void MpDb_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if (ci.Id == CopyItem.Id) {
                    CopyItem = ci;
                    //await Initialize();
                    //OnPropertyChanged(nameof(IconImageSource));
                }
            }
        }
        #endregion

        #endregion

        #region Commands
        public ICommand DeleteCopyItemCommand => new Command(
            async () => {
                await CopyItem.DeleteFromDatabaseAsync();
            });

        public ICommand ShowTagAssociationsCommand => new Command(() => {
            Device.InvokeOnMainThreadAsync(async () => {
                await Shell.Current.GoToAsync($"CopyItemTagAssociations?CopyItemId={CopyItem.Id}");
            });
            //await Navigation.PushModal(new MpCopyItemTagAssociationPageView(new MpCopyItemTagAssociationPageViewModel(CopyItem)));
            //await (Application.Current.MainPage.BindingContext as MpMainShellViewModel).TagCollectionViewModel.FavoritesTagViewModel.Tag.LinkWithCopyItemAsync(CopyItem.Id);
        });

        public ICommand RenameCopyItemCommand => new Command<object>(
            async (args) => {
            if(!IsExpanded && args != null) {
                return;
            }
            _orgTitle = CopyItem.Title;
            var renamePopupPage = new MpRenamePopupPageView(_orgTitle);
            renamePopupPage.OnComplete += async (s, e) => {
                if (renamePopupPage.WasCanceled) {
                    CopyItem.Title = _orgTitle;
                } else if (e != _orgTitle) {
                    CopyItem.Title = e;
                    OnPropertyChanged(nameof(CopyItem));
                    await CopyItem.WriteToDatabaseAsync();
                }

                if (string.IsNullOrEmpty(CopyItem.Title)) {
                    CopyItem.Title = "Untitled";
                    OnPropertyChanged(nameof(CopyItem));
                    await CopyItem.WriteToDatabaseAsync();
                }

                await PopupNavigation.Instance.PopAllAsync();

                OnPropertyChanged(nameof(IsTitleVisible));
            };
            await PopupNavigation.Instance.PushAsync(renamePopupPage, false);
        });

        public ICommand SelectCopyItemCommand => new Command(
            () => {
                if(IsSelected) {
                    ExpandCommand.Execute(null);
                } else {
                    IsSelected = true;
                }
            },
            ()=>IsVisible);

        public ICommand ExpandCommand => new Command<object>(
            async (arg) => {
                MpCopyItemTileCollectionPageViewModel.IsAnyItemExpanded = true;
                IsExpanded = true;
                OnPropertyChanged(nameof(ShowLeftMenu));
                await EvalJs($"disableReadOnly()");
            },
            (arg)=>IsSelected);

        public ICommand UnexpandItemCommand => new Command(
            async () => {
                MpCopyItemTileCollectionPageViewModel.IsAnyItemExpanded = false;
                IsExpanded = false;
                OnPropertyChanged(nameof(ShowLeftMenu));

                await EvalJs($"enableReadOnly()");
            });

        public ICommand FillOutTemplatesCommand => new Command(
            () => {
                Device.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync($"CopyItemdetails?CopyItemId={CopyItem.Id}&IsFillingOutTemplates=1"));
            },
            () => IsSelected);


        public ICommand SetClipboardToItemCommand => new Command(async () => {
            if(HasTemplates) {
                FillOutTemplatesCommand.Execute(null);
            } else {
                await Clipboard.SetTextAsync(CopyItem.ItemData);         
            }
            
            WasSetToClipboard = true;
            ItemStatusChanged?.Invoke(this, new EventArgs());
        });
        #endregion
    }
}

