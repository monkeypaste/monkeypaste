using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpTagTrayViewModel : ObservableCollection<MpTagTileViewModel> {
        #region View Models
        public MpMainWindowViewModel MainWindowViewModel { get; set; }

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

        public MpTagTrayViewModel(MpMainWindowViewModel parent) {
            MainWindowViewModel = parent;
            //create tiles for all the tags
            foreach (MpTag t in MpTag.GetAllTags()) {
                AddTagTile(t, false);
            }
        }

        public void TagTray_Loaded(object sender, RoutedEventArgs e) {
            //select history tag by default
            GetHistoryTagTileViewModel().IsSelected = true;
        }

        public void AddTagTile(MpTag t, bool isNew = false) {
            var newTagTile = new MpTagTileViewModel(t, this, isNew);
            this.Add(newTagTile);
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
            //if (!isNew) {
            //    foreach (var ctvm in MainWindowViewModel.ClipTrayViewModel) {
            //        if (newTagTile.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
            //            newTagTile.TagClipCount++;
            //        }
            //    }
            //}
        }

        private void RemoveTagTile(MpTagTileViewModel tagTileToRemove) {
            //when removing a tag auto-select the history tag
            if (tagTileToRemove.IsSelected) {
                tagTileToRemove.IsSelected = false;
                this.Where(tt => tt.Tag.TagName == Properties.Settings.Default.HistoryTagTitle).ToList()[0].IsSelected = true;
            }
            this.Remove(tagTileToRemove);
            tagTileToRemove.Tag.DeleteFromDatabase();
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
            RemoveTagTile(SelectedTagTile);
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
            newTag.WriteToDatabase();
            AddTagTile(newTag, true);
        }

        #endregion
    }
}
