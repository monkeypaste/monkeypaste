using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvTransactionSourceViewModelBase  : 
        MpViewModelBase<MpAvTransactionItemViewModel>,
        MpITransactionNodeViewModel {

        #region Interfaces

        #region MpITransactionNodeViewModel Implementation
        public virtual object Body { get; }
        object MpITransactionNodeViewModel.TransactionModel => Parent.Transaction;
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem { get; protected set; }
        public IEnumerable<MpITreeItemViewModel> Children => Items;
        public virtual string LabelText =>
            SourceRef == null ? "Unlabled Source Ref" : SourceRef.LabelText;
        public object ComparableSortValue => ParentTreeItem == null ? 0 : ParentTreeItem.Children.IndexOf(this);
        public virtual object IconSourceObj => SourceIconObj;

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                if (TransactionSource == null) {
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
        public ObservableCollection<MpITransactionNodeViewModel> Items { get; set; }


        #endregion

        #region State
        public bool IsAnyBusy => 
            IsBusy || 
            (Items != null && Items.Cast<MpAvTransactionSourceViewModelBase>().Any(x => x.IsAnyBusy));
        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Model

        #region MpISourceRef Sub-Model
        public object SourceIconObj {
            get {
                if (SourceRef is MpIIconResource dbi && dbi.IconResourceObj != null) {
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
                } else if (SourceRef != null) {
                    // for copyitem's use localhost handle
                    return MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(SourceRef);
                }
                return string.Empty;
            }
        }

        public MpISourceRef SourceRef { get; set; }
        #endregion

        #region MpTransactionSource Model

        public MpTransactionSourceType SourceType {
            get {
                if (TransactionSource == null) {
                    return MpTransactionSourceType.None;
                }
                return TransactionSource.CopyItemSourceType;
            }
        }

        public DateTime SourcedDateTimeUtc {
            get {
                if (TransactionSource == null) {
                    return DateTime.MinValue;
                }
                return TransactionSource.TransactionDateTime;
            }
        }

        public int SourceObjId {
            get {
                if (TransactionSource == null) {
                    return 0;
                }

                return TransactionSource.SourceObjId;
            }
        }
        public string SourceArg {
            get {
                if (TransactionSource == null) {
                    return string.Empty;
                }

                return TransactionSource.SourceArgs;
            }
        }
        public int TransactionSourceId {
            get {
                if (TransactionSource == null) {
                    return 0;
                }

                return TransactionSource.Id;
            }
        }
        public MpTransactionSource TransactionSource { get; set; }

        #endregion
        #endregion

        #endregion

        #region Constructors

        public MpAvTransactionSourceViewModelBase(MpAvTransactionItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public async virtual Task InitializeAsync(MpTransactionSource ts) {
            bool wasBusy = IsBusy;
            IsBusy = true;

            TransactionSource = ts;

            SourceRef = await MpDataModelProvider.GetSourceRefBySourceypeAndSourceIdAsync(SourceType, SourceObjId);

            IsBusy = wasBusy;
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
            }, () => Uri.IsWellFormedUriString(SourceUri, UriKind.Absolute));
        #endregion
    }
}
