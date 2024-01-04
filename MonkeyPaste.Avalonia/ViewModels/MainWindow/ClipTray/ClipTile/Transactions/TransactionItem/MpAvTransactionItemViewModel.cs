using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
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
        MpAvViewModelBase<MpAvClipTileTransactionCollectionViewModel>,
        MpAvITransactionNodeViewModel {
        #region Private Variables
        private bool _isBringingIntoView = false;
        #endregion

        #region Interfaces

        #region MpIPlainTextViewModel Implementation

        public string PlainText { get; private set; }

        #endregion

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
            Sources.Any() ?
                Sources.FirstOrDefault().LabelText :
                Transaction == null ?
                    TransactionIdx.ToString() :
                    Transaction.TransactionLabel;
        public object ComparableSortValue => TransactionDateTime;
        public object IconSourceObj {
            get {
                if (Sources.Count() > 0) {
                    return Sources.OrderBy(x => x.SourcedDateTimeUtc).FirstOrDefault().IconSourceObj;
                }
                return MpBase64Images.QuestionMark;
            }
        }


        #region MpIMenuItemViewModel Implementation

        public MpAvMenuItemViewModel ContextMenuItemViewModel =>
            null;

        #endregion

        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvITransactionNodeViewModel> Items { get; private set; } = new ObservableCollection<MpAvITransactionNodeViewModel>();

        public IEnumerable<MpAvTransactionSourceViewModel> Sources =>
            Items.OfType<MpAvTransactionSourceViewModel>().ToList();
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


        public MpAvTransactionSourceViewModel SelectedSource { get; set; }
        #endregion

        #region Appearance

        #endregion

        #region State

        public bool IsPlainTextView {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.IsPlainTextView;
            }
            set {
                if (IsPlainTextView != value &&
                    Parent != null) {
                    Parent.IsPlainTextView = value;
                    OnPropertyChanged(nameof(IsPlainTextView));
                }
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

        private bool _hasTransactionBeenApplied;
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
                    return amvm.SelectedItem != null && _hasTransactionBeenApplied;
                }
                return false;
            }
            set {
                if (HasTransactionBeenApplied != value) {
                    _hasTransactionBeenApplied = value;
                    OnPropertyChanged(nameof(HasTransactionBeenApplied));
                }
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

        public int TransactionDeviceId {
            get {
                if (Transaction == null) {
                    return 0;
                }
                return Transaction.TransactionUserDeviceId;
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
            if (TransactionId == 480) {

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

        public bool HasSource(MpISourceRef sr) {
            return Sources.Any(x => x.SourceRef.IsSourceEqual(sr));
        }

        public MpQuillDelta GetTransactionDelta() {
            if (Response == null) {
                return null;
            }
            MpQuillDelta delta = null;
            if (Response is MpAvDataObjectMessageViewModel domvm) {
                if (domvm.DataObject.TryGetData<string>(MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT, out string deltaJson) &&
                    MpJsonExtensions.DeserializeObject<MpQuillDelta>(deltaJson) is MpQuillDelta resp_delta) {
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
                        if (Parent.LastTabIdx >= 0) {
                            SelectedTabIndex = Parent.LastTabIdx;
                        }
                    }
                    break;
                case nameof(FocusNode):
                    if (FocusNode != null &&
                        IsSelected &&
                        HostClipTileViewModel != null) {
                        // TODO this be inconsisten since only annotation nodes have guids atm 
                        // but nothing else is enabled for trans so should be ok


                    }
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
                case nameof(SelectedTabIndex):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    Parent.LastTabIdx = SelectedTabIndex;
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
                    var mpdo = MpAvDataObject.Parse(json);
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

        public async Task<MpAvTransactionSourceViewModel> CreateSourceViewModelAsync(MpTransactionSource ts) {
            MpAvTransactionSourceViewModel tsvm = new MpAvTransactionSourceViewModel(this);
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

                int node_tab_idx = tnvm.GetNodeTabIdx();
                if (IsSelected && node_tab_idx != SelectedTabIndex) {
                    SelectedTabIndex = node_tab_idx;
                }
                await Task.Delay(200);

                var to_bring_into_view =
                    cttpv
                    .GetVisualDescendants<Control>()
                    .Where(x => (x is ListBoxItem || x is TreeViewItem) && x.DataContext == tnvm);

                to_bring_into_view.ForEach(x => x.BringIntoView());



                //if (tnvm is MpAvAnnotationItemViewModel anvm) {
                //    TreeViewItem ann_tvi =
                //        cttpv.GetVisualDescendants<TreeViewItem>()
                //        .FirstOrDefault(x => x.DataContext == tnvm);
                //    while (ann_tvi == null) {
                //        ann_tvi =
                //            cttpv.GetVisualDescendants<TreeViewItem>()
                //            .FirstOrDefault(x => x.DataContext == tnvm);
                //        await Task.Delay(100);
                //    }
                //    tnvm.AllAncestors()
                //        .ForEach(x => x.IsExpanded = true);
                //    node_control = ann_tvi;
                //    if (anvm.Parent is MpAvAnnotationMessageViewModel amvm) {
                //        amvm.OnPropertyChanged(nameof(amvm.SelectedItem));
                //    }
                //    anvm.OnPropertyChanged(nameof(IsSelected));
                //}
            }
            _isBringingIntoView = false;
            if (node_control == null) {
                return;
            }
            node_control.BringIntoView();
        }

        private async Task<string> GetTransactionAsPlainTextAsync() {
            await Task.Delay(1);
            object jtrans = new {
                transaction = Transaction,
                sources = Sources.Select(x => x.SourceRef).ToList()
            };
            string output = JsonConvert.SerializeObject(jtrans);
            return output.ToPrettyPrintJson();
        }

        #endregion

        #region Commands


        public ICommand TogglePlainTextCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsPlainTextView = !IsPlainTextView;

                if (args is Control c &&
                    c.GetVisualAncestor<MpAvClipTileTransactionPaneView>() is MpAvClipTileTransactionPaneView tpv &&
                    tpv.FindControl<Control>("SelectedTransactionContainer") is Control stc) {
                    PlainText = await GetTransactionAsPlainTextAsync();

                    stc.DataContext = null;
                    stc.DataContext = this;
                }
            });

        public MpIAsyncCommand DeleteThisTransactionCommand => new MpAsyncCommand(
            async () => {
                await Transaction.DeleteFromDatabaseAsync();
                await Parent.InitializeAsync(Parent.Parent.CopyItemId);
            });
        #endregion
    }

}
