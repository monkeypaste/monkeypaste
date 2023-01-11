using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileSourceViewModel  : 
        MpViewModelBase<MpAvClipTileTransactionCollectionViewModel>,
        MpITransactionNodeViewModel {

        #region Interfaces

        #region MpITransactionNodeViewModel Implementation

        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => null;
        public IEnumerable<MpITreeItemViewModel> Children => Items;
        public string LabelText => SourceLabel;
        public object ComparableSortValue => SourceCreatedDateTime;
        public object IconSourceObj => SourceIconId;

        object MpITransactionNodeViewModel.TransactionModel => SourceRef;

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                if (SourceRef == null) {
                    return new MpMenuItemViewModel();
                }
                return new MpMenuItemViewModel() {
                    IconId = SourceIconId,
                    Header = SourceLabel,
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                                    Header = $"Filter by '{SourceLabel}'",
                                    AltNavIdx = 0,
                                    IconResourceKey = "FilterImage",
                                    Command = EnableFilterByAppCommand
                                },
                        new MpMenuItemViewModel() {
                                    Header = $"Exclude '{SourceLabel}'",
                                    AltNavIdx = 0,
                                    IconResourceKey = "NoEntryImage",
                                    Command = ExcludeSourceCommand
                                },
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                            Header = $"Goto '{SourceUri}'",
                            AltNavIdx = 5,
                            IconResourceKey = "Execute",
                            Command = GotoSourceCommand
                        }
                    }
                };
            }
        }

        #endregion

        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpITransactionNodeViewModel> Items { get; set; }

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || (Items != null && Items.Any(x => x.IsAnyBusy));
        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        public int HostCopyItemId {
            get {
                if(Parent == null || Parent.Parent == null) {
                    return 0;
                }
                return Parent.Parent.CopyItemId;
            }
        }
        #endregion

        #region Model
        
        public int SourceIconId {
            get {
                if(SourceRef is MpIDbIconId dbi && dbi.IconId > 0) {
                    return dbi.IconId;
                }
                return MpDefaultDataModelTools.ThisAppIconId;
            }
        }

        public string SourceLabel {
            get {
                if (SourceRef is MpILabelText lbt) {
                    return lbt.LabelText;
                }
                return string.Empty;
            }
        }

        public string SourceUri {
            get {
                if (SourceRef is MpIUriSource uris) {
                    return uris.Uri;
                } else if(SourceRef != null) {
                    // for copyitem's use localhost handle
                    return MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(SourceRef);
                }
                return string.Empty;
            }
        }
        public int SourcePriority {
            get {
                if(SourceRef == null) {
                    return 0;
                }
                return SourceRef.Priority;
            }
        }
        public int SourceObjId {
            get {
                if (SourceRef == null) {
                    return 0;
                }

                return SourceRef.SourceObjId;
            }
        }

        public MpCopyItemSourceType SourceObjType {
            get {
                if(SourceRef == null) {
                    return MpCopyItemSourceType.None;
                }
                return SourceRef.SourceType;
            }
        }

        public DateTime SourceCreatedDateTime {
            get {
                if (SourceRef == null) {
                    return DateTime.MaxValue;
                }
                return DateTime.MaxValue;
            }
        }

        public int SourceId {
            get {
                if(SourceRef is MpDbModelBase dbmb) {
                    return dbmb.Id;
                }
                return 0;
            }
        }

        //private MpISourceRef _sourceRef;
        //public MpISourceRef SourceRef { 
        //    get {
        //        if(_sourceRef == null) {
        //            switch (SourceObjType) {
        //                case MpCopyItemSourceType.App:
        //                    var avm = MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == SourceObjId);
        //                    if (avm == null) {
        //                        // where/what is it?
        //                        Debugger.Break();
        //                        break;
        //                    }
        //                    _sourceRef = avm.App;
        //                    break;
        //                case MpCopyItemSourceType.Url:
        //                    var uvm = MpAvUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == SourceObjId);
        //                    if (uvm == null) {
        //                        // where/what is it?
        //                        Debugger.Break();
        //                        break;
        //                    }
        //                    _sourceRef = uvm.Url;
        //                    break;
        //                case MpCopyItemSourceType.Analyzer:
        //                    var aipvm = MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == SourceObjId);
        //                    if (aipvm == null) {
        //                        // where/what is it?
        //                        Debugger.Break();
        //                        break;
        //                    }
        //                    _sourceRef = aipvm.Transactions.FirstOrDefault(x=>x.Co;
        //                    break;
        //                case MpCopyItemSourceType.CopyItem:
        //                    var civm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == SourceObjId);
        //                    if (civm != null) {
        //                        _sourceRef = civm.CopyItem;
        //                    }
        //                    break;
        //            }
        //        }
        //        return _sourceRef;
        //    }
        //}
        public MpISourceRef SourceRef { get; set; }
        //public MpCopyItemSource CopyItemSource { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvClipTileSourceViewModel (MpAvClipTileTransactionCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpISourceRef sourceRef, MpCopyItemTransaction cit = null) {
            IsBusy = true;

            //_sourceRef = null;
            //CopyItemSource = cis;
            SourceRef = sourceRef;

            if(Items != null) {
                Items.Clear();
            }
            if(SourceRef == null && SourceObjType == MpCopyItemSourceType.CopyItem) {
                // ci not in tray so query (to avoid sync query)
                SourceRef = await MpDataModelProvider.GetItemAsync<MpCopyItem>(SourceObjId);
            }
            if(SourceRef is MpDllTransaction dllt) {
                MpAnalyzerPluginRequestFormat req_params = null;
                try {
                    req_params = MpJsonObject.DeserializeObject<MpAnalyzerPluginRequestFormat>(dllt.Args);
                }catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Error converting params: {dllt.Args}", ex);
                }
                MpAnnotationNodeFormat root_annotation = null;
                if(cit != null) {
                    try {
                        root_annotation = MpJsonObject.DeserializeObject<MpAnnotationNodeFormat>(cit.ResponseJson);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error converting params: {dllt.Args}", ex);
                    }
                }

                if(root_annotation != null) {
                    var root_avm = await CreateAnnotationViewModel(root_annotation, null);
                    if(root_avm != null) {
                        if(Items == null) {
                            Items = new ObservableCollection<MpITransactionNodeViewModel>();
                        }
                        Items.Add(root_avm);

                    }
                }
            }

            if (Items != null) {

                while (Items.Any(x => x.IsAnyBusy)) {
                    await Task.Delay(100);
                }
            }

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SourceRef));

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        public async Task<MpAvClipTileAnnotationViewModel> CreateAnnotationViewModel(MpAnnotationNodeFormat anf, MpITreeItemViewModel pti) {
            var anvm = new MpAvClipTileAnnotationViewModel(this);
            await anvm.InitializeAsync(anf, pti);
            return anvm;

        }
        #endregion

        #region Commands
        public ICommand EnableFilterByAppCommand => new MpCommand<object>(
            (ctvmSourceItemVm) => {
                // TODO add query filter stuff here from source
                //var targetCtvm = ctvmSourceItemVm as MpAvClipTileViewModel;
                //if (targetCtvm == null) {
                //    return;
                //}

                //MpHelpers.OpenUrl(CopyItem.Source.App.AppPath);
                //ClearClipSelection();
                //targetCtvm.IsSelected = true;
                //this triggers clip tray to swap out the app icons for the filtered app
                //MpClipTrayViewModel.Instance.FilterByAppIcon = ctvm.CopyItem.Source.PrimarySource.SourceIcon.IconImage.ImageBase64.ToBitmapSource();
                //IsFilteringByApp = true;
            });
        public ICommand ExcludeSourceCommand => new MpCommand(
             () => {
                //var avm = MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == SelectedItem.AppViewModel.AppId);
                //if (avm == null) {
                //    return;
                //}
                //await avm.RejectApp();
            });

        public ICommand ExcludeSubSelectedItemUrlDomainCommand => new MpCommand(
            () => {
                //var uvm = MpAvUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == SelectedItem.UrlViewModel.UrlId);
                //if (uvm == null) {
                //    MpConsole.WriteTraceLine("Error cannot find url id: " + SelectedItem.UrlViewModel.UrlId);
                //    return;
                //}
                //await uvm.RejectUrlOrDomain(true);
            });

        public ICommand GotoSourceCommand => new MpCommand(
            () => {
                // open uri here]
                MpAvUriNavigator.NavigateToUri(new Uri(SourceUri));
            },()=>Uri.IsWellFormedUriString(SourceUri, UriKind.Absolute));

        #endregion
    }
}
