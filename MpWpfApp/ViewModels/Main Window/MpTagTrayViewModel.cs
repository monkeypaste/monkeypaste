using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpTagTrayViewModel : MpObservableCollectionViewModel<MpTagTileViewModel> {
        #region View Models
        public MpTagTileViewModel SelectedTagTile {
            get {
                return this.Where(tt => tt.IsSelected).ToList()[0];
            }
        }
        #endregion

        #region Properties
        public bool IsEditingTagName {
            get {
                return SelectedTagTile.IsFocused || SelectedTagTile.IsEditing;
            }
        }
        #endregion--

        #region Public Methods

        public MpTagTrayViewModel() {
            //create tiles for all the tags
            foreach (MpTag t in MpTag.GetAllTags()) {
                this.Add(new MpTagTileViewModel(t,this));
            }
        }

        public void TagTray_Loaded(object sender, RoutedEventArgs e) {
            //select history tag by default
            GetHistoryTagTileViewModel().IsSelected = true;
        }

        public new void Add(MpTagTileViewModel newTagTile) {
            if(newTagTile.IsNew) {
                newTagTile.Tag.WriteToDatabase();
            }
            base.Add(newTagTile);
            //watches Tag IsSelected so History is selected if none are
            newTagTile.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case "IsSelected":
                        var tagChanged = (MpTagTileViewModel)s;
                        //ensure at least history is selected
                        if (tagChanged.IsSelected == false) {
                            //find all selected tag tiles
                            var selectedTagTiles = this.Where(tt => tt.IsSelected == true).ToList();
                            //if none selected select history tag
                            if (selectedTagTiles == null || selectedTagTiles.Count == 0) {
                                GetHistoryTagTileViewModel().IsSelected = true;
                            }
                        } else {
                            foreach (MpClipTileViewModel clipTile in MainWindowViewModel.ClipTrayViewModel) {
                                //this ensures when switching between tags the last selected tag in a list reset
                                clipTile.IsSelected = false;
                                if (tagChanged.Tag.IsLinkedWithCopyItem(clipTile.CopyItem)) {
                                    clipTile.TileVisibility = Visibility.Visible;
                                } else {
                                    clipTile.TileVisibility = Visibility.Collapsed;
                                }
                            }
                            if (MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles.Count == 0) {
                                MainWindowViewModel.ClipTrayViewModel.ClipListVisibility = Visibility.Collapsed;
                                MainWindowViewModel.ClipTrayViewModel.EmptyListMessageVisibility = Visibility.Visible;
                            } else {
                                MainWindowViewModel.ClipTrayViewModel.ClipListVisibility = Visibility.Visible;
                                MainWindowViewModel.ClipTrayViewModel.EmptyListMessageVisibility = Visibility.Collapsed;

                                MainWindowViewModel.ClipTrayViewModel.ResetClipSelection();
                            }
                        }
                        break;
                }
            };
        }

        public new void Remove(MpTagTileViewModel tagTileToRemove) {
            //when removing a tag auto-select the history tag
            if (tagTileToRemove.IsSelected) {
                tagTileToRemove.IsSelected = false;
                this.Where(tt => tt.Tag.TagName == Properties.Settings.Default.HistoryTagTitle).ToList()[0].IsSelected = true;
            }
            base.Remove(tagTileToRemove);
            tagTileToRemove.Tag.DeleteFromDatabase();

            //remove any shortcuts associated with clip
            var scvmToRemoveList = new List<MpShortcutViewModel>();
            foreach (var scvmToRemove in MainWindowViewModel.ShortcutCollectionViewModel.Where(x => x.CopyItemId == tagTileToRemove.TagId).ToList()) {
                scvmToRemoveList.Add(scvmToRemove);
            }
            foreach (var scvmToRemove in scvmToRemoveList) {
                MainWindowViewModel.ShortcutCollectionViewModel.Remove(scvmToRemove);
            }
        }

        public void ClearTagSelection() {
            foreach (var tagTile in this) {
                tagTile.IsSelected = false;
                tagTile.IsFocused = false;
            }
        }

        public void ResetTagSelection() {
            ClearTagSelection();
            GetHistoryTagTileViewModel().IsSelected = true;
            GetHistoryTagTileViewModel().IsFocused = true;
        }

        public MpTagTileViewModel GetHistoryTagTileViewModel() {
            return this.Where(tt => tt.Tag.TagName == Properties.Settings.Default.HistoryTagTitle).ToList()[0];
        }

        #endregion

        #region Commands
        private RelayCommand _deleteTagCommand;
        public ICommand DeleteTagCommand {
            get {
                if (_deleteTagCommand == null) {
                    _deleteTagCommand = new RelayCommand(DeleteTag, CanDeleteTag);
                }
                return _deleteTagCommand;
            }
        }
        private bool CanDeleteTag() {
            //allow delete if any tag besides history tag is selected, delete method will ignore history\
            return SelectedTagTile.TagName != Properties.Settings.Default.HistoryTagTitle;
        }
        private void DeleteTag() {
            this.Remove(SelectedTagTile);
        }

        private RelayCommand _createTagCommand;
        public ICommand CreateTagCommand {
            get {
                if (_createTagCommand == null) {
                    _createTagCommand = new RelayCommand(CreateTag);
                }
                return _createTagCommand;
            }
        }
        private void CreateTag() {
            //add tag to datastore so TagTile collection will automatically add the tile
            MpTag newTag = new MpTag("Untitled", MpHelpers.GetRandomColor());
            this.Add(new MpTagTileViewModel(newTag, this));
        }

        #endregion
    }
}
