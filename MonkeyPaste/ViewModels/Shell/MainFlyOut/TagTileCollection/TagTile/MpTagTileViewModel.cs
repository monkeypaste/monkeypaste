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
        #region Private Variables
        private string _orgTagName = string.Empty;
        #endregion

        #region Properties

        #region Model
        public MpTag Tag { get; set; }
        #endregion

        #region State
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

        #region Drag & Drop
        public bool IsBeingDraggedOver { get; set; } = false;

        public bool IsBeingDragged { get; set; } = false;
        #endregion

        #region Business Logic
        public bool IsUserTag {
            get {
                if(Tag == null) {
                    return false;
                }
                return Tag.Id > 4;
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpTagTileViewModel() : this(null) { }

        public MpTagTileViewModel(MpTag tag) {
            PropertyChanged += MpTagViewModel_PropertyChanged;
            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            MpDb.Instance.OnItemDeleted += Db_OnItemDeleted;
            MpDb.Instance.OnItemUpdated += Db_OnItemUpdated;
            Tag = tag;
            //ClipCollectionViewModel = new MpClipCollectionViewModel(Tag.Id);
            Task.Run(Initialize);
        }

        #endregion

        #region Private Methods
        private async Task Initialize() {
            Tag.ClipList = await MpClip.GetAllClipsByTagId(Tag.Id);
            Tag.TagColor = await MpColor.GetColorByIdAsync(Tag.ColorId);
        }

        #region Event Handlers
        private void MpTagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            Task.Run(async () => {
                switch (e.PropertyName) {
                    case nameof(IsNameReadOnly):
                        if(IsNameReadOnly && Tag.TagName != _orgTagName) {
                            _orgTagName = Tag.TagName;
                            await MpDb.Instance.AddOrUpdateAsync<MpTag>(Tag);
                        }
                        break;
                }
            });
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
        //private void Db_OnItemUpdated(object sender, MpDbObjectUpdateEventArg e) {
        //    Device.InvokeOnMainThreadAsync(async () => {
        //        if (e.DbObject is MpTag t) {
        //            if (t.Id == Tag.Id) {
        //                if(e.UpdatedPropertyLookup.Count == 0) {
        //                    Tag = t;
        //                } else {
        //                    foreach(var kvp in e.UpdatedPropertyLookup) {
        //                        var prop = Tag.GetType().GetProperties().Where(x => x.Name == kvp.Key).FirstOrDefault();
        //                        if(prop != null) {
        //                            if(prop.PropertyType == typeof(int)) {
        //                                prop.SetValue(Tag, Convert.ToInt32(kvp.Value));
        //                            } else if (prop.PropertyType == typeof(string)) {
        //                                prop.SetValue(Tag, kvp.Value);
        //                            } else {
        //                                MpConsole.WriteTraceLine(@"Unknown property type: " + prop.PropertyType.ToString());
        //                            }
        //                        }
        //                    }
        //                }                        
        //            }
        //        } 
        //    });
        //}

        private void Db_OnItemUpdated(object sender, MpDbModelBase e) {
            Device.InvokeOnMainThreadAsync(async () => {
                if (e is MpTag t) {
                    if (t.Id == Tag.Id) {
                        Tag = t;
                    }
                } else if(e is MpColor c) {
                    if(c.Id == Tag.ColorId) {
                        Tag.TagColor = await MpColor.GetColorByIdAsync(c.Id);
                        OnPropertyChanged(nameof(Tag));
                    }
                }
            });
        }

        private void Db_OnItemDeleted(object sender, MpDbModelBase e) {
            //Device.BeginInvokeOnMainThread(() => {
            //    if (e is MpClipTag dcit) {
            //        if (dcit.TagId == Tag.Id) {
            //            //when Clip unlinked
            //            var ci = Tag.ClipList.Where(x => x.Id == dcit.ClipId).FirstOrDefault();
            //            if (ci != null) {
            //                Tag.ClipList.Remove(ci);
            //                OnPropertyChanged(nameof(ClipCount));
            //            }
            //        }
            //    } else if (e is MpClip dci) {
            //        //when copy item deleted
            //        if (Tag.ClipList.Any(x=>x.Id == dci.Id)) {
            //            Tag.ClipList.Remove(dci);
            //            OnPropertyChanged(nameof(ClipCount));
            //        }
            //    }
            //});
        }
        #endregion

        #endregion        

        #region Commands
        public ICommand RenameTagCommand => new Command(
            () => {
                _orgTagName = Tag.TagName;
                IsNameReadOnly = false;
            },
            () => {
                return Tag.Id > 4;
            });

        public ICommand ChangeTagColorCommand => new Command(async () => {
            Tag.TagColor.Color = MpHelpers.Instance.GetRandomColor();
            await MpDb.Instance.UpdateItemAsync<MpColor>(Tag.TagColor);
            OnPropertyChanged(nameof(Tag));
        });
        #endregion
    }
}
