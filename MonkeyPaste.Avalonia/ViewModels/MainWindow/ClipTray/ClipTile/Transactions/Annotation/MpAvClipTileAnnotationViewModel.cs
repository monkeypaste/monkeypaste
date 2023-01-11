using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileAnnotationViewModel  : 
        MpViewModelBase<MpAvClipTileSourceViewModel>,
        MpITransactionNodeViewModel {

        #region Interfaces

        #region MpITransactionNodeViewModel Implementation
        object MpITransactionNodeViewModel.TransactionModel => AnnotationNodeFormat;
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem { get; set; }
        public IEnumerable<MpITreeItemViewModel> Children { get; private set; }
        public string LabelText => AnnotationNodeFormat == null ? string.Empty : AnnotationNodeFormat.label;
        public object ComparableSortValue => 1;
        public object IconSourceObj => null;

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                if (AnnotationNodeFormat == null) {
                    return new MpMenuItemViewModel();
                }
                return new MpMenuItemViewModel() {
                    IconSourceObj = IconSourceObj,
                    Header = LabelText,
                    SubItems = 
                        Children == null || Children.Count() == 0 ? 
                            null : Children.Cast<MpITransactionNodeViewModel>().Select(x=>x.ContextMenuItemViewModel).ToList()
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
        public bool IsAnyBusy => IsBusy || (Items != null && Items.Cast<MpAvClipTileAnnotationViewModel>().Any(x => x.IsAnyBusy));
        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Model
        
        public MpAnnotationNodeFormat AnnotationNodeFormat { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvClipTileAnnotationViewModel(MpAvClipTileSourceViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpAnnotationNodeFormat annotationNodeFormat, MpITreeItemViewModel pti) {
            IsBusy = true;

            if(Items != null) {
                Items.Clear();
            }
            ParentTreeItem = pti;
            AnnotationNodeFormat = annotationNodeFormat;

            if(AnnotationNodeFormat.children != null) {
                foreach (var can in AnnotationNodeFormat.children) {
                    var canf = can as MpAnnotationNodeFormat;
                    if(canf == null) {
                        continue;
                    }
                    var cavm = await Parent.CreateAnnotationViewModel(canf, this);
                    if(cavm != null) {
                        if (Items == null) {
                            Items = new ObservableCollection<MpITransactionNodeViewModel>();
                        }
                        Items.Add(cavm);
                    }
                    
                }
            }
            
            if(Items != null) {

                while (Items.Any(x=>x.IsAnyBusy)) {
                    await Task.Delay(100);
                }
            }

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Items));
            IsBusy = false;
        }

        #endregion

        #region Commands
        

        #endregion
    }
}
