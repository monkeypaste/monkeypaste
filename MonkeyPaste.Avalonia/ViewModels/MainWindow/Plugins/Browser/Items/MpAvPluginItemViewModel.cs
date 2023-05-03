using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Org.BouncyCastle.Crypto.Engines;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPluginItemViewModel :
        MpViewModelBase<MpAvPluginBrowserViewModel>,
        //MpISelectableViewModel,
        MpIHoverableViewModel {
        #region Private Variables
        #endregion

        #region Constants
        public const string DEFAULT_LICENSE_URL = @"https://www.monkeypaste.com/license";
        public const string ABUSE_BASE_URL = @"https://www.monkeypaste.com/plugins/abuse?id=";
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpISelectableViewModel Implementation
        public bool IsSelected =>

            Parent == null ? false : Parent.SelectedItem == this;
        public DateTime LastSelectedDateTime { get; set; }
        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }
        #endregion

        #endregion

        #region Properties

        #region View Models
        public MpAvPluginDependencyViewModel RootDependencyViewModel { get; private set; }
        #endregion

        #region Appearance
        public string IconBase64 { get; set; }

        public string ReadMeMarkDownText { get; set; } = string.Empty;

        public string ToggleInstallText =>
            IsInstalled ? "Uninstall" : "Install";
        #endregion

        #region State

        public bool IsAnyBusy =>
            IsBusy ||
            RootDependencyViewModel
            .SelfAndAllDescendants()
            .Cast<MpIAsyncObject>()
            .Any(x => x.IsBusy);

        public MpPluginFormat LoadedPluginRef =>
            MpPluginLoader.Plugins.Any(x => x.Value.guid == PluginGuid) ?
                MpPluginLoader.Plugins.FirstOrDefault(x => x.Value.guid == PluginGuid).Value :
                null;
        public bool IsInstalled =>
            LoadedPluginRef != null;

        public bool CanUpdate {
            get {
                if (!IsInstalled ||
                    PluginVersion == LoadedPluginRef.version) {
                    return false;
                }
                var temp = new[] { PluginVersion, LoadedPluginRef.version }.OrderDescending();
                return temp.FirstOrDefault() == PluginVersion;
            }
        }


        #endregion


        #region Model
        public string PluginReadMeUri {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.readmeUrl;
            }
        }

        public string PluginIconUri {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.iconUri;
            }
        }
        public string PluginTitle {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.title;
            }
        }
        public string PluginDescription {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.description;
            }
        }

        public string PackageUrl {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.packageUrl;
            }
        }

        #region Details
        public string PluginVersion {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.version;
            }
        }
        public string PluginCredits {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.credits;
            }
        }

        public string PluginLicenseUrl {
            get {
                if (PluginFormat == null ||
                    !Uri.IsWellFormedUriString(PluginFormat.licenseUrl, UriKind.Absolute)) {
                    return DEFAULT_LICENSE_URL;
                }
                return PluginFormat.credits;
            }
        }

        public string PluginAbuseUrl =>
            $"{ABUSE_BASE_URL}{PluginGuid}";

        public DateTime PluginPublishedDateTime {
            get {
                if (PluginFormat == null ||
                   !PluginFormat.datePublished.HasValue) {
                    return DateTime.MinValue;
                }
                return PluginFormat.datePublished.Value;
            }
        }

        public string PluginProjectUrl {
            get {
                if (PluginFormat == null ||
                    !Uri.IsWellFormedUriString(PluginFormat.projectUrl, UriKind.Absolute)) {
                    return string.Empty;
                }
                return PluginFormat.projectUrl;
            }
        }
        public string PluginTags {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.tags;
            }
        }
        #endregion

        public string PluginRootDirectory {
            get {
                if (PluginFormat is MpPluginFormat pf) {
                    return pf.RootDirectory;
                }
                return string.Empty;

            }
        }
        public string PluginGuid {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.guid;

            }
        }
        public MpManifestFormat PluginFormat { get; set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvPluginItemViewModel() : this(null) { }
        public MpAvPluginItemViewModel(MpAvPluginBrowserViewModel parent) : base(parent) {
            PropertyChanged += MpAvPluginItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpManifestFormat pf) {
            IsBusy = true;
            PluginFormat = pf;

            if (string.IsNullOrEmpty(PluginIconUri)) {
                IconBase64 = MpBase64Images.QuestionMark;
            } else {
                var icon_bytes = await MpFileIo.ReadBytesFromUriAsync(PluginIconUri, PluginRootDirectory);
                if (icon_bytes == null || icon_bytes.Length == 0) {
                    IconBase64 = MpBase64Images.QuestionMark;
                } else {
                    IconBase64 = icon_bytes.ToBase64String();
                }
            }
            OnPropertyChanged(nameof(IsInstalled));

            await CreateRootDependencyViewModelAsync();

            IsBusy = false;
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvPluginItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (!IsSelected) {
                        break;
                    }
                    LoadReadMeAsync().FireAndForgetSafeAsync(this);
                    break;
                case nameof(IsAnyBusy):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    break;
            }
        }

        private async Task LoadReadMeAsync() {
            if (!string.IsNullOrEmpty(ReadMeMarkDownText)) {
                // already loaded
                return;
            }
            if (string.IsNullOrEmpty(PluginReadMeUri)) {
                // no readme
                return;
            }
            bool was_busy = IsBusy;
            IsBusy = true;

            // temp test delay
            await Task.Delay(1000);

            var read_me_bytes = await MpFileIo.ReadBytesFromUriAsync(PluginReadMeUri, PluginRootDirectory);
            ReadMeMarkDownText = read_me_bytes.ToDecodedString();

            OnPropertyChanged(nameof(ToggleInstallText));
            IsBusy = was_busy;
        }
        private async Task CreateRootDependencyViewModelAsync() {
            RootDependencyViewModel = new MpAvPluginDependencyViewModel(this) {
                LabelText = "Dependencies"
            };
            if (PluginFormat == null ||
                PluginFormat.dependencies == null ||
                PluginFormat.dependencies.Count == 0) {
                return;
            }

            for (int i = 0; i < Enum.GetNames(typeof(MpPluginDependencyType)).Length; i++) {
                MpPluginDependencyType pdt = (MpPluginDependencyType)i;
                if (pdt == MpPluginDependencyType.None) {
                    continue;
                }
                var pdt_deps = PluginFormat.dependencies.Where(x => x.type == pdt);
                if (!pdt_deps.Any()) {
                    continue;
                }
                var pdt_vm = new MpAvPluginDependencyViewModel(this) {
                    LabelText = pdt.EnumToLabel()
                };
                foreach (var pdt_dep in pdt_deps) {
                    var pdt_dep_vm = new MpAvPluginDependencyViewModel(this);
                    await pdt_dep_vm.InitializeAsync(pdt_dep);
                    pdt_vm.Items.Add(pdt_dep_vm);
                }
                RootDependencyViewModel.Items.Add(pdt_vm);
            }
        }

        #endregion

        #region Commands
        public ICommand InstallPluginCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;

                await MpAvAnalyticItemCollectionViewModel.Instance
                    .InstallAnalyzerCommand.ExecuteAsync(PackageUrl);

                IsBusy = false;
                OnPropertyChanged(nameof(LoadedPluginRef));
                OnPropertyChanged(nameof(IsInstalled));

            }, () => {
                return !IsInstalled;
            });

        public ICommand UninstallPluginCommand => new MpAsyncCommand(
            async () => {
                var confirm = await Mp.Services.NativeMessageBox.ShowOkCancelMessageBoxAsync(
                    title: "Confirm",
                    message: $"Are you sure you want to remove '{PluginTitle}' and all its presets and shortcuts?",
                    owner: MpAvWindowManager.AllWindows.FirstOrDefault(x => x.DataContext == Parent),
                    iconResourceObj: "QuestionMarkImage");
                if (!confirm) {
                    // cancel
                    return;
                }

                IsBusy = true;
                await MpAvAnalyticItemCollectionViewModel.Instance
                    .UninstallAnalyzerCommand.ExecuteAsync(PluginGuid);

                // refresh list to remove this plugin
                Parent.PerformFilterCommand.Execute(null);

                IsBusy = false;
            }, () => {
                return IsInstalled;
            });

        public ICommand UpdatePluginCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;

                await MpAvAnalyticItemCollectionViewModel.Instance
                        .UpdatePluginCommand.ExecuteAsync(new object[] { PluginGuid, PackageUrl });


                IsBusy = false;
                OnPropertyChanged(nameof(CanUpdate));
            }, () => {
                return CanUpdate;
            });

        public ICommand ToggleIsPluginInstalledCommand => new MpCommand(
            () => {
                if (IsInstalled) {
                    UninstallPluginCommand.Execute(null);
                } else {
                    InstallPluginCommand.Execute(null);
                }
            });
        #endregion

    }
}
