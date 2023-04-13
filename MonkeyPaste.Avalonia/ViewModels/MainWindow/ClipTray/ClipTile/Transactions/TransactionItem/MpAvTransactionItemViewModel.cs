using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public enum MpTransactionTabType {
        Source,
        Request,
        Response
    }
    public class MpAvTransactionItemViewModel :
        MpViewModelBase<MpAvClipTileTransactionCollectionViewModel>,
        MpAvITransactionNodeViewModel {
        #region Private Variables
        private bool _isBringingIntoView = false;
        #endregion

        #region Interfaces

        #region MpAvITransactionNodeViewModel Implementation
        public MpAvClipTileViewModel HostClipTileViewModel =>
            Parent == null ?
                null :
                Parent.Parent;
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => null;
        public IEnumerable<MpITreeItemViewModel> Children =>
            Items;
        public string Body => $"{LabelText} on {TransactionDateTime}";
        public string LabelText =>
            Transaction == null ? TransactionIdx.ToString() : Transaction.TransactionLabel;
        public object ComparableSortValue => TransactionDateTime;
        public object IconSourceObj {
            get {
                if (Sources.Count() > 0) {
                    return Sources.OrderBy(x => x.SourcedDateTimeUtc).FirstOrDefault().IconSourceObj;
                }
                return MpDefaultDataModelTools.ThisAppIconId;
            }
        }


        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                if (Transaction == null) {
                    return new MpMenuItemViewModel();
                }
                return new MpMenuItemViewModel() {
                    IconSourceObj = IconSourceObj,
                    Header = TransactionLabel,
                    //SubItems = new List<MpMenuItemViewModel>() {
                    //    new MpMenuItemViewModel() {
                    //                Header = $"Filter by '{TransactionLabel}'",
                    //                AltNavIdx = 0,
                    //                IconResourceKey = "FilterImage",
                    //                Command = EnableFilterByAppCommand
                    //            },
                    //    new MpMenuItemViewModel() {
                    //                Header = $"Exclude '{TransactionLabel}'",
                    //                AltNavIdx = 0,
                    //                IconResourceKey = "NoEntryImage",
                    //                Command = ExcludeSourceCommand
                    //            },
                    //    new MpMenuItemViewModel() {IsSeparator = true},
                    //    new MpMenuItemViewModel() {
                    //        Header = $"Goto '{SourceUri}'",
                    //        AltNavIdx = 5,
                    //        IconResourceKey = "Execute",
                    //        Command = GotoSourceCommand
                    //    }
                    //}
                };
            }
        }

        #endregion

        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvITransactionNodeViewModel> Items { get; private set; } = new ObservableCollection<MpAvITransactionNodeViewModel>();

        public IEnumerable<MpAvTransactionSourceViewModelBase> Sources =>
            Items.OfType<MpAvTransactionSourceViewModelBase>().ToList();
        public IEnumerable<MpAvTransactionMessageViewModelBase> Messages =>

            Items.OfType<MpAvTransactionMessageViewModelBase>();

        public MpAvTransactionMessageViewModelBase Request =>
            Messages
                .FirstOrDefault(x => x.TransactionMessageType == MpTransactionMessageType.Request);
        public MpAvTransactionMessageViewModelBase Response =>
            Messages
                .FirstOrDefault(x => x.TransactionMessageType == MpTransactionMessageType.Response);

        public MpAvDataObjectViewModel DataObjectViewModel { get; private set; }

        public MpAvITransactionNodeViewModel FocusNode {
            get {
                // select deepest selected node
                var selected_desc =
                     this.AllDescendants()
                    .OrderByDescending(x => x.AllAncestors().Count())
                    .Cast<MpISelectableViewModel>()
                    .FirstOrDefault(x => x.IsSelected);

                if (selected_desc is MpAvITransactionNodeViewModel tnvm) {
                    return tnvm;
                }
                return this;
            }
        }


        public MpAvTransactionSourceViewModelBase SelectedSource { get; set; }
        #endregion

        #region Appearance

        #endregion

        #region State

        public int FocusNodeTabIdx {
            get {
                IEnumerable<MpITreeItemViewModel> test = null;
                if (FocusNode != null) {
                    test = FocusNode.SelfAndAllAncestors();
                }
                if (FocusNode.SelfAndAllAncestors().OfType<MpAvTransactionSourceViewModelBase>().Any()) {
                    return 0;
                }
                if (FocusNode.SelfAndAllAncestors().OfType<MpAvTransactionMessageViewModelBase>().FirstOrDefault() is MpAvTransactionMessageViewModelBase tmvm) {
                    if (tmvm.TransactionMessageType == MpTransactionMessageType.Request) {
                        return 1;
                    } else {
                        return 2;
                    }
                }
                if (FocusNode != this) {

                }
                return 0;
            }
        }
        public int TransactionIdx {
            get {
                if (Parent == null) {
                    return -1;
                }
                return Parent.Transactions.OrderBy(x => x.TransactionDateTime).IndexOf(this);
            }
        }

        public bool IsAnyBusy =>
            IsBusy ||
            Sources.Any(x => x.IsAnyBusy) ||
            (Messages != null && Messages.Any(x => x.IsAnyBusy));

        public bool IsHovering { get; set; }

        public bool IsSelected {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.SelectedTransaction == this;
            }
            set {
                if (IsSelected != value && Parent != null && value) {
                    Parent.SelectedTransaction = this;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public DateTime LastSelectedDateTime { get; set; }

        public int HostCopyItemId {
            get {
                if (Parent == null || Parent.Parent == null) {
                    return 0;
                }
                return Parent.Parent.CopyItemId;
            }
        }

        public int SelectedTabIndex { get; set; }
        public MpTransactionTabType SelectedTabType =>
            (MpTransactionTabType)SelectedTabIndex;


        public bool IsAnalysisTransaction =>
            Messages.Any(x => x is MpAvAnnotationMessageViewModel || x is MpAvParameterRequestMessageViewModel);

        public bool IsAppliableTransaction =>
            TransactionType == MpTransactionType.Analyzed;

        public bool IsOneTimeAppliableTransaction =>
            Response is MpAvDeltaMessageViewModel;

        public bool IsRunTimeAppliableTransaction =>
            Response is MpAvAnnotationMessageViewModel;

        public bool HasTransactionBeenApplied {
            get {
                if (!IsAppliableTransaction) {
                    return true;
                }
                if (IsOneTimeAppliableTransaction) {
                    return AppliedDateTime.HasValue;
                }
                if (IsSelected &&
                    Response is MpAvAnnotationMessageViewModel amvm) {
                    return amvm.SelectedItem != null;
                }
                return false;
            }
        }

        #endregion

        #region Model

        public MpJsonMessageFormatType RequestMessageType {
            get {
                if (Transaction == null) {
                    return MpJsonMessageFormatType.None;
                }
                return Transaction.RequestMessageType;
            }
        }
        public string RequestJson {
            get {
                if (Transaction == null) {
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
                if (Transaction == null) {
                    return string.Empty;
                }
                return Transaction.ResponseMessageJson;
            }
        }


        public MpTransactionType TransactionType {
            get {
                if (Transaction == null) {
                    return MpTransactionType.None;
                }
                return Transaction.TransactionType;
            }
        }
        public string TransactionLabel {
            get {
                if (Transaction == null || string.IsNullOrEmpty(Transaction.TransactionLabel)) {
                    return TransactionIdx.ToString();
                }
                return Transaction.TransactionLabel;
            }
        }


        public DateTime? AppliedDateTime {
            get {
                if (Transaction == null) {
                    return null;
                }
                return Transaction.AppliedDateTime;
            }
            set {
                if (AppliedDateTime != value) {
                    Transaction.AppliedDateTime = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AppliedDateTime));
                }
            }
        }

        public DateTime TransactionDateTime {
            get {
                if (Transaction == null) {
                    return DateTime.MaxValue;
                }
                return Transaction.TransactionDateTime;
            }
        }

        public int TransactionId {
            get {
                if (Transaction == null) {
                    return 0;
                }
                return Transaction.Id;
            }
        }

        public MpCopyItemTransaction Transaction { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvTransactionItemViewModel(MpAvClipTileTransactionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvTransactionItemViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpCopyItemTransaction cit) {
            IsBusy = true;
            if (HostClipTileViewModel.CopyItemId == 4) {

            }
            Transaction = cit;

            var req = await CreateMessageViewModelAsync(RequestMessageType, RequestJson, this, MpTransactionMessageType.Request);
            if (req != null) {
                Items.Add(req);
            }
            var resp = await CreateMessageViewModelAsync(ResponseMessageType, ResponseJson, this, MpTransactionMessageType.Response);
            if (resp != null) {
                Items.Add(resp);
            }

            var sources = await MpDataModelProvider.GetCopyItemTransactionSourcesAsync(TransactionId);
            foreach (var source in sources) {
                var tsvm = await CreateSourceViewModelAsync(source);
                Items.Add(tsvm);
            }

            //DataObjectViewModel = new MpAvDataObjectViewModel();
            //await DataObjectViewModel.AddOrReplaceFormatAsync(RequestMessageType.ToDataFormat(), RequestJson);
            //await DataObjectViewModel.AddOrReplaceFormatAsync(ResponseMessageType.ToDataFormat(), ResponseJson);


            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Sources));
            OnPropertyChanged(nameof(Messages));

            IsBusy = false;
        }

        public MpQuillDelta GetTransactionDelta() {
            if (Response == null) {
                return null;
            }
            MpQuillDelta delta = null;
            if (Response is MpAvDataObjectMessageViewModel domvm) {
                if (domvm.DataObject.TryGetData<string>(MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT, out string deltaJson) &&
                    MpJsonConverter.DeserializeObject<MpQuillDelta>(deltaJson) is MpQuillDelta resp_delta) {
                    delta = resp_delta;
                }
            }


            if (delta == null &&
                Response is MpAvDeltaMessageViewModel dmvm) {
                delta = dmvm.QuillDelta;
            }
            return delta;
        }

        public MpAnnotationNodeFormat GetTransactionAnnotation() {
            if (Response is MpAvAnnotationMessageViewModel amvm &&
                amvm.RootAnnotationViewModel != null) {
                return amvm.RootAnnotationViewModel.Annotation;
            }
            return null;
        }
        #endregion

        #region Private Methods

        private void MpAvTransactionItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        OnPropertyChanged(nameof(FocusNode));
                    }
                    break;
                case nameof(FocusNode):

                    BringNodeIntoViewAsync(FocusNode).FireAndForgetSafeAsync(this);

                    break;
                case nameof(HasModelChanged):
                    if (!HasModelChanged) {
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        IsBusy = true;
                        await Transaction.WriteToDatabaseAsync();
                        IsBusy = false;
                        HasModelChanged = false;
                    });
                    break;
                case nameof(SelectedSource):
                    Sources.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    OnPropertyChanged(nameof(FocusNode));
                    break;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Sources));
            OnPropertyChanged(nameof(Request));
            OnPropertyChanged(nameof(Response));
        }
        public async Task<MpAvTransactionMessageViewModelBase> CreateMessageViewModelAsync(
            MpJsonMessageFormatType jsonFormat,
            string json,
            MpAvITransactionNodeViewModel parentAnnotation,
            MpTransactionMessageType messageType) {
            if (string.IsNullOrEmpty(json)) {
                return null;
            }
            MpAvTransactionMessageViewModelBase cttimvmb = null;
            switch (jsonFormat) {
                case MpJsonMessageFormatType.DataObject:
                    // scan data object for annotation or delta, then recall this method with message data
                    var mpdo = MpPortableDataObject.Parse(json);
                    if (mpdo.TryGetData(MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT, out string ann_json)) {
                        cttimvmb = await CreateMessageViewModelAsync(MpJsonMessageFormatType.Annotation, ann_json, this, messageType);
                        return cttimvmb;
                    } else if (mpdo.TryGetData(MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT, out string delta_json)) {
                        cttimvmb = await CreateMessageViewModelAsync(MpJsonMessageFormatType.Delta, delta_json, this, messageType);
                        return cttimvmb;
                    } else {
                        cttimvmb = new MpAvDataObjectMessageViewModel(this);
                    }

                    break;
                case MpJsonMessageFormatType.Delta:
                    cttimvmb = new MpAvDeltaMessageViewModel(this);
                    break;
                case MpJsonMessageFormatType.ParameterRequest:
                    cttimvmb = new MpAvParameterRequestMessageViewModel(this);
                    break;
                case MpJsonMessageFormatType.Annotation:
                    cttimvmb = new MpAvAnnotationMessageViewModel(this);
                    break;
                default:
                    return null;
            }
            cttimvmb.TransactionMessageType = messageType;
            await cttimvmb.InitializeAsync(json, parentAnnotation);
            return cttimvmb;
        }

        public async Task<MpAvTransactionSourceViewModelBase> CreateSourceViewModelAsync(MpTransactionSource ts) {
            MpAvTransactionSourceViewModelBase tsvm = null;
            switch (ts.CopyItemSourceType) {
                case MpTransactionSourceType.AnalyzerPreset:
                    tsvm = new MpAvAnalyzerSourceViewModel(this);
                    break;
                case MpTransactionSourceType.App:
                    tsvm = new MpAvAppSourceViewModel(this);
                    break;
                case MpTransactionSourceType.Url:
                    tsvm = new MpAvUrlSourceViewModel(this);
                    break;
                case MpTransactionSourceType.CopyItem:
                    tsvm = new MpAvCopyItemSourceViewModel(this);
                    break;
                case MpTransactionSourceType.UserDevice:
                    tsvm = new MpAvUserDeviceSourceViewModel(this);
                    break;
                default:
                    throw new Exception("Source Type must be defined");
            }
            await tsvm.InitializeAsync(ts);
            return tsvm;
        }

        private async Task BringNodeIntoViewAsync(MpAvITransactionNodeViewModel tnvm) {
            if (_isBringingIntoView ||
                !Parent.IsTransactionPaneOpen ||
                tnvm == this ||
                tnvm == null ||
                !IsSelected) {
                return;
            }
            _isBringingIntoView = true;
            Control node_control = null;
            if (HostClipTileViewModel.GetContentView() is Control c &&
                c.GetVisualAncestor<MpAvClipTileView>() is MpAvClipTileView ctv &&
                ctv.GetVisualDescendant<MpAvClipTileTransactionPaneView>() is MpAvClipTileTransactionPaneView cttpv) {


                if (FocusNode is MpITreeItemViewModel tivm) {
                    if (IsSelected && FocusNodeTabIdx != SelectedTabIndex) {
                        SelectedTabIndex = FocusNodeTabIdx;
                    }
                }
                if (tnvm is MpAvAnnotationItemViewModel anvm) {
                    TreeViewItem ann_tvi =
                        cttpv.GetVisualDescendants<TreeViewItem>()
                        .FirstOrDefault(x => x.DataContext == tnvm);
                    while (ann_tvi == null) {
                        ann_tvi =
                            cttpv.GetVisualDescendants<TreeViewItem>()
                            .FirstOrDefault(x => x.DataContext == tnvm);
                        await Task.Delay(100);
                    }
                    tnvm.AllAncestors()
                        .ForEach(x => x.IsExpanded = true);
                    node_control = ann_tvi;
                    if (anvm.Parent is MpAvAnnotationMessageViewModel amvm) {
                        amvm.OnPropertyChanged(nameof(amvm.SelectedItem));
                    }
                    anvm.OnPropertyChanged(nameof(IsSelected));
                }
            }
            _isBringingIntoView = false;
            if (node_control == null) {
                return;
            }
            node_control.BringIntoView();
        }
        #endregion

        #region Commands



        #endregion
    }
}
