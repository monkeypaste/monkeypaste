using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileDetailCollectionViewModel : 
        MpSelectorViewModelBase<MpAvClipTileViewModel,MpAvClipTileDetailItemViewModel> {
        #region Private Variables

        private int _detailIdx { get; set; } = -1;

        #endregion

        #region Properties

        #region Appearance

        public string DetailText {
            get {
                if (SelectedItem == null) {
                    return string.Empty;
                }
                return SelectedItem.DetailText;
            }
        }

        #endregion



        #region Model

        public MpCopyItem CopyItem {
            get {
                if(Parent == null) {
                    return null;
                }
                return Parent.CopyItem;
            } 
        }

        #endregion

        #endregion
        #region Constructors
        public MpAvClipTileDetailCollectionViewModel(MpAvClipTileViewModel parent) : base(parent) {
            PropertyChanged += MpAvClipTileDetailCollectionViewModel_PropertyChanged;
            Items.Clear();

            for (int i = 1; i < typeof(MpCopyItemDetailType).Length(); i++) {
                var ctdivm = new MpAvClipTileDetailItemViewModel(this, (MpCopyItemDetailType)i);
                Items.Add(ctdivm);
            }
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            IsBusy = true;


            await Task.WhenAll(Items.Select(x => x.IntializeAsync()));
            SelectedItem = null;
            CycleDetailCommand.Execute(null);

            await Task.Delay(1);

            IsBusy = false;
        }
        #endregion

        #region Private Methods

        private void MpAvClipTileDetailCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(CopyItem):
                    InitializeAsync().FireAndForgetSafeAsync(this);
                    break;
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(DetailText));
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand CycleDetailCommand => new MpAsyncCommand(
            async () => {
                if (CopyItem == null || Parent.IsPlaceholder) {
                    //_detailIdx = -1;
                    //DetailText = String.Empty;
                    SelectedItem = null;
                    return;
                }

                int sel_idx = Items.IndexOf(SelectedItem);
                do {
                    sel_idx++;
                    if(sel_idx >= Items.Count) {
                        sel_idx = 0;
                    }
                    SelectedItem = Items[sel_idx];
                } while (string.IsNullOrEmpty(DetailText));
            });


        #endregion
    }
}
