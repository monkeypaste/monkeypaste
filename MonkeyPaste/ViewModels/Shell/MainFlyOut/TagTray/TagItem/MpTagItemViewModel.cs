﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpTagItemViewModel : MpViewModelBase {
        public MpTag Tag { get; set; }

        public bool IsSelected { get; set; } = false;

        public int CopyItemCount {
            get {
                if (Tag == null || Tag.CopyItemList == null) {
                    return 0;
                }
                return Tag.CopyItemList.Count;
            }
        }
        public MpTagItemViewModel() : this(null) { }

        public MpTagItemViewModel(MpTag tag) {
            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            MpDb.Instance.OnItemDeleted += Db_OnItemDeleted;
            Tag = tag;
            Task.Run(Initialize);
        }        

        private async Task Initialize() {
            Tag.CopyItemList = await MpCopyItem.GetAllCopyItemsByTagId(Tag.Id);
            Tag.TagColor = await MpColor.GetColorById(Tag.ColorId);
        }
        private async void Db_OnItemAdded(object sender, MpDbObject e) {
            if(e is MpCopyItemTag ncit) {
                if(ncit.TagId == Tag.Id) {
                    //occurs when item is linked to tag
                    var nci = await MpCopyItem.GetCopyItemById(ncit.CopyItemId);
                    if(!Tag.CopyItemList.Contains(nci)) {
                        Tag.CopyItemList.Add(nci);
                    }
                }
            } else if(e is MpCopyItem nci) {
                //occurs for all and recent
                bool isLinked = await Tag.IsLinkedWithCopyItemAsync(nci.Id);
                if(isLinked) {
                    Tag.CopyItemList.Add(nci);
                    //OnPropertyChanged(nameof(CopyItemCount));
                }
            }
        }
        private async void Db_OnItemDeleted(object sender, MpDbObject e) {
            if (e is MpCopyItemTag dcit) {
                if (dcit.TagId == Tag.Id) {
                    //when copyitem unlinked
                    var ci = Tag.CopyItemList.Where(x => x.Id == dcit.CopyItemId).FirstOrDefault();
                    if(ci != null) {
                        Tag.CopyItemList.Remove(ci);
                    }
                }
            } else if(e is MpCopyItem dci) {
                //when copy item deleted
                if(Tag.CopyItemList.Contains(dci)) {
                    Tag.CopyItemList.Remove(dci);
                }
            }
        }

        public ICommand RenameTagCommand => new Command(() => {
            MpConsole.WriteLine("Rename Tag");
        });
               
    }
}