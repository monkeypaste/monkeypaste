﻿using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public enum MpTransactionMessageType {
        Request,
        Response
    }
    public abstract class MpAvTransactionMessageViewModelBase :
        MpAvViewModelBase<MpAvTransactionItemViewModel>,
        MpAvITransactionNodeViewModel {

        #region Interfaces

        #region MpIPlainTextViewModel Implementation

        public string PlainText {
            get {
                //return "TEST message";
                return Body;
            }
        }
        #endregion

        #region MpAvITransactionNodeViewModel Implementation
        public MpAvClipTileViewModel HostClipTileViewModel =>
            Parent == null || Parent.Parent == null ?
                null :
                Parent.Parent.Parent;

        public virtual string Body => Json;
        public bool IsExpanded { get; set; }

        private MpITreeItemViewModel _parentTreeItem;
        public MpITreeItemViewModel ParentTreeItem {
            get {
                if (_parentTreeItem == this) {
                    MpDebug.Break();
                }
                return _parentTreeItem ?? Parent;
            }
            protected set {
                if (ParentTreeItem != value) {
                    if (value == this) {
                        MpDebug.Break();
                    }
                    _parentTreeItem = value;
                    OnPropertyChanged(nameof(ParentTreeItem));
                }
            }
        }

        public virtual IEnumerable<MpITreeItemViewModel> Children => Sources;
        public abstract string LabelText { get; }
        public object ComparableSortValue => ParentTreeItem == null ? 0 : ParentTreeItem.Children.IndexOf(this);
        public object IconSourceObj {
            get {
                if (IconResourceObj == null) {
                    if (Parent == null) {
                        return null;
                    }
                    return Parent.IconSourceObj;
                }
                return IconResourceObj;
            }
        }

        #region MpIMenuItemViewModel Implementation

        public virtual MpAvMenuItemViewModel ContextMenuItemViewModel {
            get {
                return null;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvTransactionSourceViewModel> Sources { get; set; } = new ObservableCollection<MpAvTransactionSourceViewModel>();


        #endregion

        #region Appearance

        public virtual object IconResourceObj { get; set; }
        #endregion

        #region State
        public virtual bool IsAnyBusy =>
            IsBusy || (Sources != null && Sources.Any(x => x.IsAnyBusy));
        public bool IsHovering { get; set; }

        public bool IsSelected {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.SelectedTabIndex == this.GetNodeTabIdx();
            }
            set {
                if (value && IsSelected != value && Parent != null) {
                    Parent.SelectedTabIndex = this.GetNodeTabIdx();
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public DateTime LastSelectedDateTime { get; set; }

        public MpTransactionMessageType TransactionMessageType { get; set; }
        #endregion

        #region Model

        public MpJsonMessageFormatType JsonFormat { get; private set; }
        public string Json { get; protected set; }

        public int TransactionId {
            get {
                if (Parent == null) {
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
