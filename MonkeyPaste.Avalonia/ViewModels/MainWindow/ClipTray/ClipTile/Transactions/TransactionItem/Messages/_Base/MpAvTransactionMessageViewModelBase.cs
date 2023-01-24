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
    public abstract class MpAvTransactionMessageViewModelBase  : 
        MpViewModelBase<MpAvTransactionItemViewModel>,
        MpAvITransactionNodeViewModel {

        #region Interfaces

        #region MpAvITransactionNodeViewModel Implementation
        public virtual object Body => Json;
        object MpAvITransactionNodeViewModel.TransactionModel => Parent.Transaction;
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem { get; protected set; }
        public IEnumerable<MpITreeItemViewModel> Children => Sources;
        public abstract string LabelText { get; }
        public object ComparableSortValue => ParentTreeItem == null ? 0 : ParentTreeItem.Children.IndexOf(this);
        public object IconSourceObj {
            get {
                if(IconResourceObj == null) {
                    if(Parent == null) {
                        return null;
                    }
                    return Parent.IconSourceObj;
                }
                return IconResourceObj;
            }
        }

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                return null;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvTransactionSourceViewModelBase> Sources { get; set; } = new ObservableCollection<MpAvTransactionSourceViewModelBase>();


        #endregion

        #region Appearance

        public virtual object IconResourceObj { get; set; }
        #endregion
        #region State
        public virtual bool IsAnyBusy => IsBusy || (Sources != null && Sources.Cast<MpAvTransactionMessageViewModelBase>().Any(x => x.IsAnyBusy));
        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Model
        
        public MpJsonMessageFormatType JsonFormat { get; private set; }
        public string Json { get; protected set; }

        public int TransactionId {
            get {
                if(Parent == null) {
                    return 0;
                }
                return Parent.TransactionId;
            }
        }
        #endregion

        #endregion

        #region Constructors

        public MpAvTransactionMessageViewModelBase(MpAvTransactionItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public abstract Task InitializeAsync(object jsonOrParsedFragment, MpAvITransactionNodeViewModel parentAnnotation);

        #endregion

        #region Commands
        

        #endregion
    }
}
