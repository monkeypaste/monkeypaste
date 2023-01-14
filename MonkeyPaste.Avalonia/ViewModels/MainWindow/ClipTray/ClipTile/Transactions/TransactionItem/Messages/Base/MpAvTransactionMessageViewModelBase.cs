using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvTransactionMessageViewModelBase  : 
        MpViewModelBase<MpAvTransactionItemViewModelBase>,
        MpITransactionNodeViewModel {

        #region Interfaces

        #region MpITransactionNodeViewModel Implementation
        public object Body { get; }
        object MpITransactionNodeViewModel.TransactionModel => Parent.Transaction;
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem { get; protected set; }
        public IEnumerable<MpITreeItemViewModel> Children => Sources;
        public abstract string LabelText { get; }
        public object ComparableSortValue => ParentTreeItem == null ? 0 : ParentTreeItem.Children.IndexOf(this);
        public object IconSourceObj => null;

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
        public ObservableCollection<MpAvSourceViewModelBase> Sources { get; set; } = new ObservableCollection<MpAvSourceViewModelBase>();


        #endregion

        #region State
        public bool IsAnyBusy => IsBusy || (Sources != null && Sources.Cast<MpAvTransactionMessageViewModelBase>().Any(x => x.IsAnyBusy));
        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Model
        
        public MpJsonMessageFormatType JsonFormat { get; private set; }
        public string Json { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvTransactionMessageViewModelBase(MpAvTransactionItemViewModelBase parent) : base(parent) { }

        #endregion

        #region Public Methods
        public abstract Task InitializeAsync(object jsonOrParsedFragment, MpITransactionNodeViewModel parentAnnotation);

        #endregion

        #region Commands
        

        #endregion
    }
}
