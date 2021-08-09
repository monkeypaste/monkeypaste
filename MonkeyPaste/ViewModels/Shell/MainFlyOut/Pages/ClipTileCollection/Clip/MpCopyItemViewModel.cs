using FFImageLoading.Forms;
using FFImageLoading.Helpers.Exif;
using Rg.Plugins.Popup.Services;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemViewModel : MpViewModelBase {
        #region Private Variables
        public event EventHandler ItemStatusChanged;
        private string _orgTitle = string.Empty;
        private double _collapsedHeight = 100;
        #endregion

        #region Properties

        #region Models
        public MpCopyItem CopyItem { get; set; }
        #endregion

        #region View Models
        public MpContextMenuViewModel ContextMenuViewModel { get; set; }
        #endregion

        public Func<string, Task<string>> EvaluateEditorJavaScript { get; set; }

        public event EventHandler OnEditorLoaded;

        public string EditorHtml { get; set; }

        public string Html { get; set; }
        public string Text { get; set; }
        public string Templates { get; set; }

        public MpJsProperty<bool> IsEditorLoaded { get; set; }

        public double EditorHeight { get; set; }

        public System.Timers.Timer UpdateTimer { get; set; }

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        public bool ShowLeftMenu {
            get {
                return !IsExpanded;
            }
        }

        public bool WasSetToClipboard { get; set; } = false;

        public bool HasTemplates {
            get {
                return CopyItem.Templates != null && CopyItem.Templates.Count > 0;
            }
        }

        public bool IsFavorite {
            get {
                if (CopyItem == null) {
                    return false;
                }
                var favTagList = MpDb.Instance.QueryAsync<MpTag>("select * from MpTag where TagName=?", "Favorites").Result;

                if (favTagList != null && favTagList.Count > 0) {
                    var result = MpDb.Instance.QueryAsync<MpCopyItemTag>("select * from MpCopyItemTag where CopyItemId=? and TagId=?", CopyItem.Id, favTagList[0].Id).Result;
                    return result != null && result.Count > 0;
                }
                return false;
            }
        }

        public ImageSource IconImageSource {
            get {
                if (CopyItem == null) {
                    return null;
                }
                return (StreamImageSource)new MpImageConverter().Convert(CopyItem.App.Icon.IconImage.ImageBase64, typeof(ImageSource));
            }
        }

        public bool IsTitleReadOnly { get; set; } = true;

        public bool IsVisible { get; set; } = true;
        #endregion

        #region Public Methods
        public MpCopyItemViewModel() { }

        public MpCopyItemViewModel(MpCopyItem item) {
            PropertyChanged += MpCopyItemViewModel_PropertyChanged;
            OnEditorLoaded += MpCopyItemViewModel_OnEditorLoaded;
            MpDb.Instance.OnItemUpdated += MpDb_OnItemUpdated;
            CopyItem = item;
            Routing.RegisterRoute("CopyItemdetails", typeof(MpCopyItemDetailPageView));
            Routing.RegisterRoute("CopyItemTagAssociations", typeof(MpCopyItemTagAssociationPageView));

            Task.Run(Initialize);
        }
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

            Device.BeginInvokeOnMainThread(() => {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpCopyItemDetailPageViewModel)).Assembly;
                var stream = assembly.GetManifestResourceStream("MonkeyPaste.Resources.Html.Editor.Editor2.html");
                using (var reader = new System.IO.StreamReader(stream)) {
                    var html = reader.ReadToEnd();
                    string contentTag = @"<div id='editor'>";
                    var data = CopyItem.ItemText;// string.IsNullOrEmpty(CopyItem.ItemHtml) ? CopyItem.ItemText : CopyItem.ItemHtml;
                    html = html.Replace(contentTag, contentTag + data);
                    EditorHtml = html;
                }
            });
        }

        private async Task<string> EvalJs(string js) {
            while(EvaluateEditorJavaScript == null) {
                await Task.Delay(100);
            }
            string result = string.Empty;
            try {
                result = await EvaluateEditorJavaScript(js);
            }
            catch(Exception ex) {
                MpConsole.WriteTraceLine("EvalJs exception:" + ex);
            }
            return result;
        }
        #region Event Handlers
        private void MpCopyItemViewModel_OnEditorLoaded(object sender, EventArgs e) {
            UpdateTimer = new System.Timers.Timer();
            UpdateTimer.Interval = 100;
            UpdateTimer.AutoReset = true;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer.Start();
        }

        private async void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            Text = await EvalJs($"getText()");
            Html = await EvalJs($"getHtml()");
            Templates = await EvalJs($"getTemplates()");
            var heightStr = await EvalJs($"getTotalHeight()");
            if(!IsExpanded || string.IsNullOrEmpty(heightStr) || heightStr == "null") {
                heightStr = _collapsedHeight.ToString();
            }
            EditorHeight = Convert.ToDouble(heightStr);
            MpConsole.WriteLine("Editor Height: " + heightStr + " " + EditorHeight);
        }

        private void MpCopyItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(CopyItem):
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
                        MpDb.Instance.AddOrUpdate<MpCopyItem>(CopyItem);
                    }
                    break;
                case nameof(EvaluateEditorJavaScript):
                    if(EvaluateEditorJavaScript != null) {
                        OnEditorLoaded?.Invoke(this, null);
                    }
                    break;
            }
        }

        private void JsProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsEditorLoaded):
                    UpdateTimer = new System.Timers.Timer();
                    UpdateTimer.Interval = 100;
                    UpdateTimer.AutoReset = true;
                    UpdateTimer.Elapsed += UpdateTimer_Elapsed;
                    UpdateTimer.Start();
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
                await MpDb.Instance.DeleteItemAsync<MpCopyItem>(CopyItem);
                await MpCopyItemTag.DeleteAllCopyItemTagsForCopyItemId(CopyItem.Id);
            });

        public ICommand ShowTagAssociationsCommand => new Command(() => {
            Device.InvokeOnMainThreadAsync(async () => {
                await Shell.Current.GoToAsync($"CopyItemTagAssociations?CopyItemId={CopyItem.Id}");
            });
            //await Navigation.PushModal(new MpCopyItemTagAssociationPageView(new MpCopyItemTagAssociationPageViewModel(CopyItem)));
            //await (Application.Current.MainPage.BindingContext as MpMainShellViewModel).TagCollectionViewModel.FavoritesTagViewModel.Tag.LinkWithCopyItemAsync(CopyItem.Id);
        });

        public ICommand RenameCopyItemCommand => new Command(async () => {
            _orgTitle = CopyItem.Title;
            if(IsExpanded) {
                IsTitleReadOnly = false;
            } else {
                var renamePopupPage = new MpRenamePopupPageView(_orgTitle);
                renamePopupPage.OnComplete += async (s, e) => {
                    if (renamePopupPage.WasCanceled) {
                        CopyItem.Title = _orgTitle;
                    } else if (e != _orgTitle) {
                        CopyItem.Title = e;
                        OnPropertyChanged(nameof(CopyItem));
                        MpDb.Instance.UpdateItem<MpCopyItem>(CopyItem);
                    }
                    await PopupNavigation.Instance.PopAllAsync();
                };
                await PopupNavigation.Instance.PushAsync(renamePopupPage, false);
            }            

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
                //Device.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync($"CopyItemdetails?CopyItemId={CopyItem.Id}&IsFillingOutTemplates=0"));
                IsExpanded = true;
                OnPropertyChanged(nameof(ShowLeftMenu));
                await EvalJs($"disableReadOnly()");
            },
            (arg)=>IsSelected);

        public ICommand UnexpandItemCommand => new Command(
            async () => {
                //Device.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync($"CopyItemdetails?CopyItemId={CopyItem.Id}&IsFillingOutTemplates=0"));
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
                await Clipboard.SetTextAsync(CopyItem.ItemText);         
            }

            WasSetToClipboard = true;
            ItemStatusChanged?.Invoke(this, new EventArgs());
        });
        #endregion
    }
}

