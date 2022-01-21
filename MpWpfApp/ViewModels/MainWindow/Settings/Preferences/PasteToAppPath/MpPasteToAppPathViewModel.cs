using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace MpWpfApp {
    public class MpPasteToAppPathViewModel : MpViewModelBase<MpPasteToAppPathViewModelCollection> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Properties
        public Brush PasteToAppPathDataRowBorderBrush {
            get {
                if(IsValid) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        }
        #endregion

        #region Business Logic
        private int _selectedWindowState = -1;
        public int SelectedWindowState {
            get {
                if(PasteToAppPath == null) {
                    return 0;
                }
                if(_selectedWindowState < 0) {
                    _selectedWindowState = (int)PasteToAppPath.WindowState;
                }
                return _selectedWindowState;
            }
            set {
                if(_selectedWindowState != value) {
                    _selectedWindowState = value;
                    WindowState = (MpProcessHelper.WinApi.ShowWindowCommands)_selectedWindowState;
                    OnPropertyChanged(nameof(SelectedWindowState));
                }
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if(_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(IsReadOnly));
                }
            }
        }

        public bool IsReadOnly {
            get {
                return !IsSelected;
            }
        }

        private bool _isValid = false;
        public bool IsValid {
            get {
                return _isValid;
            }
            set {
                if (_isValid != value) {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                    OnPropertyChanged(nameof(PasteToAppPathDataRowBorderBrush));
                }
            }
        }

        private bool _isRuntime = false;
        public bool IsRuntime {
            get {
                return _isRuntime;
            }
            set {
                if (_isRuntime != value) {
                    _isRuntime = value;
                    OnPropertyChanged(nameof(IsRuntime));
                }
            }
        }

        private bool _isHidden = false;
        public bool IsHidden {
            get {
                return _isHidden;
            }
            set {
                if (_isHidden != value) {
                    _isHidden = value;
                    OnPropertyChanged(nameof(IsHidden));
                }
            }
        }

        private IntPtr _handle = IntPtr.Zero;
        public IntPtr Handle {
            get {
                return _handle;
            }
            set {
                if(_handle != value) {
                    _handle = value;
                    OnPropertyChanged(nameof(Handle));
                }
            }
        }
        #endregion

        #region Model Properties
        public bool PressEnter {
            get {
                if(PasteToAppPath == null) {
                    return false;
                }
                return PasteToAppPath.PressEnter;
            }
            set {
                if(PasteToAppPath != null && PasteToAppPath.PressEnter != value) {
                    PasteToAppPath.PressEnter = value;
                    Task.Run(async () => { await PasteToAppPath.WriteToDatabaseAsync(); });
                    OnPropertyChanged(nameof(PressEnter));
                }
            }
        }

        public MpProcessHelper.WinApi.ShowWindowCommands WindowState {
            get {
                if(PasteToAppPath == null) {
                    return MpProcessHelper.WinApi.ShowWindowCommands.Normal;
                }
                return (MpProcessHelper.WinApi.ShowWindowCommands)PasteToAppPath.WindowState;
            }
            set {
                if (PasteToAppPath.WindowState != (int)value) {
                    PasteToAppPath.WindowState = (int)value;
                    Task.Run(async () => { await PasteToAppPath.WriteToDatabaseAsync(); });
                    OnPropertyChanged(nameof(WindowState));
                }
            }
        }
        public BitmapSource AppIcon {
            get {
                if (PasteToAppPath == null) {
                    return new BitmapImage();
                }
                var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == AvatarId);
                if(ivm == null) {
                    return MpProcessHelper.MpProcessManager.ProcessIconBuilder.GetBase64BitmapFromFilePath(AppPath).ToBitmapSource();
                }
                return ivm.IconBitmapSource;
            }
            set {
                if(PasteToAppPath != null) {
                    Task.Run(
                        async () => {
                            var dbi = await MpDbImage.Create(value.ToBase64String());
                            PasteToAppPath.AvatarDbImage = dbi;
                            PasteToAppPath.AvatarId = dbi.Id;
                            await PasteToAppPath.AvatarDbImage.WriteToDatabaseAsync();
                            await PasteToAppPath.WriteToDatabaseAsync();
                        });
                    OnPropertyChanged(nameof(AppIcon));
                }
            }
        }

        public string Args {
            get {
                if(PasteToAppPath == null) {
                    return string.Empty;
                }
                if (PasteToAppPath.Args == null) {
                    return string.Empty;
                }
                return PasteToAppPath.Args;
            }
            set {
                if(PasteToAppPath != null && PasteToAppPath.Args != value) {
                    PasteToAppPath.Args = value;
                    Task.Run(async () => { await PasteToAppPath.WriteToDatabaseAsync(); });
                    OnPropertyChanged(nameof(Args));
                }
            }
        }

        public string Label {
            get {
                if (PasteToAppPath == null) {
                    return string.Empty;
                }
                //if(string.IsNullOrEmpty(PasteToAppPath.Label)) {
                //    return AppName;
                //}
                return PasteToAppPath.Label;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.Label != value) {
                    PasteToAppPath.Label = value;
                    Task.Run(async () => { await PasteToAppPath.WriteToDatabaseAsync(); });
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public bool IsSilent {
            get {
                if (PasteToAppPath == null) {
                    return false;
                }
                return PasteToAppPath.IsSilent;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.IsSilent != value) {
                    PasteToAppPath.IsSilent = value;
                    Task.Run(async () => { await PasteToAppPath.WriteToDatabaseAsync(); });
                    OnPropertyChanged(nameof(IsSilent));
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        public bool IsAdmin {
            get {
                if (PasteToAppPath == null) {
                    return false;
                }
                return PasteToAppPath.IsAdmin;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.IsAdmin != value) {
                    PasteToAppPath.IsAdmin = value;
                    Task.Run(async () => { await PasteToAppPath.WriteToDatabaseAsync(); });
                    OnPropertyChanged(nameof(IsAdmin));
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        public string AppName {
            get {
                if (PasteToAppPath == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(PasteToAppPath.AppName)) {
                    return Path.GetFileName(PasteToAppPath.AppPath) + (IsAdmin ? " (Admin)":string.Empty) + (IsSilent ? " (Silent)" : string.Empty);
                }
                return PasteToAppPath.AppName + (IsAdmin ? " (Admin)" : string.Empty) + (IsSilent ? " (Silent)" : string.Empty);
            }
            set {
                if(PasteToAppPath.AppName != value && PasteToAppPath.AppPath != value) {
                    PasteToAppPath.AppName = value;
                    Task.Run(async () => { await PasteToAppPath.WriteToDatabaseAsync(); });
                    OnPropertyChanged(nameof(AppName));
                }
            }
        } 

        public string AppPath {
            get {
                if (PasteToAppPath == null) {
                    return String.Empty;
                }
                return PasteToAppPath.AppPath;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.AppPath != value) {
                    PasteToAppPath.AppPath = value;
                    Task.Run(async () => { await PasteToAppPath.WriteToDatabaseAsync(); });
                    OnPropertyChanged(nameof(AppPath));
                }
            }
        }

        public int AvatarId {
            get {
                if(PasteToAppPath == null) {
                    return 0;
                }
                return PasteToAppPath.AvatarId;
            }
            set {
                if(AvatarId != value) {
                    PasteToAppPath.AvatarId = value;
                    OnPropertyChanged(nameof(AvatarId));
                }
            }
        }

        public int PasteToAppPathId {
            get {
                if(PasteToAppPath == null) {
                    return 0;
                }
                return PasteToAppPath.PasteToAppPathId;
            }
            set {
                if(PasteToAppPath != null && PasteToAppPath.PasteToAppPathId != value) {
                    PasteToAppPath.PasteToAppPathId = value;
                    OnPropertyChanged(nameof(PasteToAppPathId));
                }
            }
        }

        private MpPasteToAppPath _pasteToAppPath;
        public MpPasteToAppPath PasteToAppPath {
            get {
                return _pasteToAppPath;
            }
            set {
                if(_pasteToAppPath != value) {
                    _pasteToAppPath = value;
                    OnPropertyChanged(nameof(PasteToAppPath));
                    OnPropertyChanged(nameof(PasteToAppPathId));
                    OnPropertyChanged(nameof(AppPath));
                    OnPropertyChanged(nameof(IsAdmin));
                    OnPropertyChanged(nameof(WindowState));
                    OnPropertyChanged(nameof(IsSilent));
                    OnPropertyChanged(nameof(AppName));
                    OnPropertyChanged(nameof(AppIcon));
                    OnPropertyChanged(nameof(Args));
                    OnPropertyChanged(nameof(Label));
                    OnPropertyChanged(nameof(PressEnter));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpPasteToAppPathViewModel() : base(null) { }

        public MpPasteToAppPathViewModel(MpPasteToAppPathViewModelCollection parent, MpPasteToAppPath pasteToAppPath, IntPtr handle) : base(parent) {
            //constructor used for running applications
            PasteToAppPath = pasteToAppPath;
            if(handle != IntPtr.Zero) {
                Handle = handle;
                IsRuntime = true;
            }            
        }

        public MpPasteToAppPathViewModel(MpPasteToAppPathViewModelCollection parent, MpPasteToAppPath pasteToAppPath) : base(parent) {
            //constructor used for user defined paste to applications
            PasteToAppPath = pasteToAppPath;
        }

        public string Validate() {
            if (string.IsNullOrEmpty(Args)) {
                return string.Empty;
            }
            if(Args.Length > MpPreferences.MaxCommandLineArgumentLength) {
                return @"Max length of Commandline args is " + MpPreferences.MaxCommandLineArgumentLength + " this is " + Args.Length;
            }
            return string.Empty;
        }
        public async Task DisposeAsync() {
            base.Dispose();
            await PasteToAppPath.DeleteFromDatabaseAsync();
        }
        #endregion

        #region Commands
        public ICommand ChangeIconCommand => new RelayCommand<object>(
            (param) => {
                var iconColorChooserMenuItem = new MenuItem();
                var iconContextMenu = new ContextMenu();
                iconContextMenu.Items.Add(iconColorChooserMenuItem);
                MpHelpers.SetColorChooserMenuItem(
                    iconContextMenu,
                    iconColorChooserMenuItem,
                    (s1, e1) => {
                        var brush = (Brush)((Border)s1).Tag;
                        var bmpSrc = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/texture.png"));
                        AppIcon = MpWpfImagingHelper.TintBitmapSource(bmpSrc, ((SolidColorBrush)brush).Color);
                    }
                );
                var iconImageChooserMenuItem = new MenuItem();
                iconImageChooserMenuItem.Header = "Choose Image...";
                iconImageChooserMenuItem.Icon = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/image_icon.png")) };
                iconImageChooserMenuItem.Click += (s, e) => {
                    var openFileDialog = new OpenFileDialog() {
                        Filter = "Image|*.png;*.gif;*.jpg;*.jpeg;*.bmp",
                        Title = "Select Image for " + Label,
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    };
                    bool? openResult = openFileDialog.ShowDialog();
                    if (openResult != null && openResult.Value) {
                        string imagePath = openFileDialog.FileName;
                        AppIcon = (BitmapSource)new BitmapImage(new Uri(imagePath));
                    }
                };
                iconContextMenu.Items.Add(iconImageChooserMenuItem);
                ((Button)param).ContextMenu = iconContextMenu;
                iconContextMenu.PlacementTarget = ((Button)param);
                iconContextMenu.IsOpen = true;
            });
        #endregion
    }
}
