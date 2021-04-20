using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
                return SelectedTagTile.IsEditing;
            }
        }
        #endregion--

        #region Public Methods

        public MpTagTrayViewModel() : base() {
            //create tiles for all the tags
            foreach (MpTag t in MpTag.GetAllTags()) {
                this.Add(new MpTagTileViewModel(t));
            }
        }
        public MpTagTrayViewModel(MpClipTrayViewModel ctrvm) : this() {
            CollectionChanged += (s, e) => {
                UpdateSortOrder();
            };
            ctrvm.CollectionChanged += (s, e) => {
                if(MainWindowViewModel.ClipTileSortViewModel.IsSorting) {
                    return;
                }
                if (e.NewItems != null) {
                    foreach (MpClipTileViewModel ctvm in ctrvm) {
                        AddClipToSudoTags(ctvm);
                    }
                }
                if (e.OldItems != null) {
                    RefreshAllCounts();
                }
            };

        }

        public void TagTray_Loaded(object sender, RoutedEventArgs e) {
            var tagTrayStackPanel = (StackPanel)sender;
            var tagTray = (ListBox)tagTrayStackPanel.FindName("TagTray");
            tagTrayStackPanel.PreviewMouseDown += (s, e10) => {
                MainWindowViewModel.ClipTrayViewModel.ResetClipSelection();
            };
            tagTray.Drop += (s, e2) => {
                return;
            };
            RefreshAllCounts();

            UpdateSortOrder(true);
            GetRecentTagTileViewModel().IsSelected = true;
        }

        public void AddClipToSudoTags(MpClipTileViewModel ctvm) {
            GetHistoryTagTileViewModel().AddClip(ctvm);
            //GetRecentTagTileViewModel().AddClip(ctvm);

            RefreshAllCounts();
        }

        public void UpdateSortOrder(bool fromModel = false) {
            if (fromModel) {
                this.Sort(x => x.TagSortIdx);
            } else {
                foreach (var ttvm in this) {
                    ttvm.TagSortIdx = this.IndexOf(ttvm);
                }
            }
        }

        public void RefreshAllCounts() {
            foreach(var ttvm in this) {
                ttvm.TagClipCount = 0;
                foreach(var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                    if(ttvm.IsLinkedWithClipTile(ctvm)) {
                        ttvm.TagClipCount++;
                    }
                }
            }
            if (GetRecentTagTileViewModel().IsSelected) {
                //will trigger reselection in Add's property change
                GetRecentTagTileViewModel().IsSelected = false;
            }
        }

        public void RefreshRecentTag() {
            if (GetRecentTagTileViewModel().TagClipCount >= Properties.Settings.Default.MaxRecentClipItems) {
                var rtvm = GetRecentTagTileViewModel();
                var rctvml = new List<MpClipTileViewModel>();
                foreach (var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                    if (rtvm.IsLinkedWithClipTile(ctvm)) {
                        rctvml.Add(ctvm);
                    }
                }
                rctvml = rctvml.OrderBy(x => x.CopyItemCreatedDateTime).ToList();
                int itemsToRemoveCount = rtvm.TagClipCount - Properties.Settings.Default.MaxRecentClipItems;
                for (int i = 0; i < itemsToRemoveCount; i++) {
                    rtvm.RemoveClip(rctvml[i]);
                }

                if(rtvm.IsSelected) {
                    //will trigger reselection in Add's property change
                    rtvm.IsSelected = false;
                }
                rtvm.TagClipCount = Properties.Settings.Default.MaxRecentClipItems;
            } else if (GetRecentTagTileViewModel().TagClipCount < Properties.Settings.Default.MaxRecentClipItems) {
                var rtvm = GetRecentTagTileViewModel();
                var rctvml = new List<MpClipTileViewModel>();
                foreach (var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                    if (rtvm.IsLinkedWithClipTile(ctvm)) {
                        rctvml.Add(ctvm);
                    }
                }
                rctvml = rctvml.OrderBy(x => x.CopyItemCreatedDateTime).ToList();
                int itemsToRemoveCount = rtvm.TagClipCount - Properties.Settings.Default.MaxRecentClipItems;
                for (int i = 0; i < itemsToRemoveCount; i++) {
                    rtvm.RemoveClip(rctvml[i]);
                }

                if (rtvm.IsSelected) {
                    //will trigger reselection in Add's property change
                    rtvm.IsSelected = false;
                }
                rtvm.TagClipCount = Properties.Settings.Default.MaxRecentClipItems;
            }
        }
        public new void Add(MpTagTileViewModel newTagTile) {
            base.Add(newTagTile);

            if (newTagTile.IsNew) {
                newTagTile.Tag.WriteToDatabase();
            }
            //watches Tag IsSelected so History is selected if none are
            newTagTile.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(newTagTile.IsSelected):
                        var tagChanged = (MpTagTileViewModel)s;
                        //ensure at least history is selected
                        if (tagChanged.IsSelected == false) {
                            //find all selected tag tiles
                            var selectedTagTiles = this.Where(tt => tt.IsSelected == true).ToList();
                            //if none selected select history tag
                            if (selectedTagTiles == null || selectedTagTiles.Count == 0) {
                                //GetHistoryTagTileViewModel().IsSelected = true;
                                GetRecentTagTileViewModel().IsSelected = true;
                            }
                        } else {
                            MainWindowViewModel.ClipTrayViewModel.FilterByAppIcon = null;
                            MainWindowViewModel.ClipTrayViewModel.IsFilteringByApp = false;

                            foreach (MpClipTileViewModel clipTile in MainWindowViewModel.ClipTrayViewModel) {
                                //this ensures when switching between tags the last selected tag in a list reset
                                clipTile.IsSelected = false;
                                if (tagChanged.IsLinkedWithClipTile(clipTile)) {
                                    clipTile.TileVisibility = Visibility.Visible;
                                    foreach(var rtbvm in clipTile.RichTextBoxViewModelCollection) {
                                        rtbvm.SubItemVisibility = Visibility.Visible;
                                    }
                                } else {
                                    clipTile.TileVisibility = Visibility.Collapsed;
                                }
                            }
                            if (MainWindowViewModel.ClipTrayViewModel.ListBox != null) {
                                //this ensures visibility takes affect if filtering by app
                                //MainWindowViewModel.ClipTrayViewModel.GetTray().Items.Refresh();
                            }
                            
                            if (MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles.Count > 0) {
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
            foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == tagTileToRemove.TagId).ToList()) {
                scvmToRemoveList.Add(scvmToRemove);
            }
            foreach (var scvmToRemove in scvmToRemoveList) {
                MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
            }
        }

        public void ClearTagEditing() {
            foreach(var ttvm in this) {
                ttvm.IsEditing = false;
            }
        }
        public void ClearTagSelection() {
            ClearTagEditing();
            foreach (var tagTile in this) {
                tagTile.IsSelected = false;
                //tagTile.IsTextBoxFocused = false;
            }
        }

        public void ResetTagSelection() {
            ClearTagSelection();
            //GetHistoryTagTileViewModel().IsSelected = true;
            GetRecentTagTileViewModel().IsSelected = true;
        }

        public void UpdateTagAssociation() {
            foreach (var ttvm in this) {
                if (ttvm == GetHistoryTagTileViewModel() || ttvm.IsSelected) {
                    continue;
                }

                bool isTagLinkedToAllSelectedClips = true;
                foreach (var sctvm in MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles) {
                    if (!ttvm.IsLinkedWithClipTile(sctvm)) {
                        isTagLinkedToAllSelectedClips = false;
                    }
                }
                ttvm.IsAssociated = isTagLinkedToAllSelectedClips && MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count > 0;

            }
        }

        public MpTagTileViewModel GetHistoryTagTileViewModel() {
            return this.Where(tt => tt.Tag.TagName == Properties.Settings.Default.HistoryTagTitle).ToList()[0];
        }

        public MpTagTileViewModel GetRecentTagTileViewModel() {
            return this.Where(tt => tt.Tag.TagId == 2).ToList()[0];
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
            MpTag newTag = new MpTag("Untitled", MpHelpers.Instance.GetRandomColor(),this.Count);
            this.Add(new MpTagTileViewModel(newTag));
        }

        #endregion
    }
}
