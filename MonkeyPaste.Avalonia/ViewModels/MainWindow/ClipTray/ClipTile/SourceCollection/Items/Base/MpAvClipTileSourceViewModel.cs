using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileSourceViewModel  : 
        MpViewModelBase<MpAvClipTileSourceCollectionViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpIMenuItemViewModel {
        #region Properties

        #region View Models

        #region MpIContextMenuItemViewModel Implementation

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

        #region State

        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

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
                return SourceLabel;
            }
        }

        public string SourceUri {
            get {
                if (SourceRef is MpIUriSource uris) {
                    return uris.Uri;
                } else if(SourceRef != null) {
                    // for copyitem's use localhost handle
                    return MpSourceRefHelper.ToUrl(SourceRef);
                }
                return string.Empty;
            }
        }

        public int SourceObjId {
            get {
                if (CopyItemSource == null) {
                    return 0;
                }

                return CopyItemSource.SourceObjId;
            }
        }

        public MpCopyItemSourceType SourceObjType {
            get {
                if(CopyItemSource == null) {
                    return MpCopyItemSourceType.None;
                }
                return CopyItemSource.CopyItemSourceType;
            }
        }

        public DateTime SourceCreatedDateTime {
            get {
                if (CopyItemSource == null) {
                    return DateTime.MaxValue;
                }
                return CopyItemSource.CreatedDateTime;
            }
        }

        public int SourceId {
            get {
                if(CopyItemSource == null) {
                    return 0;
                }
                return CopyItemSource.Id;
            }
        }

        private MpISourceRef _sourceRef;
        public MpISourceRef SourceRef { 
            get {
                if(_sourceRef == null) {
                    switch (SourceObjType) {
                        case MpCopyItemSourceType.App:
                            var avm = MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == SourceObjId);
                            if (avm == null) {
                                // where/what is it?
                                Debugger.Break();
                                break;
                            }
                            _sourceRef = avm.App;
                            break;
                        case MpCopyItemSourceType.Url:
                            var uvm = MpAvUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == SourceObjId);
                            if (uvm == null) {
                                // where/what is it?
                                Debugger.Break();
                                break;
                            }
                            _sourceRef = uvm.Url;
                            break;
                        case MpCopyItemSourceType.CopyItem:
                            var civm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == SourceObjId);
                            if (civm != null) {
                                _sourceRef = civm.CopyItem;
                            }
                            break;
                    }
                }
                return _sourceRef;
            }
        }
        public MpCopyItemSource CopyItemSource { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvClipTileSourceViewModel (MpAvClipTileSourceCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpCopyItemSource cis) {
            IsBusy = true;

            _sourceRef = null;
            CopyItemSource = cis;

            if(SourceRef == null && SourceObjType == MpCopyItemSourceType.CopyItem) {
                // ci not in tray so query (to avoid sync query)
                _sourceRef = await MpDataModelProvider.GetItemAsync<MpCopyItem>(SourceObjId);
            }

            OnPropertyChanged(nameof(SourceRef));

            IsBusy = false;
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
