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
    public class MpAvTransactionItemViewModel  : 
        MpViewModelBase<MpAvClipTileTransactionCollectionViewModel>,
        MpAvITransactionNodeViewModel {

        #region Interfaces

        #region MpAvITransactionNodeViewModel Implementation

        public object Body => "Empty TransactionItem Body";
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => null;
        public IEnumerable<MpITreeItemViewModel> Children => Items;
        public string LabelText => 
            Transaction == null ? TransactionIdx.ToString() : Transaction.TransactionLabel;
        public object ComparableSortValue => TransactionDateTime;
        public object IconSourceObj => SourceIconObj;

        object MpAvITransactionNodeViewModel.TransactionModel => Transaction;

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                if (Transaction == null) {
                    return new MpMenuItemViewModel();
                }
                return new MpMenuItemViewModel() {
                    IconSourceObj = SourceIconObj,
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

        public ObservableCollection<MpAvTransactionSourceViewModelBase> Sources { get; set; } = new ObservableCollection<MpAvTransactionSourceViewModelBase>();
        public IEnumerable<MpAvTransactionMessageViewModelBase> Items {
            get {
                var items = new List<MpAvTransactionMessageViewModelBase>();
                if(Request != null) {
                    items.Add(Request);
                }
                if(Response != null) {
                    items.Add(Response);
                }
                return items;
            }
        }

        public MpAvTransactionMessageViewModelBase Request { get; set; }
        public MpAvTransactionMessageViewModelBase Response { get; set; }

        public MpAvDataObjectViewModel DataObjectViewModel { get; private set; }
        #endregion

        #region Appearance

        #endregion

        #region State

        public int TransactionIdx {
            get {
                if(Parent == null) {
                    return -1;
                }
                return Parent.Transactions.OrderBy(x => x.TransactionDateTime).IndexOf(this);
            }
        }

        public bool IsAnyBusy => 
            IsBusy || 
            Sources.Any(x=>x.IsAnyBusy) ||
            (Items != null && Items.Any(x => x.IsAnyBusy));

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
                if(Sources.Count > 0) {
                    return Sources.OrderBy(x => x.SourcedDateTimeUtc).FirstOrDefault().IconSourceObj;
                }
                return MpDefaultDataModelTools.ThisAppIconId;
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
                if(Transaction == null) {
                    return 0;
                }
                return Transaction.Id;
            }
        }

        public MpCopyItemTransaction Transaction { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvTransactionItemViewModel (MpAvClipTileTransactionCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpCopyItemTransaction cit) {
            IsBusy = true;

            Transaction = cit; 

            Request = await CreateMessageViewModelAsync(RequestMessageType, RequestJson, this);
            Response = await CreateMessageViewModelAsync(ResponseMessageType, ResponseJson, this);

            var sources = await MpDataModelProvider.GetCopyItemTransactionSourcesAsync(TransactionId);
            foreach(var source in sources) {
                var tsvm = await CreateSourceViewModelAsync(source);
                Sources.Add(tsvm);
            }

            //DataObjectViewModel = new MpAvDataObjectViewModel();
            //await DataObjectViewModel.AddOrReplaceFormatAsync(RequestMessageType.ToDataFormat(), RequestJson);
            //await DataObjectViewModel.AddOrReplaceFormatAsync(ResponseMessageType.ToDataFormat(), ResponseJson);


            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Sources));
            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        public MpQuillDelta GetTransactionDelta() {
            if(Response == null) {
                return null;
            }
            MpQuillDelta delta = null;
            if(Response is MpAvDataObjectMessageViewModel domvm) {
                if(domvm.DataObject.TryGetData<string>(MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT, out string deltaJson) &&
                    MpJsonConverter.DeserializeObject<MpQuillDelta>(deltaJson) is MpQuillDelta resp_delta) {
                    delta = resp_delta;
                } 
            }
            
            
            if(delta == null && 
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


        public async Task<MpAvTransactionSourceViewModelBase> CreateSourceViewModelAsync(MpTransactionSource ts) {
            MpAvTransactionSourceViewModelBase tsvm = null;
            switch(ts.CopyItemSourceType) {
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
        
        public async Task<MpAvTransactionMessageViewModelBase> CreateMessageViewModelAsync(MpJsonMessageFormatType jsonFormat, string json, MpAvITransactionNodeViewModel parentAnnotation) {
            if(string.IsNullOrEmpty(json)) {
                return null;
            }
            MpAvTransactionMessageViewModelBase cttimvmb = null;
            switch (jsonFormat) {
                case MpJsonMessageFormatType.DataObject:
                    var mpdo = MpPortableDataObject.Parse(json);
                    if(mpdo.TryGetData(MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT, out string ann_json)) {
                        cttimvmb = await CreateMessageViewModelAsync(MpJsonMessageFormatType.Annotation, ann_json,this);
                        return cttimvmb;
                    } else if(mpdo.TryGetData(MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT, out string delta_json)) {
                        cttimvmb = await CreateMessageViewModelAsync(MpJsonMessageFormatType.Delta, delta_json, this);
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
            await cttimvmb.InitializeAsync(json, parentAnnotation);
            return cttimvmb;

        }
        #endregion

        #region Commands


        public ICommand SelectChildCommand => new MpCommand<object>(
            (args) => {
                if(args is string argStr) {
                    if(argStr.IsStringGuid()) {
                        if(Response is MpAvAnnotationMessageViewModel iamvm) {
                            // this only comes from editor
                            var to_select =
                                iamvm.SelectedItem.AllDescendants()
                                .Cast<MpAvAnnotationItemViewModel>().FirstOrDefault(x => x.AnnotationGuid == argStr);

                            iamvm.SelectedItem = to_select;
                        }
                    }
                }
            });

        #endregion
    }
}
