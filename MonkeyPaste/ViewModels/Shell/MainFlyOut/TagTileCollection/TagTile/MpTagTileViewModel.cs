using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpTagTileViewModel : MpViewModelBase {
        #region Properties

        //public MpClipCollectionViewModel ClipCollectionViewModel { get; set; }

        public MpTag Tag { get; set; }

        public bool IsSelected { get; set; } = false;

        public int ClipCount {
            get {
                if (Tag == null || Tag.ClipList == null) {
                    return 0;
                }
                return Tag.ClipList.Count;
            }
        }
               

        public bool IsNameReadOnly { get; set; } = true;
        #endregion

        #region Public Methods
        public MpTagTileViewModel() : this(null) { }

        public MpTagTileViewModel(MpTag tag) {
            PropertyChanged += MpTagViewModel_PropertyChanged;
            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            MpDb.Instance.OnItemDeleted += Db_OnItemDeleted;
            Tag = tag;
            //ClipCollectionViewModel = new MpClipCollectionViewModel(Tag.Id);
            Task.Run(Initialize);
        }        
        #endregion

        #region Private Methods
        private async Task Initialize() {
            Tag.ClipList = await MpClip.GetAllClipsByTagId(Tag.Id);
            Tag.TagColor = await MpColor.GetColorById(Tag.ColorId);
        }

        #region Event Handlers
        private void MpTagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    break;
            }
        }

        private void Db_OnItemAdded(object sender, MpDbModelBase e) {
            Device.InvokeOnMainThreadAsync(async () => {
                if (e is MpClipTag ncit) {
                    if (ncit.TagId == Tag.Id) {
                        //occurs when item is linked to tag
                        var nci = await MpClip.GetClipById(ncit.ClipId);
                        if (!Tag.ClipList.Contains(nci)) {
                            Tag.ClipList.Add(nci);

                            OnPropertyChanged(nameof(ClipCount));
                        }
                    }
                } else if (e is MpClip nci) {
                    //occurs for all and recent
                    bool isLinked = await Tag.IsLinkedWithClipAsync(nci);
                    if (isLinked && !Tag.ClipList.Any(x=>x.Id == nci.Id)) {
                        Tag.ClipList.Add(nci);
                        OnPropertyChanged(nameof(ClipCount));
                    }
                }
            });
        }

        private void Db_OnItemDeleted(object sender, MpDbModelBase e) {
            Device.BeginInvokeOnMainThread(() => {
                if (e is MpClipTag dcit) {
                    if (dcit.TagId == Tag.Id) {
                        //when Clip unlinked
                        var ci = Tag.ClipList.Where(x => x.Id == dcit.ClipId).FirstOrDefault();
                        if (ci != null) {
                            Tag.ClipList.Remove(ci);
                            OnPropertyChanged(nameof(ClipCount));
                        }
                    }
                } else if (e is MpClip dci) {
                    //when copy item deleted
                    if (Tag.ClipList.Any(x=>x.Id == dci.Id)) {
                        Tag.ClipList.Remove(dci);
                        OnPropertyChanged(nameof(ClipCount));
                    }
                }
            });
        }
        #endregion
        #endregion        

        #region Commands
        public ICommand RenameTagCommand => new Command(() => {
            IsNameReadOnly = false;
        });
        #endregion
    }
}
