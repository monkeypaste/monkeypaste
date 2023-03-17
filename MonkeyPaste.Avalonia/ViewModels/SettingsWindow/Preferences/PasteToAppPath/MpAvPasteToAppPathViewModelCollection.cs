
using Microsoft.Win32;
using MonkeyPaste;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MonkeyPaste.Avalonia {
    public class MpAvPasteToAppPathViewModelCollection :
        MpSelectorViewModelBase<MpAvPasteToAppPathViewModelCollection, MpAvPasteToAppPathViewModel>,
        MpIMenuItemViewModel,
        MpIAsyncSingletonViewModel<MpAvPasteToAppPathViewModelCollection>,
        INotifyPropertyChanged {

        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                var itemsToRemove = Items.Where(x => x.IsRuntime).ToList();
                for (int i = 0; i < itemsToRemove.Count; i++) {
                    //clear running applications so they aren't duplicated but are current
                    Items.Remove(itemsToRemove[i]);
                }
                var pmivml = new List<MpMenuItemViewModel>();
                foreach (var kvp in Mp.Services.ProcessWatcher.RunningProcessLookup) {
                    var appName = Mp.Services.ProcessWatcher.GetProcessApplicationName(kvp.Value[0]);
                    if (kvp.Value.Count == 0 || string.IsNullOrEmpty(appName)) {
                        continue;
                    }
                    var processPath = kvp.Key;
                    var processName = Mp.Services.ProcessWatcher.GetProcessApplicationName(kvp.Value[0]);
                    var avm = MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppPath.ToLower() == processPath.ToLower());
                    var rpmivm = new MpMenuItemViewModel() {
                        Header = processName,
                        IconId = avm == null ? 0 : avm.IconId,
                        SubItems = new List<MpMenuItemViewModel>()
                    };
                    foreach (var handle in kvp.Value) {
                        var ptapvm = new MpAvPasteToAppPathViewModel(
                                this,
                                new MpPasteToAppPath() {
                                    AppPath = processPath,
                                    AppName = Mp.Services.ProcessWatcher.GetProcessApplicationName(handle),
                                    IconId = avm == null ? 0 : avm.IconId,
                                    IsAdmin = Mp.Services.ProcessWatcher.IsAdmin(handle)
                                },
                                handle);

                        if (!ptapvm.IsHidden) {
                            rpmivm.SubItems.Add(ptapvm.ContextMenuItemViewModel);
                        }

                    }
                    if (rpmivm.SubItems.Count > 0) {
                        pmivml.Add(rpmivm);
                    }
                }
                if (pmivml.Count > 0) {
                    pmivml.Add(new MpMenuItemViewModel() { IsSeparator = true });
                }
                foreach (var ptapvm in Items) {
                    pmivml.AddRange(Items.Select(x => x.ContextMenuItemViewModel));
                }

                pmivml.Add(new MpMenuItemViewModel() { IsSeparator = true });

                pmivml.Add(new MpMenuItemViewModel() {
                    Header = "Add Application",
                    IconResourceKey = Application.Current.Resources["AddIcon"] as string,
                    Command = MpAvSettingsViewModel.Instance.ShowSettingsWindowCommand,
                    CommandParameter = 1
                });
                var rmivm = new MpMenuItemViewModel() {
                    Header = "Paste To Path",
                    IconResourceKey = Application.Current.Resources["PasteIcon"] as string,
                    SubItems = pmivml
                };
                return rmivm;
            }
        }

        private MpAvPasteToAppPathViewModel _selectedPasteToAppPathViewModel;
        public MpAvPasteToAppPathViewModel SelectedPasteToAppPathViewModel {
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

        #endregion

        public ObservableCollection<IntPtr> HiddenHandles { get; set; } = new ObservableCollection<IntPtr>();

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

        private static MpAvPasteToAppPathViewModelCollection _instance;
        public static MpAvPasteToAppPathViewModelCollection Instance => _instance ?? (_instance = new MpAvPasteToAppPathViewModelCollection());


        public async Task InitAsync() {
            //MpProcessManager.CurrentProcessWindowHandleStackDictionary += (s, e) => {
            //    switch (e.PropertyName) {
            //        case nameof(MpRunningApplicationManager.Instance.CurrentProcessWindowHandleStackDictionary):
            //            _menuItemViewModels = null;
            //            OnPropertyChanged(nameof(MenuItemViewModels));
            //            break;
            //    }
            //};

            var allPasteToAppPaths = await MpDataModelProvider.GetItemsAsync<MpPasteToAppPath>();
            foreach (var ptap in allPasteToAppPaths) {
                Items.Add(new MpAvPasteToAppPathViewModel(this, ptap));
            }
        }
        private MpAvPasteToAppPathViewModelCollection() : base() {

        }
        public void PasteToAppPathDataGrid_Loaded(object sender, RoutedEventArgs args) {
            var dg = (DataGrid)sender;
            dg.SelectionChanged += (s, e) => {
                foreach (var ptapvm in Items) {
                    ptapvm.IsSelected = ptapvm == SelectedPasteToAppPathViewModel ? true : false;
                }
            };
        }

        //public ContextMenu UpdatePasteToMenuItem(ContextMenu cm) {
        //    MenuItem ptamir = null;
        //    foreach (var mi in cm.Items) {
        //        if (mi == null || mi is Separator) {
        //            continue;
        //        }
        //        if ((mi as MenuItem).Name == "PasteToAppPathMenuItem") {
        //            ptamir = mi as MenuItem;
        //        }
        //    }
        //    if (ptamir == null) {
        //        return cm;
        //    }
        //    ICommand pasteCommand = null;
        //    //if (cm.DataContext is MpRtbListBoxItemRichTextBoxViewModel) {
        //    //    pasteCommand = (cm.DataContext as MpRtbListBoxItemRichTextBoxViewModel).RichTextBoxViewModelCollection.PasteSubSelectedClipsCommand;
        //    //} else {
        //    //    pasteCommand = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
        //    //}
        //    pasteCommand = MpClipTrayViewModel.Instance.PasteSelectedClipsCommand;
        //    ptamir.Items.Clear();
        //    bool addedSeperator = false;
        //    foreach (var ptamivmc in MpPasteToAppPathViewModelCollection.Instance.MenuItemViewModels) {
        //        if (ptamivmc.Count == 0) {
        //            continue;
        //        }
        //        if (ptamivmc[0].IsRuntime) {
        //            bool areAllHidden = true;
        //            foreach (var ptamivm in ptamivmc) {
        //                if (!ptamivm.IsHidden) {
        //                    areAllHidden = false;
        //                }
        //            }
        //            if (areAllHidden) {
        //                continue;
        //            }
        //            var ptamip = new MenuItem();
        //            ptamip.Header = MpProcessManager.GetProcessApplicationName(ptamivmc[0].Handle);
        //            ptamip.Icon = new Image() { Source = ptamivmc[0].IconId };
        //            foreach (var ptamivm in ptamivmc) {
        //                if (ptamivm.IsHidden) {
        //                    continue;
        //                }
        //                var ptami = new MenuItem();
        //                var l = new Label();
        //                l.Content = MpProcessManager.GetProcessMainWindowTitle(ptamivm.Handle) + (ptamivm.IsAdmin ? " (Admin)" : string.Empty);

        //                var eyeOpenImg = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(MpJsonPreferenceIO.Instance.AbsoluteResourcesPath + @"/Images/eye.png")) };
        //                var eyeClosedImg = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(MpJsonPreferenceIO.Instance.AbsoluteResourcesPath + @"/Images/eye_closed.png")) };
        //                var btn = new Button() { Cursor = Cursors.Hand, Content = eyeOpenImg, BorderThickness = new Thickness(0), Background = Brushes.Transparent, Width = 20, Height = 20, HorizontalAlignment = HorizontalAlignment.Right/*, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center*/ };
        //                bool isOverButton = false;
        //                MouseEventHandler mouseEnter = (object o, MouseEventArgs e) => {
        //                    btn.Content = eyeClosedImg;
        //                    isOverButton = true;
        //                };
        //                btn.MouseEnter += mouseEnter;

        //                MouseEventHandler mouseLeave = (object o, MouseEventArgs e) => {
        //                    btn.Content = eyeOpenImg;
        //                    isOverButton = false;
        //                };
        //                btn.MouseLeave += mouseLeave;

        //                RoutedEventHandler btnClick = (object o, RoutedEventArgs e) => {
        //                    ptamivm.IsHidden = true;
        //                    ptamip.Items.Remove(ptami);
        //                    if (ptamip.Items.Count == 0) {
        //                        ptamir.Items.Remove(ptamip);
        //                    }
        //                };
        //                btn.Click += btnClick;

        //                RoutedEventHandler btnUnload = null;

        //                btnUnload = (object o, RoutedEventArgs e) => {
        //                    btn.MouseEnter -= mouseEnter;
        //                    btn.MouseLeave -= mouseLeave;
        //                    btn.Click -= btnClick;
        //                    btn.Unloaded -= btnUnload;
        //                };

        //                btn.Unloaded += btnUnload;

        //                var sp = new StackPanel() { Orientation = Orientation.Horizontal };
        //                sp.Children.Add(l);
        //                sp.Children.Add(btn);

        //                ptami.Header = sp;
        //                ptami.Icon = new Image() { Source = ptamivm.IconId };
        //                //ptami.Command = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
        //                //ptami.CommandParameter = ptamivm.Handle;

        //                RoutedEventHandler ptamiClick = (object o, RoutedEventArgs e) => {
        //                    if (!isOverButton) {
        //                        pasteCommand.Execute(ptamivm.Handle);
        //                    }
        //                };
        //                ptami.Click += ptamiClick;

        //                RoutedEventHandler ptamiUnload = null;
        //                ptamiUnload = (object o, RoutedEventArgs e) => {
        //                    ptami.Click -= ptamiClick;
        //                    ptami.Unloaded -= ptamiUnload;
        //                };
        //                ptami.Unloaded += ptamiUnload;

        //                ptamip.Items.Add(ptami);
        //            }
        //            ptamir.Items.Add(ptamip);
        //        } else {
        //            if (!addedSeperator) {
        //                ptamir.Items.Add(new Separator());
        //                addedSeperator = true;
        //            }
        //            var ptaumi = new MenuItem();
        //            ptaumi.Header = ptamivmc[0].AppName;// + (ptamivmc[0].IsAdmin ? " (Admin)" : string.Empty) + (ptamivmc[0].IsSilent ? " (Silent)" : string.Empty);
        //            ptaumi.Icon = new Image() { Source = ptamivmc[0].IconId };
        //            ptaumi.Command = pasteCommand;
        //            ptaumi.CommandParameter = ptamivmc[0].PasteToAppPathId;

        //            ptamir.Items.Add(ptaumi);
        //        }
        //    }
        //    var addNewMenuItem = new MenuItem();
        //    addNewMenuItem.Header = "Add Application...";
        //    addNewMenuItem.Icon = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(MpJsonPreferenceIO.Instance.AbsoluteResourcesPath + @"/Icons/Silk/icons/add.png")) };
        //    RoutedEventHandler addNewMenuItemClick = (object o, RoutedEventArgs e) => {
        //        MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand.Execute(1);
        //    };
        //    addNewMenuItem.Click += addNewMenuItemClick;

        //    RoutedEventHandler addNewMenuItemUnload = null;
        //    addNewMenuItemUnload = (object o, RoutedEventArgs e) => {
        //        addNewMenuItem.Click -= addNewMenuItemClick;
        //        addNewMenuItem.Unloaded -= addNewMenuItemUnload;
        //    };
        //    ptamir.Items.Add(addNewMenuItem);

        //    return cm;
        //}

        //public new void Add(MpPasteToAppPathViewModel ptapvm) {
        //    base.Add(ptapvm);
        //}

        //public new void Remove(MpPasteToAppPathViewModel ptapvm) {
        //    if (this.Contains(ptapvm)) {
        //        base.Remove(ptapvm);
        //        ptapvm.Dispose();
        //    }
        //}

        public MpAvPasteToAppPathViewModel FindById(int ptapid) {

            return Items.Where(x => x.PasteToAppPathId == ptapid).First();
        }
        #endregion

        #region Private Methods
        private bool Validate() {
            ValidationText = string.Empty;
            foreach (var ptapvm in Items) {
                ValidationText += ptapvm.Validate();
            }
            return string.IsNullOrEmpty(ValidationText);
        }
        #endregion

        #region Commands    
        private MpCommand _deletePasteToAppPathCommand;
        public ICommand DeletePasteToAppPathCommand {
            get {
                if (_deletePasteToAppPathCommand == null) {
                    _deletePasteToAppPathCommand = new MpCommand(DeletePasteToAppPath);
                }
                return _deletePasteToAppPathCommand;
            }
        }
        private void DeletePasteToAppPath() {
            Items.Remove(SelectedPasteToAppPathViewModel);
        }

        public ICommand AddPasteToAppPathCommand => new MpCommand<object>(
            async (args) => {
                string appPath = string.Empty;
                if (args is MpApp) {
                    appPath = (args as MpApp).AppPath;
                    if (!File.Exists(appPath)) {
                        Console.WriteLine("AddPasteToAppPath error, appPath does not exist: " + appPath);
                        return;
                    }
                } else {
                    MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;

                    string openResult = await Mp.Services.NativePathDialog
                            .ShowFileDialogAsync($"Select application path", null, null, true);

                    MpAvMenuExtension.CloseMenu();

                    if (openResult.IsFile()) {
                        appPath = openResult;
                    }
                    MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
                }
                if (!appPath.IsFile()) {
                    return;
                }
                var app_pi = new MpPortableProcessInfo() { ProcessPath = appPath };
                var avm = await MpAvAppCollectionViewModel.Instance.GetOrCreateAppViewModelByProcessInfo(app_pi);
                if (avm == null) {
                    MpConsole.WriteTraceLine($"Error, couldn't create app vm for path '{appPath}'");
                    return;
                }
                var nptap = new MpPasteToAppPath() {
                    AppPath = appPath,
                    AppName = avm.AppName,
                    IconId = avm.IconId,
                    IsAdmin = false,
                    IsSilent = false,
                    Args = string.Empty
                };
                await nptap.WriteToDatabaseAsync();
                var nptapvm = new MpAvPasteToAppPathViewModel(this, nptap);
                Items.Add(nptapvm);

                SelectedPasteToAppPathViewModel = nptapvm;
                Validate();
            });

        public ICommand HideHandleCommand => new MpCommand<object>(
            (args) => {
                nint handle = (nint)args;

                if (!HiddenHandles.Contains(handle)) {
                    HiddenHandles.Add(handle);
                }
                Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsHidden)));
                OnPropertyChanged(nameof(ContextMenuItemViewModel));
            }, (args) => {
                return args is nint handle && handle != nint.Zero;
            });

        public ICommand ShowHandleCommand => new MpCommand<object>(
            (args) => {
                nint handle = (nint)args;
                if (HiddenHandles.Contains(handle)) {
                    HiddenHandles.Remove(handle);
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsHidden)));
                    OnPropertyChanged(nameof(ContextMenuItemViewModel));
                }
            }, (args) => {
                return args is nint handle && handle != nint.Zero;
            });

        public ICommand ShowAllHandlesCommand => new MpCommand(
            () => {
                HiddenHandles.Clear();
                Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsHidden)));
                OnPropertyChanged(nameof(ContextMenuItemViewModel));
            });

        #endregion
    }
}
