
using MpWinFormsClassLibrary;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace MpWpfApp {
    public class MpMainWindowViewModel : MpViewModelBase {
        private ObservableCollection<MpClipTileViewModel> _clipTiles = new ObservableCollection<MpClipTileViewModel>();
        public ObservableCollection<MpClipTileViewModel> ClipTiles {
            get {
                return _clipTiles;
            }
            set {
                if(_clipTiles != value) {
                    _clipTiles = value;
                }
            }
        }

        public List<MpClipTileViewModel> SelectedClipTiles {
            get {
                return ClipTiles.Where(ct => ct.IsSelected).ToList();
            }
        }

        private ObservableCollection<MpTagTileViewModel> _tagTiles = new ObservableCollection<MpTagTileViewModel>();
        public ObservableCollection<MpTagTileViewModel> TagTiles {
            get {
                return _tagTiles;
            }
            set {
                if(_tagTiles != value) {
                    _tagTiles = value;
                }
            }
        }

        private bool _isInAppendMode = false;
        public bool IsInAppendMode {
            get {
                return _isInAppendMode;
            }
            set {
                if(_isInAppendMode != value) {
                    _isInAppendMode = value;
                    OnPropertyChanged("IsInAppendMode");
                }
            }
        }

        private bool _isAppPaused = false;
        public bool IsAppPaused {
            get {
                return _isAppPaused;
            }
            set {
                if(_isAppPaused != value) {
                    _isAppPaused = value;
                    OnPropertyChanged("IsAppPaused");
                }
            }
        }

        public double AppStateButtonGridWidth {
            get {
                return MpMeasurements.Instance.AppStateButtonPanelWidth;
            }
        }

        public double TrayHeight {
            get {
                return MpMeasurements.Instance.TrayHeight;
            }
        }

        public double TitleMenuHeight {
            get {
                return MpMeasurements.Instance.TitleMenuHeight;
            }
        }

        public double FilterMenuHeight {
            get {
                return MpMeasurements.Instance.FilterMenuHeight;
            }
        }

        public double MainWindowWidth {
            get {
                return SystemParameters.PrimaryScreenWidth;
            }
        }

        public double MainWindowHeight {
            get {
                return MpMeasurements.Instance.MainWindowRect.Height;
            }
        }

        public MpMainWindowViewModel() {
            base.DisplayName = "MpMainWindowViewModel";

            //clears model data and loads everything from db and setups clipboard listener
            MpDataStore.Instance.Init();

            //when clipboard changes add a cliptile
            MpDataStore.Instance.ClipList.CollectionChanged += (s1, e1) => {
                if(e1.NewItems != null) {
                    foreach(MpClip c in e1.NewItems) {
                        AddClipTile(c);
                    }
                }
                if(e1.OldItems != null) {
                    foreach(MpClip c in e1.OldItems) {
                        RemoveClipTile(ClipTiles.Where(ct => ct.CopyItem == c).ToList()[0]);
                    }
                }
            };
            //create tiles for all clips in the database
            foreach(MpClip c in MpDataStore.Instance.ClipList) {
                AddClipTile(c);
            }
            //select first tile by default
            if(ClipTiles.Count > 0) {
                ClipTiles[0].IsSelected = true;
            }

            //when a tag is added or deleted reflect it in the tiles
            MpDataStore.Instance.TagList.CollectionChanged += (s2, e2) => {
                if(e2.NewItems != null) {
                    foreach(MpTag t in e2.NewItems) {
                        AddTagTile(t,true);
                    }
                }
                if(e2.OldItems != null) {
                    foreach(MpTag t in e2.OldItems) {
                        RemoveTagTile(TagTiles.Where(tt => tt.Tag == t).ToList()[0]);
                    }
                }
            };
            //create tiles for all the tags
            foreach(MpTag t in MpDataStore.Instance.TagList) {
                AddTagTile(t);
            }
            //select history tag by default
            TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;

        }

        private void AddTagTile(MpTag t, bool isNew = false) {
            var newTagTile = new MpTagTileViewModel(t);
            //watches Tag IsSelected so History is selected if none are
            newTagTile.PropertyChanged += (s, e) => {
                if(e.PropertyName == "IsSelected") {
                    var tagChanged = ((MpTagTileViewModel)s);
                    //ensure at least history is selected
                    if(tagChanged.IsSelected == false) {
                        //find all selected tag tiles
                        var selectedTagTiles = TagTiles.Where(tt => tt.IsSelected == true).ToList();
                        //if none selected select history tag
                        if(selectedTagTiles == null || selectedTagTiles.Count == 0) {
                            TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;
                        }
                    } else {
                        foreach(MpClipTileViewModel clipTile in ClipTiles) {
                            if(tagChanged.Tag.IsLinkedWithCopyItem(clipTile.CopyItem)) {
                                clipTile.Visibility = Visibility.Visible;
                            } else {
                                clipTile.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            };
            TagTiles.Add(newTagTile);
            if(isNew) {
                newTagTile.IsEditing = true;
            }
        }

        private void RemoveTagTile(MpTagTileViewModel tagTileToRemove) {
            if(tagTileToRemove.IsSelected) {
                tagTileToRemove.IsSelected = false;
                TagTiles.Where(tt => tt.Tag.TagName == "History").ToList()[0].IsSelected = true;
            }
            TagTiles.Remove(tagTileToRemove);
        }

        private void AddClipTile(MpClip ci) {
            //always make new cliptile the only one selected
            //first clear all selections
            foreach(var ct in ClipTiles) {
                ct.IsSelected = false;
            }
            //then create/add new tile with selected = true
            var newClipTile = new MpClipTileViewModel(ci);
            ClipTiles.Insert(0, newClipTile);
            newClipTile.IsSelected = true;
            //((Border)((MpMainWindow)Application.Current.MainWindow).FindName("Clip" + newClipTile.CopyItem.CopyItemId)).Focus();
        }

        private void RemoveClipTile(MpClipTileViewModel clipTileToRemove) {
            //when the clip is selected change selection to previous tile or next if it is first tile
            if(clipTileToRemove.IsSelected) {
                clipTileToRemove.IsSelected = false;
                if(ClipTiles.Count > 1) {
                    if(ClipTiles.IndexOf(clipTileToRemove) == 0) {
                        ClipTiles[1].IsSelected = true;
                    } else {
                        ClipTiles[ClipTiles.IndexOf(clipTileToRemove) - 1].IsSelected = true;
                    }
                }
            }
            ClipTiles.Remove(clipTileToRemove);
        }

        private DelegateCommand _addTagCommand;
        public ICommand AddTagCommand {
            get {
                if(_addTagCommand == null) {
                    _addTagCommand = new DelegateCommand(CreateNewTag);
                }
                return _addTagCommand;
            }
        }
        private void CreateNewTag() {
            //add tag to datastore so TagTile collection will automatically add the tile
            MpTag newTag = new MpTag("Untitled", MpHelperSingleton.Instance.GetRandomColor());
            newTag.WriteToDatabase();
            MpDataStore.Instance.TagList.Add(newTag);
        }

        private DelegateCommand _toggleAppendModeCommand;
        public ICommand ToggleAppendModeCommand {
            get {
                if(_toggleAppendModeCommand == null) {
                    _toggleAppendModeCommand = new DelegateCommand(ToggleAppendMode, CanToggleAppendMode);
                }
                return _toggleAppendModeCommand;
            }
        }
        private bool CanToggleAppendMode() {
            //only allow append mode to activate if app is not paused and only ONE clip is selected
            return !IsAppPaused && SelectedClipTiles.Count == 1;
        }
        private void ToggleAppendMode() {
            IsInAppendMode = !IsInAppendMode;
        }
        protected override void Loaded() {
            base.Loaded();
            PresentationSource source = PresentationSource.FromVisual(App.Current.MainWindow);

            double dpiX = 0, dpiY = 0;
            if(source != null) {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            var mw = Application.Current.MainWindow;
            mw.Width = SystemParameters.PrimaryScreenWidth;
            mw.Height = SystemParameters.PrimaryScreenHeight * 0.35;
            mw.Left = 0;
            mw.Top = SystemParameters.WorkArea.Height - mw.Height;

            ShowWindowCommand.Execute(null);
        }
    }
}