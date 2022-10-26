using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using MonkeyPaste.Common;
using Pango;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileDetailCollectionViewModel : 
        MpAvSelectorViewModelBase<MpAvClipTileViewModel, MpAvClipTileDetailItemViewModel>,
        MpIHoverableViewModel {
        #region Private Variables

        #endregion

        #region Properties
        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }
        #endregion
        #region View Models

        //private List<MpAvClipTileDetailItemViewModel> _items;
        //public IEnumerable<MpAvClipTileDetailItemViewModel> Items {
        //    get {
        //        if(_items == null) {
        //            _items = new List<MpAvClipTileDetailItemViewModel>();
        //            for (int i = 1; i < typeof(MpCopyItemDetailType).Length(); i++) {
        //                var ctdivm = new MpAvClipTileDetailItemViewModel(this, (MpCopyItemDetailType)i);
        //                _items.Add(ctdivm);
        //            }
        //        }
        //        return _items;
        //    }
        //}
        //public MpAvClipTileDetailItemViewModel SelectedItem => Items.FirstOrDefault(x=>x.Is)
        #endregion

        #region Appearance

        #endregion

        #region Model

        //public MpCopyItem CopyItem {
        //    get {
        //        if(Parent == null) {
        //            return null;
        //        }
        //        return Parent.CopyItem;
        //    } 
        //}

        #endregion

        #endregion
        #region Constructors
        public MpAvClipTileDetailCollectionViewModel(MpAvClipTileViewModel parent) : base(parent) {
            if(Items.Count == 0) {
                for (int i = 1; i < typeof(MpCopyItemDetailType).Length(); i++) {
                    var ctdivm = new MpAvClipTileDetailItemViewModel(this, (MpCopyItemDetailType)i);
                    Items.Add(ctdivm);
                }
            }

        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            //IsBusy = true;


            await Task.WhenAll(Items.Select(x => x.IntializeAsync()));
            SelectedItem = Items[0];
            //IsBusy = false;
        }
        #endregion

        #region Private Methods

        private void MpAvClipTileDetailCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                    if(!IsHovering) {
                        CycleDetailCommand.Execute(null);
                    }
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand CycleDetailCommand => new MpAsyncCommand(
            async () => {
                if (Parent == null || Parent.IsPlaceholder) {
                    //_detailIdx = -1;
                    //DetailText = String.Empty;
                    SelectedItem = null;
                    return;
                }

                int sel_idx = Items.IndexOf(SelectedItem);
                do {
                    sel_idx++;
                    if (sel_idx >= Items.Count) {
                        sel_idx = 0;
                    }
                    SelectedItem = Items[sel_idx];
                    SelectedItem.UpdateDetailTextCommand.Execute(null);
                } while (string.IsNullOrEmpty(SelectedItem.DetailText));
            });


        #endregion
    }
}
