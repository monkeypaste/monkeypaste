using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using Xamarin.Forms.Internals;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MpWpfApp {
    public class MpPasteToAppPathViewModelCollection : ObservableCollection<MpPasteToAppPathViewModel>, INotifyPropertyChanged {
        private static readonly Lazy<MpPasteToAppPathViewModelCollection> _Lazy = new Lazy<MpPasteToAppPathViewModelCollection>(() => new MpPasteToAppPathViewModelCollection());
        public static MpPasteToAppPathViewModelCollection Instance { get { return _Lazy.Value; } }

        #region Private Variables

        #endregion

        #region View Models
        private ObservableCollection<ObservableCollection<MpPasteToAppPathViewModel>> _menuItemViewModels = null;
        public ObservableCollection<ObservableCollection<MpPasteToAppPathViewModel>> MenuItemViewModels {
            get {
                if (_menuItemViewModels == null) {
                    _menuItemViewModels = new ObservableCollection<ObservableCollection<MpPasteToAppPathViewModel>>();
                    foreach (var kvp in MpRunningApplicationManager.Instance.CurrentProcessWindowHandleStackDictionary) {
                        var appName = MpHelpers.Instance.GetProcessApplicationName(kvp.Value[0]);
                        if (kvp.Value.Count == 0 || string.IsNullOrEmpty(appName)) {
                            continue;
                        }
                        var processPath = kvp.Key;
                        var processHandles = new ObservableCollection<MpPasteToAppPathViewModel>();
                        foreach (var handle in kvp.Value) {
                            processHandles.Add(
                                new MpPasteToAppPathViewModel(
                                    this,
                                    new MpPasteToAppPath(
                                        processPath,
                                        MpHelpers.Instance.GetProcessMainWindowTitle(handle),
                                        MpHelpers.Instance.GetIconImage(processPath).ToBase64String(),
                                        MpHelpers.Instance.IsProcessAdmin(handle)),
                                    handle));
                        }
                        //check already created menu items and add handles to AppName if it already exists
                        int mivmIdx = -1;
                        foreach (var mivm in _menuItemViewModels) {
                            if (mivm.Count == 0) {
                                continue;
                            }
                            if (appName.ToLower() == MpHelpers.Instance.GetProcessApplicationName(mivm[0].Handle).ToLower()) {
                                mivmIdx = _menuItemViewModels.IndexOf(mivm);
                            }
                        }
                        if (mivmIdx >= 0) {
                            foreach (var ph in processHandles) {
                                _menuItemViewModels[mivmIdx].Add(ph);
                            }
                        } else {
                            _menuItemViewModels.Add(processHandles);
                        }
                    }
                    foreach (var ptapvm in this) {
                        _menuItemViewModels.Add(new ObservableCollection<MpPasteToAppPathViewModel>() { ptapvm });
                    }
                }
                return _menuItemViewModels;
            }
        }

        #endregion

        #region Properties
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel;
        public MpPasteToAppPathViewModel SelectedPasteToAppPathViewModel {
            get {
                return _selectedPasteToAppPathViewModel;
            }
            set {
                if (_selectedPasteToAppPathViewModel != value) {
                    _selectedPasteToAppPathViewModel = value;
                    OnPropertyChanged(nameof(SelectedPasteToAppPathViewModel));
                }
            }
        }

        private string _validationText = string.Empty;
        private string ValidationText {
            get {
                return _validationText;
            }
            set {
                if (_validationText != value) {
                    _validationText = value;
                    OnPropertyChanged(nameof(ValidationText));
                }
            }
        }
        #endregion

        #region Public Methods
        private MpPasteToAppPathViewModelCollection() : base() {
            MpRunningApplicationManager.Instance.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(MpRunningApplicationManager.Instance.CurrentProcessWindowHandleStackDictionary):
                        _menuItemViewModels = null;
                        OnPropertyChanged(nameof(MenuItemViewModels));
                        break;
                }
            };

            foreach (var ptap in MpPasteToAppPath.GetAllPasteToAppPaths()) {
                this.Add(new MpPasteToAppPathViewModel(this,ptap));
            }
        }
        public void PasteToAppPathDataGrid_Loaded(object sender, RoutedEventArgs args) {
            var dg = (DataGrid)sender;
            dg.SelectionChanged += (s, e) => {
                foreach (var ptapvm in this) {
                    ptapvm.IsSelected = ptapvm == SelectedPasteToAppPathViewModel ? true : false;
                }
            };
        }

        public ContextMenu UpdatePasteToMenuItem(ContextMenu cm) {
            MenuItem ptamir = null;
            foreach (var mi in cm.Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if ((mi as MenuItem).Name == "PasteToAppPathMenuItem") {
                    ptamir = mi as MenuItem;
                }
            }
            if (ptamir == null) {
                return cm;
            }
            ICommand pasteCommand = null;
            //if (cm.DataContext is MpRtbListBoxItemRichTextBoxViewModel) {
            //    pasteCommand = (cm.DataContext as MpRtbListBoxItemRichTextBoxViewModel).RichTextBoxViewModelCollection.PasteSubSelectedClipsCommand;
            //} else {
            //    pasteCommand = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
            //}
            pasteCommand = MpClipTrayViewModel.Instance.PasteSelectedClipsCommand;
            ptamir.Items.Clear();
            bool addedSeperator = false;
            foreach (var ptamivmc in MpPasteToAppPathViewModelCollection.Instance.MenuItemViewModels) {
                if (ptamivmc.Count == 0) {
                    continue;
                }
                if (ptamivmc[0].IsRuntime) {
                    bool areAllHidden = true;
                    foreach (var ptamivm in ptamivmc) {
                        if (!ptamivm.IsHidden) {
                            areAllHidden = false;
                        }
                    }
                    if (areAllHidden) {
                        continue;
                    }
                    var ptamip = new MenuItem();
                    ptamip.Header = MpHelpers.Instance.GetProcessApplicationName(ptamivmc[0].Handle);
                    ptamip.Icon = new Image() { Source = ptamivmc[0].AppIcon };
                    foreach (var ptamivm in ptamivmc) {
                        if (ptamivm.IsHidden) {
                            continue;
                        }
                        var ptami = new MenuItem();
                        var l = new Label();
                        l.Content = MpHelpers.Instance.GetProcessMainWindowTitle(ptamivm.Handle) + (ptamivm.IsAdmin ? " (Admin)" : string.Empty);

                        var eyeOpenImg = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/eye.png")) };
                        var eyeClosedImg = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/eye_closed.png")) };
                        var btn = new Button() { Cursor = Cursors.Hand, Content = eyeOpenImg, BorderThickness = new Thickness(0), Background = Brushes.Transparent, Width = 20, Height = 20, HorizontalAlignment = HorizontalAlignment.Right/*, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center*/ };
                        bool isOverButton = false;
                        MouseEventHandler mouseEnter = (object o, MouseEventArgs e) => {
                            btn.Content = eyeClosedImg;
                            isOverButton = true;
                        };
                        btn.MouseEnter += mouseEnter;

                        MouseEventHandler mouseLeave = (object o, MouseEventArgs e) => {
                            btn.Content = eyeOpenImg;
                            isOverButton = false;
                        };
                        btn.MouseLeave += mouseLeave;

                        RoutedEventHandler btnClick = (object o, RoutedEventArgs e) => {
                            ptamivm.IsHidden = true;
                            ptamip.Items.Remove(ptami);
                            if (ptamip.Items.Count == 0) {
                                ptamir.Items.Remove(ptamip);
                            }
                        };
                        btn.Click += btnClick;

                        RoutedEventHandler btnUnload = null;
                        
                        btnUnload = (object o, RoutedEventArgs e) => {
                            btn.MouseEnter -= mouseEnter;
                            btn.MouseLeave -= mouseLeave;
                            btn.Click -= btnClick;
                            btn.Unloaded -= btnUnload;
                        };

                        btn.Unloaded += btnUnload;

                        var sp = new StackPanel() { Orientation = Orientation.Horizontal };
                        sp.Children.Add(l);
                        sp.Children.Add(btn);

                        ptami.Header = sp;
                        ptami.Icon = new Image() { Source = ptamivm.AppIcon };
                        //ptami.Command = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
                        //ptami.CommandParameter = ptamivm.Handle;

                        RoutedEventHandler ptamiClick = (object o, RoutedEventArgs e) => {
                            if (!isOverButton) {
                                pasteCommand.Execute(ptamivm.Handle);
                            }
                        };
                        ptami.Click += ptamiClick;

                        RoutedEventHandler ptamiUnload = null;
                        ptamiUnload = (object o, RoutedEventArgs e) => {
                            ptami.Click -= ptamiClick;
                            ptami.Unloaded -= ptamiUnload;
                        };
                        ptami.Unloaded += ptamiUnload;

                        ptamip.Items.Add(ptami);
                    }
                    ptamir.Items.Add(ptamip);
                } else {
                    if (!addedSeperator) {
                        ptamir.Items.Add(new Separator());
                        addedSeperator = true;
                    }
                    var ptaumi = new MenuItem();
                    ptaumi.Header = ptamivmc[0].AppName;// + (ptamivmc[0].IsAdmin ? " (Admin)" : string.Empty) + (ptamivmc[0].IsSilent ? " (Silent)" : string.Empty);
                    ptaumi.Icon = new Image() { Source = ptamivmc[0].AppIcon };
                    ptaumi.Command = pasteCommand;
                    ptaumi.CommandParameter = ptamivmc[0].PasteToAppPathId;

                    ptamir.Items.Add(ptaumi);
                }
            }
            var addNewMenuItem = new MenuItem();
            addNewMenuItem.Header = "Add Application...";
            addNewMenuItem.Icon = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Icons/Silk/icons/add.png")) };
            RoutedEventHandler addNewMenuItemClick = (object o, RoutedEventArgs e) => {
                MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand.Execute(1);
            };
            addNewMenuItem.Click += addNewMenuItemClick;

            RoutedEventHandler addNewMenuItemUnload = null;
            addNewMenuItemUnload = (object o, RoutedEventArgs e) => {
                addNewMenuItem.Click -= addNewMenuItemClick;
                addNewMenuItem.Unloaded -= addNewMenuItemUnload;
            };
            ptamir.Items.Add(addNewMenuItem);

            return cm;
        }

        public new void Add(MpPasteToAppPathViewModel ptapvm) {
            base.Add(ptapvm);
        }

        public new void Remove(MpPasteToAppPathViewModel ptapvm) {
            if (this.Contains(ptapvm)) {
                base.Remove(ptapvm);
                ptapvm.Dispose();
            }
        }

        public MpPasteToAppPathViewModel FindById(int ptapid) {
            return this.Where(x => x.PasteToAppPathId == ptapid).First();
        }
        #endregion

        #region Private Methods
        private bool Validate() {
            ValidationText = string.Empty;
            foreach (var ptapvm in this) {
                ValidationText += ptapvm.Validate();
            }
            return string.IsNullOrEmpty(ValidationText);
        }
        #endregion

        #region Commands    
        private RelayCommand _deletePasteToAppPathCommand;
        public ICommand DeletePasteToAppPathCommand {
            get {
                if (_deletePasteToAppPathCommand == null) {
                    _deletePasteToAppPathCommand = new RelayCommand(DeletePasteToAppPath);
                }
                return _deletePasteToAppPathCommand;
            }
        }
        private void DeletePasteToAppPath() {
            this.Remove(SelectedPasteToAppPathViewModel);
        }

        private RelayCommand<object> _addPasteToAppPathCommand;
        public ICommand AddPasteToAppPathCommand {
            get {
                if (_addPasteToAppPathCommand == null) {
                    _addPasteToAppPathCommand = new RelayCommand<object>(AddPasteToAppPath);
                }
                return _addPasteToAppPathCommand;
            }
        }
        private void AddPasteToAppPath(object args) {
            string appPath = string.Empty;
            if (args is MpApp) {
                appPath = (args as MpApp).AppPath;
                if (!File.Exists(appPath)) {
                    Console.WriteLine("AddPasteToAppPath error, appPath does not exist: " + appPath);
                    return;
                }
            } else {
                var openFileDialog = new OpenFileDialog() {
                    Filter = "Applications|*.lnk;*.exe",
                    Title = "Select application path",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                bool? openResult = openFileDialog.ShowDialog();
                if (openResult != null && openResult.Value) {
                    appPath = openFileDialog.FileName;
                    if (Path.GetExtension(openFileDialog.FileName).Contains("lnk")) {
                        appPath = MpHelpers.Instance.GetShortcutTargetPath(openFileDialog.FileName);
                    }
                }
            }

            var nptapvm = new MpPasteToAppPathViewModel(this,new MpPasteToAppPath(
                appPath, string.Empty, MpHelpers.Instance.GetIconImage(appPath).ToBase64String(),false));
            nptapvm.PasteToAppPath.WriteToDatabase();
            this.Add(nptapvm);

            SelectedPasteToAppPathViewModel = nptapvm;
            Validate();
        }
        #endregion


        #region INotifyPropertyChanged 
        public bool ThrowOnInvalidPropertyName { get; private set; }

        protected override event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName) {
            // Verify that the property name matches a real, 
            // public, instance property on this object. 
            if (TypeDescriptor.GetProperties(this)[propertyName] == null) {
                string msg = "Invalid property name: " + propertyName;
                if (this.ThrowOnInvalidPropertyName) {
                    throw new Exception(msg);
                } else {
                    Debug.Fail(msg);
                }
            }
        }
        #endregion
    }
}
