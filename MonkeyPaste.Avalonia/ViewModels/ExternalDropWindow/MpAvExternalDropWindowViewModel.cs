using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvExternalDropWindowViewModel :
        MpViewModelBase,
        MpIChildWindowViewModel,
        MpIWantsTopmostWindowViewModel,
        MpIHoverableViewModel {
        #region Private Variables

        private bool _wasHiddenOrCanceled = false;
        private Dictionary<int, bool> _preShowPresetState { get; set; } = new Dictionary<int, bool>();
        private DispatcherTimer _curDropTargetTimer { get; set; }
        private MpPoint _lastGlobalMousePoint { get; set; } // debouncer
        #endregion

        #region Statics
        private static MpAvExternalDropWindowViewModel _instance;
        public static MpAvExternalDropWindowViewModel Instance => _instance ?? (_instance = new MpAvExternalDropWindowViewModel());

        #endregion

        #region Interfaces

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; } = false;

        #endregion

        #region MpIWantsTopmostWindowViewModel Implementation
        bool MpIWantsTopmostWindowViewModel.WantsTopmost =>
            true;

        #endregion

        #region MpIChildWindowViewModel Implementation
        public MpWindowType WindowType =>
            MpWindowType.Modal;

        public bool IsOpen { get; set; }
        #endregion

        #endregion

        #region Properties

        #region View Models

        public MpAvAppViewModel DropAppViewModel { get; set; }
        #endregion

        #region State

        public bool IsDropWidgetEnabled { get; set; } = true;

        public bool HasUserToggledAnyHandlers { get; set; } = false;
        public double TotalRememberWaitTimeS =>
            10;
        public double RememberSecondsRemaining { get; private set; }

        public double RememberProgress =>
            (TotalRememberWaitTimeS - RememberSecondsRemaining) / TotalRememberWaitTimeS;

        public bool IsShowingDropWindow { get; private set; } = false;
        public bool IsShowingFinishMenu { get; private set; } = false;
        #endregion

        #region Model

        #endregion
        #endregion

        #region Constructors
        public MpAvExternalDropWindowViewModel() : base(null) {
            if (!IsDropWidgetEnabled) {
                return;
            }
            PropertyChanged += MpAvExternalDropWindowViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessega);
        }

        #endregion

        #region Public Methods
        public void Init() {
            // empty singleton init
        }
        #endregion

        #region Private Methods

        private void MpAvExternalDropWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
            //    case nameof(IsDrop)
            //}
        }
        private void ReceivedGlobalMessega(MpMessageType msg) {
            if (!IsDropWidgetEnabled) {
                return;
            }
            switch (msg) {
                case MpMessageType.ItemDragBegin:
                    ShowDropWindowCommand.Execute(null);
                    break;
                case MpMessageType.ItemDragCanceled:
                    CancelDropWidgetCommand.Execute(null);
                    break;
                case MpMessageType.ItemDragEnd:
                    MpConsole.WriteLine("DragEnd receieved");
                    if (_wasHiddenOrCanceled) {
                        Reset();
                        return;
                    }
                    // NOTE ItemDragEnd not sent if drag source is recycled (so listener checks for left mouse down)
                    ShowFinishDropMenuCommand.Execute(null);
                    break;
            }
        }


        private void SetDropWindowPosition(Window w) {
            MpPoint w_origin = MpPoint.Zero;
            var screen_bounds = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds;
            // TODO orient this base on mainwindow orientation
            switch (MpAvMainWindowViewModel.Instance.MainWindowOrientationType) {
                case MpMainWindowOrientationType.Bottom:
                    w_origin = screen_bounds.TopLeft + new MpPoint(10, 10);
                    break;
            }
            w.Position = w_origin.ToAvPixelPoint(MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling);
        }

        private Dictionary<int, bool> GetFormatPresetState() {
            return
              MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets
              .ToDictionary(kvp => kvp.PresetId, kvp => kvp.IsEnabled);

        }

        private async Task SaveAppPresetFormatStateAsync() {
            if (DropAppViewModel == null ||
                DropAppViewModel.IsThisApp) {
                return;
            }

            // TODO should use MpAppClipboardFormatInfo data for last active here
            var test = MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets.Select(x => new object[] { x.PresetId, x.IsEnabled });
            foreach (var preset_vm in MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets) {
                string param_info = preset_vm.GetPresetParamJson();
                await MpAppClipboardFormatInfo.CreateAsync(
                    appId: DropAppViewModel.AppId,
                    format: preset_vm.ClipboardFormat.clipboardName,
                    formatInfo: param_info,
                    ignoreFormat: !preset_vm.IsEnabled);
            }
        }
        private bool HasPresetsChanged() {
            foreach (var wp in MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets) {
                if (_preShowPresetState[wp.PresetId] != wp.IsEnabled) {
                    return true;
                }
            }
            return false;
        }
        private void RestoreFormatPresetState() {
            if (_preShowPresetState == null || _preShowPresetState.Count == 0) {
                return;
            }

            MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets
                .ForEach(x => x.IsEnabled = _preShowPresetState[x.PresetId]);
        }

        private void StartDropTargetListener() {
            if (_curDropTargetTimer == null) {
                _curDropTargetTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
                _curDropTargetTimer.Tick += _curDropTargetTimer_Tick;
            }
            _curDropTargetTimer.Start();
        }


        private void StopDropTargetListener() {
            if (_curDropTargetTimer == null) {
                return;
            }
            _curDropTargetTimer.Stop();
            _lastGlobalMousePoint = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;

        }

        private void _curDropTargetTimer_Tick(object sender, EventArgs e) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => _curDropTargetTimer_Tick(sender, e));
                return;
            }
            if (!MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                StopDropTargetListener();
            }
            var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            UpdateDropAppViewModelCommand.Execute(gmp);
        }

        private void OpenDropWindow() {
            var dw = new MpAvWindow() {
                Width = 300,
                MaxWidth = 300,
                MinHeight = 75,
                MaxHeight = 300,
                SizeToContent = SizeToContent.Manual,
                Background = Brushes.Transparent,
                TransparencyLevelHint = WindowTransparencyLevel.Transparent,
                WindowState = WindowState.Normal,
                SystemDecorations = SystemDecorations.None,
                ShowInTaskbar = false,
                Topmost = true,
                DataContext = this,
                Content = new MpAvExternalDropView()
            };
            dw.GetObservable(Window.IsVisibleProperty).Subscribe(value => SetDropWindowPosition(dw));

            dw.ShowChild();
            IsShowingDropWindow = true;
        }

        private void OpenDropCompleteWindow() {
            var dcw = new MpAvWindow() {
                Width = 300,
                MinHeight = 10,
                SizeToContent = SizeToContent.Height,
                WindowState = WindowState.Normal,
                SystemDecorations = SystemDecorations.Full,
                CanResize = false,
                ShowInTaskbar = false,
                Topmost = true,
                DataContext = this,
                Content = new MpAvDropCompleteView()
            };
            dcw.GetObservable(Window.IsVisibleProperty).Subscribe(value => SetDropWindowPosition(dcw));

            dcw.ShowChild();
            IsShowingFinishMenu = true;
        }

        private void Reset() {
            _wasHiddenOrCanceled = false;
            _lastGlobalMousePoint = null;

            IsShowingFinishMenu = false;
            IsShowingDropWindow = false;
            IsOpen = false;
            HasUserToggledAnyHandlers = false;

            _preShowPresetState.Clear();
            DropAppViewModel = null;

            StopDropTargetListener();
        }
        #endregion

        #region Commands
        public ICommand ShowDropWindowCommand => new MpCommand(
            () => {
                Reset();
                _preShowPresetState = GetFormatPresetState();
                StartDropTargetListener();

                OpenDropWindow();
            }, () => {
                return !IsShowingDropWindow;
            });

        public ICommand UpdateDropAppViewModelCommand => new MpCommand<object>(
            (drop_gmp_arg) => {
                var gmp = drop_gmp_arg as MpPoint;
                if (gmp == null) {
                    return;
                }

                // TODO? may need to account for multiple screens in process watcher

                var result = MpAvAppCollectionViewModel.Instance.GetAppViewModelFromScreenPoint(gmp, MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling);
                if (result != null &&
                    !result.IsThisApp) {
                    bool is_same_drop_target = DropAppViewModel != null && result.AppId == DropAppViewModel.AppId;
                    if (is_same_drop_target) {
                        return;
                    }
                    DropAppViewModel = result;
                    if (HasUserToggledAnyHandlers) {
                        // NOTE when user toggles format the change needs to stick and for rest
                        // of drag app overrides need to be ignored or their toggle maybe lost
                        return;
                    }

                    // only execute from here if any format list item was toggled or drop target has changed
                    // so presets match current widget state 
                    if (DropAppViewModel.ClipboardFormatInfos is MpAppClipboardFormatInfoCollectionViewModel cfic && !cfic.IsEmpty) {
                        foreach (var cfivm in cfic.Items) {
                            // get preset for app specified format
                            var wpvm =
                            MpAvClipboardHandlerCollectionViewModel.Instance
                                .AllAvailableWriterPresets
                                .FirstOrDefault(x => x.ClipboardFormat.clipboardName == cfivm.ClipboardFormat);
                            if (wpvm == null) {
                                continue;
                            }
                            wpvm.IsEnabled = !cfivm.IgnoreFormat;
                        }
                    } else if (!is_same_drop_target) {
                        // no overrides, reset to default
                        RestoreFormatPresetState();
                    }
                }
            }, (drop_gmp_arg) => {
                var gmp = drop_gmp_arg as MpPoint;
                if (drop_gmp_arg is not MpPoint) {
                    return false;
                }
                bool canUpdate = !IsShowingFinishMenu && IsDropWidgetEnabled;
                if (!canUpdate) {
                    return false;
                }
                if (_lastGlobalMousePoint != null &&
                            gmp.Distance(_lastGlobalMousePoint) < MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST) {
                    // debounce (window handle from point is expensive)
                    return false;
                }

                _lastGlobalMousePoint = gmp;
                return true;
            });

        public ICommand ShowFinishDropMenuCommand => new MpAsyncCommand<object>(
            async (drop_gmp_arg) => {
                StopDropTargetListener();

                IsOpen = false;
                IsShowingDropWindow = false;

                var test = Mp.Services.DropProcessWatcher.DropProcess;
                MpConsole.WriteLine($"Drop process: '{test}'");

                var test2 = Mp.Services.ProcessWatcher.LastProcessInfo;
                MpConsole.WriteLine($"Drop process2: '{test}'");

                if (DropAppViewModel == null ||
                    !HasPresetsChanged() ||
                    DropAppViewModel.IsThisApp) {
                    DoNotRememberDropInfoCommand.Execute(null);
                    return;
                }
                OpenDropCompleteWindow();

                await Task.Delay(300);
                RememberSecondsRemaining = TotalRememberWaitTimeS;
                var sw = Stopwatch.StartNew();
                while (true) {
                    RememberSecondsRemaining = TotalRememberWaitTimeS - sw.Elapsed.TotalSeconds;
                    if (!IsShowingFinishMenu) {
                        break;
                    }
                    if (RememberSecondsRemaining <= 0) {
                        RememberDropInfoCommand.Execute(null);
                        break;
                    }
                    while (IsHovering) {
                        if (!IsShowingDropWindow) {
                            break;
                        }
                        RememberSecondsRemaining = TotalRememberWaitTimeS;
                        await Task.Delay(100);
                    }
                    OnPropertyChanged(nameof(RememberProgress));
                    await Task.Delay(100);
                }

                RestoreFormatPresetState();
                Reset();
            }, (drop_gmp_arg) => {
                return !IsShowingFinishMenu && !_wasHiddenOrCanceled;
            });

        public ICommand RememberDropInfoCommand => new MpAsyncCommand(
            async () => {
                await SaveAppPresetFormatStateAsync();
                RestoreFormatPresetState();
                Reset();
            });

        public ICommand DoNotRememberDropInfoCommand => new MpCommand(
            () => {
                RestoreFormatPresetState();
                Reset();
            });

        public ICommand CancelDropWidgetCommand => new MpCommand(
            () => {
                _wasHiddenOrCanceled = true;
                MpConsole.WriteLine("Drop canceled");
                IsOpen = false;
            });
        #endregion
    }
}
