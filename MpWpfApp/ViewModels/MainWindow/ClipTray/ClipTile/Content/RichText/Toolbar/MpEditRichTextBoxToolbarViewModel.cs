using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MonkeyPaste;

namespace MpWpfApp {

    public class MpEditRichTextBoxToolbarViewModel : MpUndoableViewModelBase<MpEditRichTextBoxToolbarViewModel>, IDisposable {
        #region Private Variables
        #endregion        

        #region Properties

        #region View Models
        private MpClipTileViewModel _hostClipTileViewModel = null;
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                return _hostClipTileViewModel;
            }
            set {
                if (_hostClipTileViewModel != value) {
                    _hostClipTileViewModel = value;
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                }
            }
        }

        public MpRtbListBoxItemRichTextBoxViewModel SubSelectedRtbViewModel {
            get {
                if (HostClipTileViewModel == null) {
                    return null;
                }
                if (HostClipTileViewModel.RichTextBoxViewModelCollection.Count == 0 ||
                   HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems.Count != 1) {
                    return null;
                }
                return HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems[0];
            }
        }
        #endregion

        #region Controls
        //public Border EditToolbarBorder { get; set; }

        //public StackPanel BorderStackPanel { get; set; }
        //public RichTextBox LastRtb { get; set; }
        //public RichTextBox SelectedRtb { get; set; }
        #endregion

        #region Layout Properties      
        private double _editBorderCanvasTop = -MpMeasurements.Instance.ClipTileEditToolbarHeight;
        public double EditBorderCanvasTop {
            get {
                return _editBorderCanvasTop;
            }
            set {
                if (_editBorderCanvasTop != value) {
                    _editBorderCanvasTop = value;
                    OnPropertyChanged(nameof(EditBorderCanvasTop));
                }
            }
        }

        private double _editBorderWidth = -MpMeasurements.Instance.ClipTileBorderMinSize;
        public double EditBorderWidth {
            get {
                return _editBorderWidth;
            }
            set {
                if (_editBorderWidth != value) {
                    _editBorderWidth = value;
                    OnPropertyChanged(nameof(EditBorderWidth));
                }
            }
        }
        #endregion

        #region Visibility Properties

        #endregion

        #region Brush Properties
        public Brush AddTemplateButtonBackgroundBrush {
            get {
                if (IsAddTemplateButtonEnabled) {
                    return Brushes.Transparent;
                }
                return Brushes.LightGray;
            }
        }
        #endregion

        #region State Properties
        private bool _useSpellCheck = Properties.Settings.Default.UseSpellCheck;
        public bool UseSpellCheck {
            get {
                return _useSpellCheck;
            }
            set {
                if (_useSpellCheck != value) {
                    _useSpellCheck = value;
                    OnPropertyChanged(nameof(UseSpellCheck));
                }
            }
        }

        private bool _hasTextChanged = false;
        public bool HasTextChanged {
            get {
                return _hasTextChanged;
            }
            set {
                if(_hasTextChanged != value) {
                    _hasTextChanged = value;
                    OnPropertyChanged(nameof(HasTextChanged));
                }
            }
        }

        private bool _isAddTemplateButtonEnabled = true;
        public bool IsAddTemplateButtonEnabled {
            get {
                return _isAddTemplateButtonEnabled;
            }
            set {
                if (_isAddTemplateButtonEnabled != value) {
                    _isAddTemplateButtonEnabled = value;
                    OnPropertyChanged(nameof(IsAddTemplateButtonEnabled));
                    OnPropertyChanged(nameof(AddTemplateButtonBackgroundBrush));
                }
            }
        }
        #endregion

        #region Business Logic Properties
        public MpObservableCollection<FontFamily> SystemFonts {
            get {
                return new MpObservableCollection<FontFamily>(Fonts.SystemFontFamilies);
            }
        }

        private MpObservableCollection<string> _fontSizes = null;
        public MpObservableCollection<string> FontSizes {
            get {
                if (_fontSizes == null) {
                    _fontSizes = new MpObservableCollection<string>() {
                         "8",
                        "9",
                        "10",
                        "11",
                        "12",
                        "14",
                        "16",
                        "18",
                        "20",
                        "22",
                        "24",
                        "26",
                        "28",
                        "36",
                        "48",
                        "72"
                };
                }
                return _fontSizes;
            }
            set {
                if (_fontSizes != value) {
                    _fontSizes = value;
                    OnPropertyChanged(nameof(FontSizes));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpEditRichTextBoxToolbarViewModel() :base() { }

        public MpEditRichTextBoxToolbarViewModel(MpClipTileViewModel ctvm) : base() {
            HostClipTileViewModel = ctvm;
        }

        public void ClipTileEditorToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            if(HostClipTileViewModel == null) {
                return;
            }
            if (HostClipTileViewModel.CopyItemType != MpCopyItemType.RichText) {
                return;
            }
            //BorderStackPanel = (StackPanel)sender;
            //EditToolbarBorder = BorderStackPanel.GetVisualAncestor<Border>();
        }

        public void Resize(double deltaEditToolbarTop, double deltaWidth) {
            if(deltaEditToolbarTop > 0) {
                HostClipTileViewModel.EditToolbarVisibility = Visibility.Visible;
            } else {
                HostClipTileViewModel.EditToolbarVisibility = Visibility.Collapsed;
            }
            //EditBorderCanvasTop += deltaEditToolbarTop;
            //Canvas.SetTop(EditToolbarBorder, EditBorderCanvasTop);
            //EditBorderWidth += deltaWidth;
            //EditToolbarBorder.Width += deltaWidth;

            if (HostClipTileViewModel.IsEditingTile) {
                //var ctv = (Application.Current.MainWindow as MpMainWindow).FindName("ClipTray") as MpClipTrayView;
                //ctv.ClipTray.ScrollIntoView(HostClipTileViewModel);
                HostClipTileViewModel.RichTextBoxViewModelCollection.ResetSubSelection();
                //Rtb_SelectionChanged(this, new RoutedEventArgs());
            } else if(!HostClipTileViewModel.IsPastingTemplate) {
                HostClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();
                //ClipTileViewModel.RichTextBoxViewModelCollection.Refresh();
            }
        }

        public void Animate(
            double deltaTop, 
            double tt, 
            EventHandler onCompleted, 
            double fps = 30,
            DispatcherPriority priority = DispatcherPriority.Render) {
            double fromTop = EditBorderCanvasTop;
            double toTop = fromTop + deltaTop;
            double dt = (deltaTop / tt) / fps;

            var timer = new DispatcherTimer(priority);
            timer.Interval = TimeSpan.FromMilliseconds(fps);
            timer.Tick += (s, e32) => {
                if (MpHelpers.Instance.DistanceBetweenValues(EditBorderCanvasTop, toTop) > 0.5) {
                    EditBorderCanvasTop += dt;
                    //Canvas.SetTop(EditToolbarBorder, EditBorderCanvasTop);
                } else {
                    timer.Stop();
                    if (HostClipTileViewModel.IsEditingTile) {
                        var ctv = (Application.Current.MainWindow as MpMainWindow).FindName("ClipTray") as MpClipTrayView;
                        ctv.ClipTray.ScrollIntoView(HostClipTileViewModel);
                        HostClipTileViewModel.RichTextBoxViewModelCollection.ResetSubSelection();
                        //Rtb_SelectionChanged(this, new RoutedEventArgs());
                    } else {
                        HostClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();
                        HostClipTileViewModel.RichTextBoxViewModelCollection.Refresh();
                    }
                    if (onCompleted != null) {
                        onCompleted.BeginInvoke(this, new EventArgs(), null, null);
                    }
                }
            };
            timer.Start();
        }
        #endregion

        #region Private Methods 
       
        #endregion

        #region Commands
        private RelayCommand _refreshDocumentCommand = null;
        public ICommand RefreshDocumentCommand {
            get {
                if(_refreshDocumentCommand == null) {
                    _refreshDocumentCommand = new RelayCommand(RefreshDocument,CanRefreshDocument);
                }
                return _refreshDocumentCommand;
            }
        }
        private bool CanRefreshDocument() {
            return HasTextChanged && 
                   HostClipTileViewModel != null && 
                   HostClipTileViewModel.IsEditingTile &&
                   HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 1;
        }
        private void RefreshDocument() {
            HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems[0].SaveSubItemToDatabase();
            //HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems[0].ClearHyperlinks();
            //HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems[0].CreateHyperlinks();
        }
        #endregion

        #region IDisposable
        public void Dispose() {
            //EditToolbarBorder = null;
        }
        #endregion
    }
}
