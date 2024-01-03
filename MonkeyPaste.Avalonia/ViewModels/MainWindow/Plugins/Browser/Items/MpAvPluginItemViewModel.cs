using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPluginItemViewModel :
        MpAvViewModelBase<MpAvPluginBrowserViewModel>,
        //MpISelectableViewModel,
        MpIFilterMatch,
        MpIFilterMatchScore,
        MpIHoverableViewModel {
        #region Private Variables
        #endregion

        #region Constants

        public const string ABUSE_BASE_URL =
#if LOCAL_SERVER
            @"https://localhost/plugins/abuse?id=";
#else
            @"https://www.monkeypaste.com/plugins/abuse?id=";
#endif

        #endregion

        #region Statics
        #endregion

        #region Interfaces


        #region MpIFilterMatch Implementation
        private string[] _filterFields => new string[] {
            PluginTitle,
            PluginDescription,
            PluginAuthor,
            PluginTags,
            PluginProjectUrl
        };

        bool MpIFilterMatch.IsFilterMatch(string filter) {
            if (string.IsNullOrEmpty(filter)) {
                return true;
            }
            string lc_filter = filter.ToLower();
            return
                _filterFields
                .Where(x => x != null)
                .Any(x => x.ToLower().Contains(lc_filter));
        }
        int MpIFilterMatchScore.MatchScore(string filter) {
            if (string.IsNullOrEmpty(filter)) {
                return 1;
            }
            string lc_filter = filter.ToLower();
            return
                _filterFields
                .Where(x => x.ToLower().Contains(lc_filter))
                .Min(x => x.ComputeLevenshteinDistance(filter));
            //.Sum(x => x.ToLower().IndexListOfAll(lc_filter).Count);
        }
        #endregion

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
        public MpAvPluginDependencyViewModel RootDependencyViewModel =>
            RootDependencyCollection.FirstOrDefault();

        private ObservableCollection<MpAvPluginDependencyViewModel> _rootDependencyCollection;
        public ObservableCollection<MpAvPluginDependencyViewModel> RootDependencyCollection {
            get {
                if (_rootDependencyCollection == null) {
                    _rootDependencyCollection = new ObservableCollection<MpAvPluginDependencyViewModel>() {
                        new MpAvPluginDependencyViewModel(this) {
                            //LabelText = UiStrings.PluginBrowserDependenciesLabel
                        }
                    };
                }
                return _rootDependencyCollection;
            }
        }
        #endregion

        #region Appearance
        public string IconBase64 { get; set; }

        public string ReadMeMarkDownText { get; set; } = string.Empty;

        #endregion

        #region State
        public bool IsReadmeExpanded { get; set; }
        public bool IsReadmeLoading { get; set; }
        public bool IsUpdatePending {
            get {
                if (InstalledFormat is not MpPluginWrapper pw) {
                    return false;
                }
                return pw.UpdateDir.IsDirectory();
            }
        }

        public bool IsVisible {
            get {
                if (Parent == null ||
                    !(this as MpIFilterMatch).IsFilterMatch(Parent.FilterText)) {
                    return false;
                }
                switch (Parent.SelectedTabType) {
                    case MpPluginBrowserTabType.Browse:
                        return HasRemotes;
                    case MpPluginBrowserTabType.Installed:
                        return HasInstallation;
                    case MpPluginBrowserTabType.Updates:
                        return CanUpdate;
                    default:
                        return false;
                }
            }
        }
        public bool IsAnyBusy =>
            IsBusy ||
            RootDependencyViewModel
            .SelfAndAllDescendants()
            .Cast<MpIAsyncObject>()
            .Any(x => x.IsBusy);

        public bool HasInstallation =>
            InstalledFormat != null;
        public bool HasRemotes =>
            RemoteFormats.Any();
        public bool CanUninstall =>
            Parent != null &&
            Parent.SelectedTabType == MpPluginBrowserTabType.Installed &&
            PluginFormat != null &&
            !IsUpdatePending &&
            !MpPluginLoader.CorePluginGuids.Contains(PluginGuid);

        public bool CanUpdate {
            get {
                if (InstalledFormat == null ||
                   SortedRemoteFormats.FirstOrDefault() is not { } max_remote) {
                    return false;
                }
                return InstalledFormat.version.ToVersion() < max_remote.version.ToVersion();
            }
        }


        // NOTE SelectedRemoteFormatIdx is a placeholder if multiple versions get supported
        public int SelectedRemoteFormatIdx { get; set; } = 0;

        #region Format props


        private MpManifestFormat _pluginFormat;
        public MpManifestFormat PluginFormat {
            get {
                MpManifestFormat result = null;
                if (Parent == null) {
                    result = AllFormats.FirstOrDefault();
                } else {
                    switch (Parent.SelectedTabType) {
                        case MpPluginBrowserTabType.Installed:
                            result = InstalledFormat;
                            break;
                        case MpPluginBrowserTabType.Browse:
                            result = SelectedRemoteFormat;
                            break;
                        case MpPluginBrowserTabType.Updates:
                            if (InstalledFormat != null &&
                                SelectedRemoteFormat != null &&
                                InstalledFormat.version.ToVersion() != SelectedRemoteFormat.version.ToVersion()) {
                                result = SelectedRemoteFormat;
                            } else {
                                result = null;
                            }
                            break;

                    }
                }
                bool changed = result != _pluginFormat;
                _pluginFormat = result;
                if (changed) {
                    OnPropertyChanged(nameof(PluginFormat));
                }
                return _pluginFormat;
            }
        }

        IEnumerable<MpManifestFormat> AllFormats {
            get {
                if (Parent == null) {
                    yield break;
                }
                foreach (var mf in Parent.AllManifests) {
                    if (mf.guid == PluginGuid) {
                        yield return mf;
                    }
                }
            }
        }
        IList<MpManifestFormat> RemoteFormats =>
            AllFormats
            .Where(x => x is not MpPluginFormat)
            .ToList();
        MpPluginFormat InstalledFormat =>
            AllFormats
            .OfType<MpPluginFormat>()
            .FirstOrDefault();

        MpManifestFormat[] SortedRemoteFormats =>
            RemoteFormats
            .OrderByDescending(x => x.version.ToVersion())
            .ToArray();

        MpManifestFormat SelectedRemoteFormat {
            get {
                if (SelectedRemoteFormatIdx >= RemoteFormats.Count) {
                    return null;
                }
                return SortedRemoteFormats[SelectedRemoteFormatIdx];
            }
        }


        #endregion

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

        public string PluginDonateUri {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.donateUrl;
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
        public string PluginVersionText {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.version;
            }
        }
        public string PluginAuthor {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.author;
            }
        }

        public string PluginLicenseUrl {
            get {
                if (PluginFormat == null ||
                    !Uri.IsWellFormedUriString(PluginFormat.licenseUrl, UriKind.Absolute)) {
                    return null;
                }
                return PluginFormat.licenseUrl;
            }
        }

        public string PluginAbuseUrl =>
            $"{ABUSE_BASE_URL}{PluginGuid}";

        public DateTime? PluginPublishedDateTime {
            get {
                if (PluginFormat == null ||
                   !PluginFormat.datePublished.HasValue) {
                    return DateTime.Now;
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
                return string.Join(", ", PluginFormat.tags.ToStringOrEmpty().Split(",").Select(x => x.Trim()));
            }
        }
        #endregion

        public string PluginManifestDirectory {
            get {
                if (PluginFormat is MpPluginWrapper pf) {
                    return pf.ManifestDir;
                }
                return string.Empty;

            }
        }

        public string PluginGuid { get; private set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvPluginItemViewModel() : this(null) { }
        public MpAvPluginItemViewModel(MpAvPluginBrowserViewModel parent) : base(parent) {
            PropertyChanged += MpAvPluginItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(string plugin_guid) {
            IsBusy = true;
            PluginGuid = plugin_guid;

            if (string.IsNullOrEmpty(PluginIconUri)) {
                IconBase64 = MpBase64Images.JigsawPiece;
            } else {
                var icon_bytes = await MpFileIo.ReadBytesFromUriAsync(PluginIconUri, PluginManifestDirectory);
                if (icon_bytes == null || icon_bytes.Length == 0) {
                    IconBase64 = MpBase64Images.JigsawPiece;
                } else {
                    IconBase64 = icon_bytes.ToBase64String();
                }
            }
            RefreshState();

            await CreateRootDependencyViewModelAsync();
            IsBusy = false;
        }
        public void RefreshState() {
            OnPropertyChanged(nameof(PluginFormat));
            OnPropertyChanged(nameof(HasInstallation));
            OnPropertyChanged(nameof(CanUninstall));
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(IsUpdatePending));
        }

        public override string ToString() {
            return $"{PluginFormat} {(HasInstallation ? "LOCAL" : "REMOTE")}";
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
                    RefreshState();
                    OnPropertyChanged(nameof(RootDependencyCollection));
                    break;
                case nameof(IsAnyBusy):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    break;
                case nameof(IsReadmeExpanded):
                    if (IsReadmeExpanded) {
                        LoadReadMeAsync().FireAndForgetSafeAsync();
                    }
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
            IsReadmeLoading = true;

            var read_me_bytes = await MpFileIo.ReadBytesFromUriAsync(PluginReadMeUri, PluginManifestDirectory);
            ReadMeMarkDownText = read_me_bytes.ToDecodedString();
            IsReadmeLoading = false;
        }
        private async Task CreateRootDependencyViewModelAsync() {
            RootDependencyViewModel.Items.Clear();
            if (PluginFormat == null ||
                PluginFormat.dependencies == null ||
                PluginFormat.dependencies.Count == 0) {
                RootDependencyViewModel.Items.Add(new MpAvPluginDependencyViewModel(this) {
                    LabelText = UiStrings.CommonNoneLabel
                });
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
                    LabelText = pdt.EnumToUiString(),
                    ParentTreeItem = RootDependencyViewModel
                };
                foreach (var pdt_dep in pdt_deps) {
                    var pdt_dep_vm = new MpAvPluginDependencyViewModel(this) {
                        ParentTreeItem = pdt_vm
                    };
                    await pdt_dep_vm.InitializeAsync(pdt_dep);
                    pdt_vm.Items.Add(pdt_dep_vm);
                }
                RootDependencyViewModel.Items.Add(pdt_vm);
            }

            OnPropertyChanged(nameof(RootDependencyViewModel));
            OnPropertyChanged(nameof(RootDependencyCollection));
        }

        #endregion

        #region Commands
        public ICommand InstallPluginCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;
                if (PluginFormat.GetComponentManager() is { } cm) {
                    await cm.InstallAsync(PluginGuid, PackageUrl);
                }

                Parent.PerformFilterCommand.Execute("refresh");
                IsBusy = false;

            }, () => {
                return !HasInstallation;
            });

        public ICommand UninstallPluginCommand => new MpAsyncCommand(
            async () => {
                var confirm = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                    title: UiStrings.CommonConfirmLabel,
                    message: string.Format(UiStrings.PluginBrowserNtfUninstallMsg, PluginTitle),
                    iconResourceObj: "QuestionMarkImage");
                if (!confirm) {
                    // cancel
                    return;
                }

                IsBusy = true;
                if (PluginFormat.GetComponentManager() is { } cm) {
                    await cm.UninstallAsync(PluginGuid);
                }

                // refresh list to remove this plugin
                Parent.PerformFilterCommand.Execute("refresh");
                IsBusy = false;
            }, () => {
                return HasInstallation && CanUninstall;
            });

        public ICommand UpdatePluginCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;
                if (PluginFormat.GetComponentManager() is { } cm) {
                    await cm.BeginUpdateAsync(PluginGuid, PackageUrl);
                }

                IsBusy = false;

                RefreshState();
            }, () => {
                return CanUpdate && !IsUpdatePending;
            });

        public ICommand ToggleIsPluginInstalledCommand => new MpCommand(
            () => {
                if (HasInstallation) {
                    UninstallPluginCommand.Execute(null);
                } else {
                    InstallPluginCommand.Execute(null);
                }
            }, () => {
                if (HasInstallation) {
                    return UninstallPluginCommand.CanExecute(null);
                } else {
                    return InstallPluginCommand.CanExecute(null);
                }
            });
        #endregion

    }
}
