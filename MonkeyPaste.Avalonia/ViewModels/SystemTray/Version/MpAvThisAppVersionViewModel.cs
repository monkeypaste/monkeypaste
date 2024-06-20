using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvThisAppVersionViewModel : MpAvViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        static string VERSION_CHECK_URL = $"{MpServerConstants.VERSION_BASE_URL}/version.php";
        static string CHANGE_LOG_BASE_URL = $"{MpServerConstants.DOCS_BASE_URL}/versions";


        private static MpAvThisAppVersionViewModel _instance;
        public static MpAvThisAppVersionViewModel Instance => _instance ?? (_instance = new MpAvThisAppVersionViewModel());

        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Appearance

        public string ThisAppVersionDisplayValue {
            get {
                string debug_prefix =
#if DEBUG
                    "[DEBUG] ";
#else
                    string.Empty;
#endif
                return $"{debug_prefix}{ThisAppVersion.ToString()}";
            }
        }

#endregion
        
        #region State

        public bool IsOutOfDate =>
            UpToDateAppVersion != null &&
            ThisAppVersion < UpToDateAppVersion;

        #endregion

        #region Model
        public string ChangeLogUrl =>
            MpAvDocusaurusHelpers.GetCustomUrl(
                url: $"{CHANGE_LOG_BASE_URL}/{Mp.Services.ThisAppInfo.ThisAppProductVersion}",
                hideNav: true,
                hideSidebars: true,
                isDark: MpAvPrefViewModel.Instance.IsThemeDark);

        public Version LastLoadedVersion {
            get => MpAvPrefViewModel.Instance.LastLoadedVersion.ToVersion();
            set {
                if (LastLoadedVersion.CompareTo(value) != 0) {
                    MpAvPrefViewModel.Instance.LastLoadedVersion = value.ToString();
                    OnPropertyChanged(nameof(LastLoadedVersion));
                }
            }
        }
        public Version ThisAppVersion =>
            Mp.Services.ThisAppInfo.ThisAppProductVersion.ToVersion();

        public Version UpToDateAppVersion { get; private set; }

        public Version LastNotfiedVersion { get; set; }

        #endregion

#endregion

        #region Constructors
        public MpAvThisAppVersionViewModel() : base() {
        }

        #endregion

        #region Public Methods
        public void Init() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            PropertyChanged += MpAvThisAppVersionViewModel_PropertyChanged;
            StartUpdateCheckTimer();
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvThisAppVersionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(UpToDateAppVersion):
                case nameof(IsOutOfDate):
                    MpMessenger.SendGlobal(MpMessageType.VersionInfoChanged);
                    break;
            }
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowLoadComplete:
                    if (LastLoadedVersion < ThisAppVersion) {
                        LastLoadedVersion = ThisAppVersion;
                        ShowChangeLogWindowCommand.Execute(null);
                    }
                    break;
            }
        }
        private void StartUpdateCheckTimer() {
            var update_check_timer = new DispatcherTimer() {
                Interval = TimeSpan.FromMinutes(5)
            };
            void CheckForUpdate_tick(object sender, EventArgs e) {
                CheckForUpdateCommand.Execute("timer");
            }
            update_check_timer.Tick += CheckForUpdate_tick;
            update_check_timer.Start();
            // initial check
            CheckForUpdate_tick(update_check_timer, EventArgs.Empty);
        }
        #endregion

        #region Commands

        public ICommand ShowChangeLogWindowCommand => new MpCommand(
            () => {
                Mp.Services.PlatformMessageBox.ShowWebViewWindow(
                    window_title_prefix: string.Format(UiStrings.ChangeLogWindowTitle, ThisAppVersion.ToString()),
                    address: ChangeLogUrl,
                    iconResourceObj: "MegaPhoneImage");
            });
        public MpIAsyncCommand<object> CheckForUpdateCommand => new MpAsyncCommand<object>(
            async (args) => {
                string source = args.ToStringOrEmpty();
                bool from_user = source == "Click";

                CancellationTokenSource cts = null;
                if (from_user) {
                    cts = new CancellationTokenSource();
                    Mp.Services.PlatformMessageBox.ShowBusyMessageBoxAsync(
                        title: UiStrings.CommonBusyLabel,
                        iconResourceObj: "HourGlassImage",
                        cancel_token_arg: cts.Token).FireAndForgetSafeAsync();
                }
                var req_args = new Dictionary<string, string>() {
                    {"device_type",Mp.Services.PlatformInfo.OsType.ToString() }
                };
                // send device type and receive most recent version by device
                var resp = await MpHttpRequester.SubmitPostDataToUrlAsync(VERSION_CHECK_URL, req_args);
                bool success = MpHttpRequester.ProcessServerResponse(resp, out var resp_args);
                if (cts != null) {
                    cts.Cancel();
                }
                if (!success) {
                    // couldn't connect
                    if (from_user) {
                        Mp.Services.NotificationBuilder.ShowMessageAsync(
                            msgType: MpNotificationType.BadHttpRequest,
                            title: UiStrings.CommonConnectionFailedCaption,
                            body: UiStrings.CommonConnectionFailedText,
                            iconSourceObj: "NoEntryImage").FireAndForgetSafeAsync();
                    }
                    return;
                }
                UpToDateAppVersion = resp_args["device_version"].ToVersion();

                if (!IsOutOfDate) {
                    // this is most recent version
                    if (from_user) {
                        Mp.Services.NotificationBuilder.ShowMessageAsync(
                            msgType: MpNotificationType.NoUpdateAvailable,
                            title: UiStrings.NtfUpToDateTitle,
                            body: string.Format(UiStrings.NtfUpToDateText, ThisAppVersion.ToString()),
                            iconSourceObj: new object[] { MpSystemColors.forestgreen, "CheckRoundImage" }).FireAndForgetSafeAsync();
                    }
                    return;
                }
                // update available
                bool show_ntf = source == "Click";
                if (!show_ntf) {
                    if (LastNotfiedVersion == null ||
                        UpToDateAppVersion.CompareTo(LastNotfiedVersion) != 0) {
                        show_ntf = true;
                    }
                }
                if (!show_ntf) {
                    // non-user check, already notified
                    return;
                }

                LastNotfiedVersion = UpToDateAppVersion;
                var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.UpdateAvailable,
                            title: UiStrings.NtfUpdateAvailableTitle,
                            maxShowTimeMs: from_user ? -1 : 5_000,
                            body: string.Format(UiStrings.NtfUpdateAvailableText, UpToDateAppVersion.ToString()),
                            iconSourceObj: "MegaPhoneImage");
                if (result == MpNotificationDialogResultType.Ok) {
                    MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(MpAvAccountTools.Instance.ThisProductUri);
                }
            });
        #endregion


    }
}
