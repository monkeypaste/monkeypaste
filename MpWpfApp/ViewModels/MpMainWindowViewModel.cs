
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
                    OnPropertyChanged("ClipTiles");
                }
            }
        }
        private ObservableCollection<MpClipTileViewModel> _selectedTiles = new ObservableCollection<MpClipTileViewModel>();
        public ObservableCollection<MpClipTileViewModel> SelectedClipTiles {
            get {
                return _selectedTiles;
            }
            set {
                if(_selectedTiles != value) {
                    _selectedTiles = value;
                    OnPropertyChanged("SelectedClipTiles");
                }
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
                    OnPropertyChanged("TagTiles");
                }
            }
        }
        private ObservableCollection<MpTagTileViewModel> _selectedTagTiles = new ObservableCollection<MpTagTileViewModel>();
        public ObservableCollection<MpTagTileViewModel> SelectedTagTiles {
            get {
                return _selectedTagTiles;
            }
            set {
                if(_selectedTagTiles != value) {
                    _selectedTagTiles = value;
                    OnPropertyChanged("SelectedTagTiles");
                }
            }
        }

        public MpMainWindowViewModel() {
            base.DisplayName = "MpMainWindowViewModel";
            MpDataStore.Instance.Init();

            MpDataStore.Instance.ClipList.CollectionChanged += (s, e) => {
                foreach(MpClip c in e.NewItems) {
                    AddClipTile(c);
                }
            };
            MpDataStore.Instance.TagList.CollectionChanged += (s, e) => {
                foreach(MpTag t in e.NewItems) {
                    AddTagTile(t);
                }
            };
            foreach(MpClip c in MpDataStore.Instance.ClipList) {
                AddClipTile(c);
            }
            foreach(MpTag t in MpDataStore.Instance.TagList) {
                AddTagTile(t);
            }

            SelectedTagTiles.CollectionChanged += (s, e) => {
                //clear all tile visibility
                foreach(MpClipTileViewModel clipTile in ClipTiles) {
                    clipTile.Visibility = Visibility.Collapsed;
                }
                if(SelectedTagTiles.Count == 0) {
                    SelectedTagTiles.Add(TagTiles[0]);
                }
                //loop through all clip tiles for all selected tags to set the clip tiles visibilty
                foreach(MpTagTileViewModel tagTile in TagTiles) {
                    if(tagTile.IsSelected) {
                        foreach(MpClipTileViewModel clipTile in ClipTiles) {
                            if(tagTile.Tag.IsLinkedWithCopyItem(clipTile.CopyItem)) {
                                clipTile.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            };
        }
        public void TileClicked(MpClipTileViewModel clickedTile) { 
        }
        private void AddTagTile(MpTag t) {
            var newTagTile = new MpTagTileViewModel(t);
            TagTiles.Add(newTagTile);
        }
        private void AddClipTile(MpClip ci) {
            var newTile = new MpClipTileViewModel(ci);
            ClipTiles.Insert(0, newTile);

        }

        protected override void Loaded() {
            base.Loaded();
            PresentationSource source = PresentationSource.FromVisual(App.Current.MainWindow);

            double dpiX=0, dpiY=0;
            if(source != null) {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            var mw = Application.Current.MainWindow;
            mw.Width = SystemParameters.PrimaryScreenWidth;
            mw.Height = SystemParameters.PrimaryScreenHeight * 0.35;
            mw.Left = 0;
            mw.Top = SystemParameters.WorkArea.Height - mw.Height;
            Console.WriteLine("Workarea left: " +SystemParameters.WorkArea.Left+" top: "+ SystemParameters.WorkArea.Top);
            //mw.Left = MpMeasurements.Instance.MainWindowRect.X;
            //mw.Top = MpMeasurements.Instance.MainWindowRect.Y;
            //mw.Width = MpMeasurements.Instance.MainWindowRect.Width;
            //mw.Height = MpMeasurements.Instance.MainWindowRect.Height;

            //((RowDefinition)mw.FindName("ClipTrayTitleMenuPanel")).Height = new GridLength(MpMeasurements.Instance.TitleMenuHeight);
            //((RowDefinition)mw.FindName("ClipTrayFilterMenuPanel")).Height = new GridLength(MpMeasurements.Instance.FilterMenuHeight);

            //((ColumnDefinition)mw.FindName("AppStateButtonGrid")).Width = new GridLength(MpMeasurements.Instance.AppStateButtonPanelWidth);


            //((StackPanel)mw.FindName("ClipTileListBoxItem")).Width = MpMeasurements.Instance.TileSize;
            //((StackPanel)mw.FindName("ClipTileListBoxItem")).Height = MpMeasurements.Instance.TileSize;

            ShowWindowCommand.Execute(null);
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
    }
}