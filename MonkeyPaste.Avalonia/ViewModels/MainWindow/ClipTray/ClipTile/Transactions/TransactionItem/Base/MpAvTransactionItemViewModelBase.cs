using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvTransactionItemViewModelBase  : 
        MpViewModelBase<MpAvClipTileTransactionCollectionViewModel>,
        MpITransactionNodeViewModel {

        #region Interfaces

        #region MpITransactionNodeViewModel Implementation

        public object Body { get; }
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => null;
        public IEnumerable<MpITreeItemViewModel> Children => Items;
        public string LabelText => SourceLabel;
        public object ComparableSortValue => TransactionDateTimeUtc;
        public object IconSourceObj => SourceIconObj;

        object MpITransactionNodeViewModel.TransactionModel => Transaction;

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                if (Transaction == null) {
                    return new MpMenuItemViewModel();
                }
                return new MpMenuItemViewModel() {
                    IconSourceObj = SourceIconObj,
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
        
        public IEnumerable<MpAvClipTileTransactionItemMessageViewModelBase> Items {
            get {
                var items = new List<MpAvClipTileTransactionItemMessageViewModelBase>();
                if(Request != null) {
                    items.Add(Request);
                }
                if(Response != null) {
                    items.Add(Response);
                }
                return items;
            }
        }

        public MpAvClipTileTransactionItemMessageViewModelBase Request { get; set; }
        public MpAvClipTileTransactionItemMessageViewModelBase Response { get; set; }

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

        public MpJsonMessageFormatType RequestMessageType {
            get {
                if(Transaction == null) {
                    return MpJsonMessageFormatType.None;
                }
                return Transaction.RequestMessageType;
            }
        }
        public string RequestJson {
            get {
                if(Transaction == null) {
                    return string.Empty;
                }
                return Transaction.RequestMessageJson;
            }
        }
        public MpJsonMessageFormatType ResponseMessageType {
            get {
                if (Transaction == null) {
                    return MpJsonMessageFormatType.None;
                }
                return Transaction.ResponseMessageType;
            }
        }
        public string ResponseJson {
            get {
                if(Transaction == null) {
                    return string.Empty;
                }
                return Transaction.ResponseMessageJson;
            }
        }
        
        public object SourceIconObj {
            get {
                if(SourceRef is MpIIconResource dbi && dbi.IconResourceObj != null) {
                    return dbi.IconResourceObj;
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

        public int SourceObjId {
            get {
                if (SourceRef == null) {
                    return 0;
                }

                return SourceRef.SourceObjId;
            }
        }

        public MpCopyItemSourceType TransactionType {
            get {
                if(SourceRef == null) {
                    return MpCopyItemSourceType.None;
                }
                return SourceRef.SourceType;
            }
        }

        public DateTime TransactionDateTimeUtc {
            get {
                if (SourceRef == null) {
                    return DateTime.MaxValue;
                }
                return DateTime.MaxValue;
            }
        }

        public int TransactionId {
            get {
                if(Transaction == null) {
                    return 0;
                }
                return Transaction.Id;
            }
        }

        public MpISourceRef SourceRef { get; set; }

        public MpCopyItemTransaction Transaction { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvTransactionItemViewModelBase (MpAvClipTileTransactionCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpCopyItemTransaction cit) {
            IsBusy = true;

            //_sourceRef = null;
            //CopyItemSource = cis;
            Transaction = cit;
            
            var refs = await MpDataModelProvider.GetSourceRefsByCopyItemTransactionIdAsync(TransactionId);
            SourceRef = refs.FirstOrDefault();

            Request = await CreateMessageViewModel(RequestMessageType, RequestJson, this);
            Response = await CreateMessageViewModel(ResponseMessageType, ResponseJson, this);
            

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SourceRef));

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        public async Task<MpAvClipTileTransactionItemMessageViewModelBase> CreateMessageViewModel(MpJsonMessageFormatType jsonFormat, string json, MpITransactionNodeViewModel parentAnnotation) {
            if(string.IsNullOrEmpty(json)) {
                return null;
            }
            MpAvClipTileTransactionItemMessageViewModelBase cttimvmb = null;
            switch (jsonFormat) {
                case MpJsonMessageFormatType.DataObject:
                    cttimvmb = new MpAvDataObjectMessageViewModel(this);
                    break;
                default:
                    return null;
            }
            await cttimvmb.InitializeAsync(json, parentAnnotation);
            return cttimvmb;

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
