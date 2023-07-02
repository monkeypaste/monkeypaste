using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
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
        MpICloseWindowViewModel,
        MpIWantsTopmostWindowViewModel,
        MpIHoverableViewModel {
        #region Private Variables

        private MpAvWindow _dropWidgetWindow;
        private MpAvWindow _dropCompleteWindow;

        private bool _wasHiddenOrCanceled = false;
        private Dictionary<string, bool> _preShowPresetState { get; set; } = new Dictionary<string, bool>();
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

        public bool IsWindowOpen { get; set; }
        #endregion

        #endregion

        #region Properties

        #region View Models

        public MpAvAppViewModel DropAppViewModel { get; set; }

        #endregion

        #region State

        public bool CanEnableDropWidget =>
            Mp.Services.PlatformInfo.IsDesktop;
        public bool IsDropWidgetEnabled =>
            CanEnableDropWidget &&
            MpPrefViewModel.Instance.ShowExternalDropWidget;

        public bool HasUserToggledAnyHandlers { get; set; } = false;
        public double TotalRememberWaitTimeS =>
            10;
        public double RememberSecondsRemaining { get; private set; }

        public double RememberProgress =>
            (TotalRememberWaitTimeS - RememberSecondsRemaining) / TotalRememberWaitTimeS;

        public bool IsShowingDropWindow =>
            _dropWidgetWindow != null;
        public bool IsShowingFinishMenu =>
            _dropCompleteWindow != null;

        public bool IsDragObjectInitializing {
            get {
                var dobj = MpAvContentDragHelper.DragDataObject;
                if (dobj == null) {
                    return false;
                }
                return dobj.IsAnyPlaceholderData();
            }
        }

        public string DragInfo {
            get {
                if (MpAvContentDragHelper.DragDataObject == null) {
                    return "NO DRAG OBJECT";
                }
                return MpAvContentDragHelper.DragDataObject.ToString();
            }
        }
        #endregion

        #region Model

        public IDataObject DragDataObject =>
            MpAvContentDragHelper.DragDataObject;

        #endregion

        #endregion

        #region Constructors
        public MpAvExternalDropWindowViewModel() : base(null) {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessega);
            PropertyChanged += MpAvExternalDropWindowViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        public void Init() {
            // empty singleton init
        }
        #endregion

        #region Private Methods

        private void MpAvExternalDropWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsWindowOpen):
                    if (IsWindowOpen) {
                        break;
                    }
                    if (_dropWidgetWindow != null) {
                        _dropWidgetWindow = null;
                    } else if (_dropCompleteWindow != null) {
                        _dropCompleteWindow = null;
                    }

                    OnPropertyChanged(nameof(IsShowingFinishMenu));
                    OnPropertyChanged(nameof(IsShowingFinishMenu));
                    break;
                case nameof(IsHovering):

                    break;
            }
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
                case MpMessageType.DropWidgetEnabledChanged:

                    break;
            }
        }


        #region Preset State

        private Dictionary<string, bool> GetFormatPresetState() {
            return
              MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets
              .ToDictionary(kvp => kvp.ClipboardFormat.clipboardName, kvp => kvp.IsEnabled);

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
        private bool DidPresetsChange(MpAvAppViewModel avm) {
            if (avm == null || avm.IsThisApp) {
                return false;
            }

            var cur_preset_state = GetFormatPresetState();
            bool is_default = !_preShowPresetState.Difference(cur_preset_state).Any();

            if (avm.ClipboardFormatInfos.Items == null) {
                return !is_default;
            }

            if (!avm.ClipboardFormatInfos.Items.Any() && is_default) {
                // no custom app settings, default toggles
                return false;
            }
            // compare cur preset state to db of app
            var avm_preset_state = avm.ClipboardFormatInfos.Items
                .ToDictionary(x => x.ClipboardFormat, x => !x.IgnoreFormat);

            return avm_preset_state.Difference(cur_preset_state).Any();
        }
        private void RestoreFormatPresetState() {
            if (_preShowPresetState == null || _preShowPresetState.Count == 0) {
                return;
            }

            MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets
                .ForEach(x => x.IsEnabled = _preShowPresetState[x.ClipboardFormat.clipboardName]);
        }

        #endregion

        #region Drop Target Listener
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
            OnPropertyChanged(nameof(IsDragObjectInitializing));
            MpAvClipboardHandlerCollectionViewModel.Instance
                .AllAvailableWriterPresets
                .ForEach(x => x.OnPropertyChanged(nameof(x.IsFormatPlaceholderOnTargetDragObject)));

            var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            UpdateDropApp(gmp);
        }

        private void UpdateDropApp(MpPoint gmp) {
            if (IsShowingFinishMenu || gmp == null) {
                return;
            }
            if (_lastGlobalMousePoint != null &&
                        gmp.Distance(_lastGlobalMousePoint) < MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST) {
                // debounce (window handle from point is expensive)
                return;
            }

            _lastGlobalMousePoint = gmp;
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
        }

        #endregion

        #region Window Openers

        private void OpenDropWindow() {
            _dropWidgetWindow = new MpAvWindow() {
                Width = 300,
                MaxWidth = 300,
                MinHeight = 75,
                MaxHeight = 300,
                SizeToContent = SizeToContent.Manual,
                Background = Brushes.Transparent,
                TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
                WindowState = WindowState.Normal,
                SystemDecorations = SystemDecorations.None,
                ShowInTaskbar = false,
                Topmost = true,
                DataContext = this,
                Content = new MpAvExternalDropView()
            };
            _dropWidgetWindow.GetObservable(Window.IsVisibleProperty).Subscribe(value => SetDropWindowPosition(_dropWidgetWindow));

            _dropWidgetWindow.ShowChild();
            OnPropertyChanged(nameof(IsShowingDropWindow));
        }

        private void OpenDropCompleteWindow() {
            if (_wasHiddenOrCanceled) {
                return;
            }
            _dropCompleteWindow = new MpAvWindow() {
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
            _dropCompleteWindow.GetObservable(Window.IsVisibleProperty).Subscribe(value => SetDropWindowPosition(_dropCompleteWindow));

            _dropCompleteWindow.ShowChild();
            OnPropertyChanged(nameof(IsShowingFinishMenu));
        }

        private void SetDropWindowPosition(Window w) {
            if (w == null ||
                !w.IsVisible) {
                return;
            }
            if (w == _dropWidgetWindow) {
                MpMessenger.SendGlobal(MpMessageType.DropWidgetOpened);
            }
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
        #endregion

        private void Reset() {
            _wasHiddenOrCanceled = false;
            _lastGlobalMousePoint = null;

            IsWindowOpen = false;
            _dropWidgetWindow = null;
            _dropCompleteWindow = null;

            OnPropertyChanged(nameof(IsShowingFinishMenu));
            OnPropertyChanged(nameof(IsShowingFinishMenu));
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

        public ICommand ShowFinishDropMenuCommand => new MpAsyncCommand<object>(
            async (drop_gmp_arg) => {
                StopDropTargetListener();

                IsWindowOpen = false;

                bool show_finish_menu = DidPresetsChange(DropAppViewModel);
                if (!show_finish_menu) {
                    DoNotRememberDropInfoCommand.Execute(null);
                    return;
                }

                OpenDropCompleteWindow();

                await Task.Delay(300);
                RememberSecondsRemaining = TotalRememberWaitTimeS;
                var sw = Stopwatch.StartNew();
                while (true) {
                    RememberSecondsRemaining = TotalRememberWaitTimeS - sw.Elapsed.TotalSeconds;
                    while (IsHovering) {
                        if (!IsShowingFinishMenu) {
                            break;
                        }
                        RememberSecondsRemaining = TotalRememberWaitTimeS;
                        await Task.Delay(100);
                    }
                    if (!IsShowingFinishMenu) {
                        break;
                    }
                    if (RememberSecondsRemaining <= 0) {
                        RememberDropInfoCommand.Execute(null);
                        break;
                    }
                    OnPropertyChanged(nameof(RememberProgress));
                    await Task.Delay(100);
                }
            }, (drop_gmp_arg) => {
                return !IsShowingFinishMenu && !_wasHiddenOrCanceled;
            });

        public ICommand RememberDropInfoCommand => new MpAsyncCommand(
            async () => {
                // NOTE cmd for 'Yes' in DropCompleteView
                await SaveAppPresetFormatStateAsync();
                RestoreFormatPresetState();
                Reset();
            });

        public ICommand DoNotRememberDropInfoCommand => new MpCommand(
            () => {
                // NOTE cmd for 'No' in DropCompleteView
                RestoreFormatPresetState();
                Reset();
            });

        public ICommand CancelDropWidgetCommand => new MpCommand<object>(
            (args) => {
                if (args is Control c) {
                    if (ToolTip.GetTip(c) is ToolTip tt) {
                        tt.IsVisible = true;
                    }
                    // drag over
                    return;
                }
                _wasHiddenOrCanceled = true;
                MpConsole.WriteLine("Drop canceled");
                IsWindowOpen = false;
            });

        public ICommand ToggleIsDropWidgetEnabledCommand => new MpCommand(
            () => {
                MpPrefViewModel.Instance.ShowExternalDropWidget = !MpPrefViewModel.Instance.ShowExternalDropWidget;
                OnPropertyChanged(nameof(IsDropWidgetEnabled));
                MpNotificationBuilder.ShowMessageAsync(
                    title: "MODE CHANGED",
                    body: $"Drop Wizard: {(IsDropWidgetEnabled ? "ON" : "OFF")}",
                    msgType: MpNotificationType.AppModeChange).FireAndForgetSafeAsync(this);

                MpMessenger.SendGlobal(MpMessageType.DropWidgetEnabledChanged);
            }, () => {
                return CanEnableDropWidget;
            });
        #endregion
    }
}
