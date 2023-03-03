using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvExternalDropWindowViewModel : MpViewModelBase, MpIHoverableViewModel {
        #region Private Variables
        private Dictionary<int, bool> _preShowPresetState { get; set; } = new Dictionary<int, bool>();
        private DispatcherTimer _curDropTargetTimer { get; set; }
        private MpPoint _lastGlobalMousePoint { get; set; } // debouncer
        #endregion

        #region Statics
        private static MpAvExternalDropWindowViewModel _instance;
        public static MpAvExternalDropWindowViewModel Instance => _instance ?? (_instance = new MpAvExternalDropWindowViewModel());

        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; } = false;

        #endregion

        #region Properties

        #region View Models

        public MpAvAppViewModel DropAppViewModel { get; set; }
        #endregion

        #region State

        public bool IsDropWidgetEnabled { get; set; } = true;

        public bool HasUserToggledAnyHandlers { get; set; } = false;
        public int TotalRememberWaitTimeS => 30;
        public int RememberSecondsRemaining { get; private set; }

        public bool IsShowingDropWindow { get; private set; } = false;
        public bool IsShowingFinishMenu { get; private set; } = false;
        #endregion

        #region Model

        #endregion
        #endregion

        #region Constructors

        public MpAvExternalDropWindowViewModel() : base(null) {
            PropertyChanged += MpAvExternalDropWindowViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessega);
        }

        #endregion

        #region Public Methods
        public void Init() {
            //MpAvShortcutCollectionViewModel.Instance.OnGlobalDragBegin += Instance_OnGlobalDragBegin;
            //MpAvShortcutCollectionViewModel.Instance.OnGlobalDragEnd += Instance_OnGlobalDragEnd;
        }

        private void Instance_OnGlobalDragEnd(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void Instance_OnGlobalDragBegin(object sender, EventArgs e) {

        }
        #endregion

        #region Private Methods

        private void MpAvExternalDropWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsShowingDropWindow):
                    if (IsShowingDropWindow) {
                        MpAvExternalDropWindow.Instance.Show();
                        IsShowingFinishMenu = false;
                    } else {
                        RestoreFormatPresetState();
                        DropAppViewModel = null;
                        MpAvExternalDropWindow.Instance.Hide();
                    }
                    break;
            }
        }
        private void ReceivedGlobalMessega(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ItemDragBegin:
                    ShowDropWindowCommand.Execute(null);
                    break;
                case MpMessageType.ItemDragEnd:
                    // NOTE ItemDragEnd not sent if drag source is recycled (so listener checks for left mouse down)
                    ShowFinishDropMenuCommand.Execute(null);
                    break;
            }
        }
        private Dictionary<int, bool> GetFormatPresetState() {
            return
              MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets
              .ToDictionary(kvp => kvp.PresetId, kvp => kvp.IsEnabled);

        }

        private void ApplyAppPresetFormatState() {

            // TODO should use MpAppClipboardFormatInfo data for last active here
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
        #endregion

        #region Commands
        public ICommand ShowDropWindowCommand => new MpCommand(
            () => {
                StartDropTargetListener();
                IsShowingDropWindow = true;
                IsShowingFinishMenu = false;
                HasUserToggledAnyHandlers = false;
                _preShowPresetState = GetFormatPresetState();
                ApplyAppPresetFormatState();
            }, () => {
                return !IsShowingDropWindow && IsDropWidgetEnabled;
            });

        public ICommand UpdateDropAppViewModelCommand => new MpCommand<object>(
            (drop_gmp_arg) => {
                var gmp = drop_gmp_arg as MpPoint;
                if (gmp == null) {
                    return;
                }

                // TODO need to account for other screen here and use density for screen where gmp is contained
                var result = MpAvAppCollectionViewModel.Instance.GetAppViewModelFromScreenPoint(gmp, MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling);
                if (result != null) {
                    if (HasUserToggledAnyHandlers || (DropAppViewModel != null && result.AppId == DropAppViewModel.AppId)) {
                        return;
                    }
                    DropAppViewModel = result;
                    Dispatcher.UIThread.Post(async () => {
                        var acil = await MpDataModelProvider.GetAppClipboardFormatInfosByAppIdAsync(DropAppViewModel.AppId);

                        // TODO eventually should also apply param settings (in which case store in state info in show window)

                        //
                        for (int i = 0; i < acil.Count; i++) {
                            var aci = acil[i];
                            var wpvm = MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets.FirstOrDefault(x => x.ClipboardFormat.clipboardName == aci.FormatType);
                            if (wpvm == null) {
                                continue;
                            }
                            wpvm.IsEnabled = !aci.IgnoreFormat;
                        }
                        //MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets
                        //.Where(x => acil.Any(y => y.FormatType == x.ClipboardFormat.clipboardName))
                        //.ForEach(x => x.IsEnabled = !acil.FirstOrDefault(y => y.FormatType == x.ClipboardFormat.clipboardName).IgnoreFormat);

                        //Dispatcher.UIThread.Post(MpAvClipboardHandlerCollectionViewModel.Instance.OnPropertyChanged(nameof(MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets)));
                    });
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

                if (DropAppViewModel == null || !HasPresetsChanged()) {
                    DoNotRememberDropInfoCommand.Execute(null);
                    return;
                }
                IsShowingFinishMenu = true;

                RememberSecondsRemaining = TotalRememberWaitTimeS;
                var sw = Stopwatch.StartNew();
                while (true) {
                    RememberSecondsRemaining = TotalRememberWaitTimeS - (int)Math.Floor(sw.Elapsed.TotalSeconds);
                    if (!IsShowingDropWindow) {
                        break;
                    }
                    if (RememberSecondsRemaining < 0) {
                        break;
                    }
                    while (IsHovering) {
                        if (!IsShowingDropWindow) {
                            break;
                        }
                        RememberSecondsRemaining = TotalRememberWaitTimeS;
                        await Task.Delay(100);
                    }
                    await Task.Delay(100);
                }
                IsShowingFinishMenu = false;
                IsShowingDropWindow = false;
            }, (drop_gmp_arg) => {
                return !IsShowingFinishMenu && IsDropWidgetEnabled;
            });

        public ICommand RememberDropInfoCommand => new MpAsyncCommand(
            async () => {

                foreach (var preset_vm in MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets) {
                    string param_info = preset_vm.GetPresetParamJson();
                    await MpAppClipboardFormatInfo.CreateAsync(
                        appId: DropAppViewModel.AppId,
                        format: preset_vm.ClipboardFormat.clipboardName,
                        formatInfo: param_info,
                        ignoreFormat: !preset_vm.IsEnabled);
                }

                IsShowingDropWindow = false;
            }, () => {
                return DropAppViewModel != null && IsDropWidgetEnabled;
            });

        public ICommand DoNotRememberDropInfoCommand => new MpCommand(
            () => {
                if (_preShowPresetState != null) {
                    _preShowPresetState.Clear();
                }
                DropAppViewModel = null;
                HasUserToggledAnyHandlers = false;
                IsShowingFinishMenu = false;
                IsShowingDropWindow = false;
            });
        #endregion
    }
}
