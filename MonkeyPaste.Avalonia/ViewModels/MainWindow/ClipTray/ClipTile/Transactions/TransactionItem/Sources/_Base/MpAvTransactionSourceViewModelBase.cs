using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvTransactionSourceViewModel :
        MpViewModelBase<MpAvTransactionItemViewModel>,
        MpAvITransactionNodeViewModel {

        #region Interfaces

        #region MpIPlainTextViewModel Implementation

        public string PlainText {
            get {
                return "TEST source";
            }
        }
        #endregion

        #region MpAvITransactionNodeViewModel Implementation
        public MpAvClipTileViewModel HostClipTileViewModel =>
            Parent == null || Parent.Parent == null ?
                null :
                Parent.Parent.Parent;
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem { get; protected set; }
        public IEnumerable<MpITreeItemViewModel> Children => Items;
        public object ComparableSortValue => ParentTreeItem == null ? 0 : ParentTreeItem.Children.IndexOf(this);
        public virtual object IconSourceObj {
            get {
                if (SourceRef is MpIIconResource dbi && dbi.IconResourceObj != null) {
                    return dbi.IconResourceObj;
                }
                return MpDefaultDataModelTools.ThisAppIconId;
            }
        }
        public virtual string LabelText =>
           SourceRef == null ?
                "Unlabled Source Ref" :
                string.IsNullOrEmpty(SourceRef.LabelText) ?
                    "Untitled" :
                    SourceRef.LabelText;

        public virtual string Body =>
            SourceUri;

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                if (TransactionSource == null) {
                    return null;
                }
                var mivm = new MpMenuItemViewModel() {
                    ParentObj = this,
                    IconSourceObj = IconSourceObj,
                    Header = SourceLabel,
                    Tooltip = MpAvDateTimeToStringConverter.Instance.Convert(SourcedDateTimeUtc, null, MpAvDateTimeToStringConverter.LITERAL_DATE_TIME_FORMAT, null),
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = $"Browse to '{SourceLabel}'",
                            IconResourceKey = IsExternalSource ? SourceUri.StartsWith("http") ? "WebImage":"FolderImage" : "NewWindowImage",
                            AltNavIdx = 5,
                            Command = MpAvUriNavigator.Instance.NavigateToSourceRefCommand,
                            CommandParameter = SourceRef
                        }
                    }
                };
                if (IsExternalSource) {
                    mivm.SubItems.AddRange(new List<MpMenuItemViewModel>() {
                        //new MpMenuItemViewModel() {
                        //            Header = $"Filter by '{SourceLabel}'",
                        //            AltNavIdx = 0,
                        //            IconResourceKey = "FilterImage",
                        //            Command = EnableFilterByAppCommand
                        //        },
                        //new MpMenuItemViewModel() {
                        //            Header = SourceRef is MpUrl ? $"Filter by '{(SourceRef as MpUrl).UrlDomainPath}'":string.Empty,
                        //            IsVisible = SourceRef is MpUrl,
                        //            AltNavIdx = 0,
                        //            IconResourceKey = "FilterImage",
                        //            Command = EnableFilterByAppCommand,
                        //            CommandParameter = "DOMAIN"
                        //        },
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                                    Header = $"{(IsSourceRejected ? "Un-reject":"Reject")} '{SourceLabel}'",
                                    AltNavIdx = 0,
                                    IconResourceKey = IsSourceRejected ? "AddImage" : "NoEntryImage",
                                    Command = ExcludeSourceCommand
                                },

                    });
                }
                return mivm;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvITransactionNodeViewModel> Items { get; set; }


        #endregion

        #region Appearance

        public string DetailUriLabel {
            get {
                if (SourceRef is MpApp) {
                    return "Show Folder";
                }

                return "Open";
            }
        }

        #endregion

        #region State
        public bool IsAnyBusy =>
            IsBusy ||
            (Items != null && Items.Cast<MpAvTransactionSourceViewModel>().Any(x => x.IsAnyBusy));
        public bool IsHovering { get; set; }

        public bool IsSelected {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.SelectedSource == this;
            }
            set {
                if (value && Parent != null && IsSelected != value) {
                    Parent.SelectedSource = this;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public DateTime LastSelectedDateTime { get; set; }

        public bool IsSourceRejected =>
            Mp.Services.SourceRefTools.IsSourceRejected(SourceRef);

        public bool IsExternalSource =>
            Mp.Services.SourceRefTools.IsExternalSource(SourceRef);

        #endregion

        #region Model

        #region MpISourceRef Sub-Model

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
                    return Mp.Services.SourceRefTools.ConvertToInternalUrl(SourceRef);
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
        //public string SourceArg {
        //    get {
        //        if (TransactionSource == null) {
        //            return string.Empty;
        //        }

        //        return TransactionSource.SourceArgs;
        //    }
        //}
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

        public MpAvTransactionSourceViewModel(MpAvTransactionItemViewModel parent) : base(parent) {
            //PropertyChanged += MpAvTransactionSourceViewModelBase_PropertyChanged;
        }


        #endregion

        #region Public Methods
        public async virtual Task InitializeAsync(MpTransactionSource ts) {
            bool wasBusy = IsBusy;
            IsBusy = true;

            TransactionSource = ts;

            SourceRef = await MpDataModelProvider.GetSourceRefBySourceTypeAndSourceIdAsync(SourceType, SourceObjId);

            IsBusy = wasBusy;
        }

        #endregion

        #region Private Methods
        private void MpAvTransactionSourceViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.FocusNode));
                    break;
            }
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
        public ICommand ExcludeSourceCommand => new MpCommand<object>(
             (args) => {
                 if (SourceRef is MpApp app &&
                    MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == app.Id) is MpAvAppViewModel avm) {
                     MpAvAppCollectionViewModel.Instance.ToggleIsRejectedCommand.Execute(avm);
                     return;
                 }
                 if (SourceRef is MpUrl url &&
                    MpAvUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == url.Id) is MpAvUrlViewModel uvm) {
                     if (args != null) {
                         // domain reject
                         uvm.ToggleIsDomainRejectedCommand.Execute(null);
                     } else {
                         uvm.ToggleIsRejectedCommand.Execute(null);
                     }
                     return;
                 }
             });


        #endregion
    }
}
