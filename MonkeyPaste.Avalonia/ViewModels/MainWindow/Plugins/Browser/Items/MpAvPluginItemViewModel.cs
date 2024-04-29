using Avalonia.Threading;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        static string ABUSE_BASE_URL =
            $"{MpServerConstants.PLUGINS_BASE_URL}/abuse?id=";

        #endregion

        #region Statics
        static DispatcherPriority DOWNLOAD_PRIORITY = DispatcherPriority.Default;
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
        public MpAvCommonCancelableProgressIndicatorViewModel InstallProgressViewModel { get; set; }
        public MpAvCommonCancelableProgressIndicatorViewModel UpdateProgressViewModel { get; set; }
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

        public IEnumerable<MpAvAnalyticItemPresetViewModel> InstalledPluginDefaultPresetViewModels {
            get {
                if(!HasInstallation) {
                    yield break;
                }
                if(MpAvAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x=>x.PluginGuid == PluginGuid) is { } aivm) {
                    // can really return any preset since only worried about shared params but to avoid being arbitrary try to use default preset
                    if(aivm.Items.FirstOrDefault(x=>x.IsSystemPreset) is { } aipvm) {
                        yield return aipvm;
                    } else {
                        yield return aivm.Items.FirstOrDefault();
                    }
                }
                
                //if(MpAvClipboardHandlerCollectionViewModel.Instance.Items.FirstOrDefault(x=>x.PluginGuid == PluginGuid) is { } chvm) {
                //    // can really return any preset since only worried about shared params but to avoid being arbitrary try to use default preset
                //    foreach(var cfvm in chvm.Items) {
                //        if(cfvm.Items.FirstOrDefault(x=>x.IsDefault) is { } def_chpvm) {
                //            yield return def_chpvm;
                //        }
                //        yield return cfvm.Items.FirstOrDefault();
                //    }
                //}
            }
        }
        #endregion

        #region Appearance
        private string _iconBase64;
        public string IconBase64 {
            get {
                if (_iconBase64 == null) {
                    return MpBase64Images.JigsawPiece;
                }
                return _iconBase64;
            }
            set {
                if (_iconBase64 != value) {
                    _iconBase64 = value;
                    OnPropertyChanged(nameof(IconBase64));
                }
            }
        }

        public string ReadMeHtml { get; set; } = string.Empty;

        public string InstallButtonText {
            get {
                if (IsUninstallPending || IsUpdatePending) {
                    return UiStrings.PluginPendingUninstallButtonText;
                }
                if (CanInstall) {
                    return UiStrings.PluginBrowserInstallLabel;
                }
                return UiStrings.PluginBrowserUninstallLabel;
            }
        }

        public string UpdateButtonText {
            get {
                if (IsUpdatePending) {
                    return UiStrings.PluginPendingUpdateButtonText;
                }
                return UiStrings.PluginBrowserUpdateLabel;
            }
        }
        public string DisabledInstallTooltip {
            get {
                if (IsUninstallPending) {
                    return UiStrings.PluginUninstallPendingTooltip;
                }
                if (IsUpdatePending) {
                    return UiStrings.PluginPendingUpdateBtnTooltip;
                }
                if (IsCorePlugin) {
                    return UiStrings.PluginBrowserCoreInstallBtnTooltip;
                }
                return string.Empty;
            }
        }
        public string DisabledUpdateTooltip {
            get {
                if (IsUpdatePending) {
                    return UiStrings.PluginPendingUpdateBtnTooltip;
                }
                return string.Empty;
            }
        }
        #endregion

        #region State
        public bool IsDownloading =>
            InstallProgressViewModel != null;
        public bool ShowDisabledInstallTooltip =>
            IsUninstallPending || IsUpdatePending || IsCorePlugin;
        public bool ShowDisabledUpdateTooltip =>
            IsUpdatePending;
        public bool IsReadmeExpanded { get; set; }
        public bool IsReadmeLoading { get; set; }
        public bool IsUpdatePending {
            get {
                if (InstalledFormat is not MpRuntimePlugin pw) {
                    return false;
                }
                return pw.UpdateDir.IsDirectory();
            }
        }
        public bool IsCorePlugin =>
            MpPluginLoader.CorePluginGuids.Contains(PluginGuid);
        public bool IsUninstallPending =>
            MpPluginLoader.UninstalledPluginGuids.Contains(PluginGuid);
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
        public bool CanInstall =>
            Parent != null &&
            Parent.SelectedTabType == MpPluginBrowserTabType.Browse &&
            PluginFormat != null &&
            !IsUninstallPending;

        public bool CanUninstall =>
            Parent != null &&
            Parent.SelectedTabType == MpPluginBrowserTabType.Installed &&
            PluginFormat != null &&
            HasInstallation &&
            !IsUpdatePending &&
            !IsCorePlugin;

        public bool CanConfigure =>
            InstalledPluginDefaultPresetViewModels.Any(x => x.SharedItems.Any());

        public bool IsConfigurePanelOpen { get; private set; }

        public bool CanUpdate {
            get {
                if (IsUpdatePending ||
                    InstalledFormat == null ||
                   SortedRemoteFormats.FirstOrDefault() is not { } max_remote) {
                    return false;
                }
                return InstalledFormat.version.ToVersion() < max_remote.version.ToVersion();
            }
        }


        // NOTE SelectedRemoteFormatIdx is a placeholder if multiple versions get supported
        public int SelectedRemoteFormatIdx { get; set; } = 0;

        #region Server Info
        bool IsUnpublished { get; set; }
        public DateTime? PluginPublishedDateTime { get; private set; }
        public int InstallCount { get; private set; }
        #endregion

        #region Format props

        public string SelectedRemotePackageUrl =>
            SelectedRemoteFormat == null ?
                null :
                SelectedRemoteFormat.packageUrl;

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
                if (PluginFormat is MpRuntimePlugin pf &&
                    pf.ManifestDir.IsDirectory()) {
                    return pf.ManifestDir;
                }
                if (MpPluginLoader.PluginGuidLookup.TryGetValue(PluginGuid, out var rp)) {
                    return rp.ManifestDir;
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
            await Task.Delay(1);
            PluginGuid = plugin_guid;

            RefreshState();
            IsBusy = false;
        }
        public void RefreshState() {
            if(!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(RefreshState);
                return;
            }
            OnPropertyChanged(nameof(PluginFormat));
            OnPropertyChanged(nameof(HasInstallation));
            OnPropertyChanged(nameof(CanUninstall));
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(IsUpdatePending));
            OnPropertyChanged(nameof(IconBase64));
            OnPropertyChanged(nameof(ShowDisabledInstallTooltip));
            OnPropertyChanged(nameof(ShowDisabledUpdateTooltip));
            OnPropertyChanged(nameof(DisabledInstallTooltip));
            OnPropertyChanged(nameof(DisabledUpdateTooltip));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(UpdateButtonText));
            OnPropertyChanged(nameof(CanConfigure));
            OnPropertyChanged(nameof(InstalledPluginDefaultPresetViewModels));
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
                case nameof(PluginFormat):
                    LoadIconAsync().FireAndForgetSafeAsync();
                    LoadDepViewModelAsync().FireAndForgetSafeAsync();
                    break;
                case nameof(IsSelected):
                    if (!IsSelected) {
                        break;
                    }
                    LoadPluginStatsAsync().FireAndForgetSafeAsync();
                    RefreshState();
                    OnPropertyChanged(nameof(RootDependencyCollection));
                    break;
                case nameof(IsAnyBusy):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    OnPropertyChanged(nameof(InstallButtonText));
                    break;
                case nameof(IsReadmeExpanded):
                    if (IsReadmeExpanded) {
                        LoadReadMeAsync().FireAndForgetSafeAsync();
                    }
                    break;
                case nameof(InstallProgressViewModel):
                    OnPropertyChanged(nameof(IsDownloading));
                    if(Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.IsSelectedDownloading));
                    }
                    break;
            }
        }

        private async Task LoadPluginStatsAsync() {
            if (PluginPublishedDateTime.HasValue || IsUnpublished) {
                // only set once if not refresh
                return;
            }

            (int count, DateTime? pub_dt) =
                await MpPluginLoader.GetOrUpdatePluginStatsAsync(PluginGuid, false);

            InstallCount = count;

            if (pub_dt.HasValue) {
                PluginPublishedDateTime = pub_dt;
                IsUnpublished = false;
            } else {
                PluginPublishedDateTime = null;
                IsUnpublished = true;
            }
        }
        private async Task LoadIconAsync() {
            if (string.IsNullOrEmpty(PluginIconUri) ||
                _iconBase64 != null) {
                return;
            }
            var icon_bytes = await MpFileIo.ReadBytesFromUriAsync(PluginIconUri, PluginManifestDirectory);
            if (icon_bytes == null || icon_bytes.Length == 0) {
                // assign plugin jigsaw so it only tries to find icon once
                IconBase64 = MpBase64Images.JigsawPiece;
            } else {
                IconBase64 = icon_bytes.ToBase64String();
            }
        }
        private async Task LoadReadMeAsync() {
            if (!string.IsNullOrEmpty(ReadMeHtml)) {
                // already loaded
                return;
            }
            if (string.IsNullOrEmpty(PluginReadMeUri)) {
                // no readme
                return;
            }
            IsReadmeLoading = true;

            var read_me_bytes = await MpFileIo.ReadBytesFromUriAsync(PluginReadMeUri, PluginManifestDirectory);
            string read_me_html = MpAvStringMarkDownToHtmlConverter.Instance.Convert(read_me_bytes.ToDecodedString(), null, null, null) as string;
            var html_doc = new HtmlDocument();
            html_doc.LoadHtml(read_me_html);

            if (PluginReadMeUri.ToLower().SplitNoEmpty("readme.md") is { } uri_parts &&
                uri_parts.Any() &&
                uri_parts.FirstOrDefault() is string read_me_base_uri &&
                Uri.IsWellFormedUriString(read_me_base_uri, UriKind.Absolute)) {

                var img_nodes = html_doc.DocumentNode.SelectNodes("//img");
                if (img_nodes != null) {
                    foreach (var img_node in img_nodes) {
                        if (img_node.GetAttributeValue("src", string.Empty) is not string src ||
                            Uri.IsWellFormedUriString(src, UriKind.Absolute)) {
                            // no src or already absolute
                            continue;
                        }
                        string new_src = read_me_base_uri + src;
                        if (Uri.IsWellFormedUriString(new_src, UriKind.Absolute)) {
                            // adjust relative uri to readmes base path
                            img_node.SetAttributeValue("src", new_src);
                        }
                    }
                }
                read_me_html = html_doc.DocumentNode.OuterHtml;
            }
            ReadMeHtml = read_me_html;
            IsReadmeLoading = false;
        }
        private async Task LoadDepViewModelAsync() {
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
                // await Dispatcher.UIThread.InvokeAsync(async () => {
                if (PluginFormat.GetComponentManager() is not { } cm) {
                    return;
                }

                InstallProgressViewModel = new MpAvCommonCancelableProgressIndicatorViewModel(this);
                IsBusy = true;
                bool success = await cm.InstallAsync(PluginGuid, SelectedRemotePackageUrl, InstallProgressViewModel);

                // TODO success should only return true if plugin installs fine but it gets gummed up w/
                // all the nested exception handling...this shouldn't be done here
                success = MpPluginLoader.PluginGuidLookup.ContainsKey(PluginGuid);

                MpConsole.WriteLine($"Install of {PluginTitle} plugin: {success.ToTestResultLabel()}");
                if (success) {
                    LoadPluginStatsAsync().FireAndForgetSafeAsync();
                } else {
                    // if install failed mark for delete to prevent startup failures
                    // this won't affect saved info

                    // TODO this should call MpPluginLoader.Delete
                    MpPluginLoader.UninstalledPluginGuids.Add(PluginGuid);
                    MpStartupCleaner.AddPathToDelete(Path.Combine(MpPluginLoader.PluginRootDir, PluginGuid));
                }

                IsBusy = false;
                InstallProgressViewModel = null;
                Parent.PerformFilterCommand.Execute("Refresh");
                // }, DOWNLOAD_PRIORITY);
            }, () => {
                return CanInstall;
            });

        public ICommand UpdatePluginCommand => new MpAsyncCommand(
            async () => {
                //await Dispatcher.UIThread.InvokeAsync(async () => {
                if (PluginFormat.GetComponentManager() is not { } cm) {
                    return;
                }

                IsBusy = true;
                UpdateProgressViewModel = new MpAvCommonCancelableProgressIndicatorViewModel(this);
                bool success = await cm.BeginUpdateAsync(PluginGuid, SelectedRemotePackageUrl, UpdateProgressViewModel);
                IsBusy = false;

                UpdateProgressViewModel = null;
                Parent.PerformFilterCommand.Execute("Refresh");
                //}, DOWNLOAD_PRIORITY);


            }, () => {
                return CanUpdate && !IsBusy;
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

                if (PluginFormat.GetComponentManager() is not { } cm) {
                    return;
                }
                IsBusy = true;
                await cm.UninstallAsync(PluginGuid);

                // refresh list to remove this plugin
                IsBusy = false;
                Parent.PerformFilterCommand.Execute("Refresh");
            }, () => {
                return CanUninstall;
            });
        public ICommand ToggleIsPluginInstalledCommand => new MpCommand(
            () => {
                if (HasInstallation) {
                    UninstallPluginCommand.Execute(null);
                } else {
                    InstallPluginCommand.Execute(null);
                }
            }, () => {
                if (IsBusy) {
                    return false;
                }
                if (HasInstallation) {
                    return UninstallPluginCommand.CanExecute(null);
                } else {
                    return InstallPluginCommand.CanExecute(null);
                }
            });

        public ICommand ShowConfigurePanelCommand => new MpCommand(
            () => {
                //if(!CanConfigure) {
                //    return;
                //}
                IsConfigurePanelOpen = true;
                InstalledPluginDefaultPresetViewModels.ForEach(x => x.OnPropertyChanged(nameof(x.SharedItems)));
            });
        
        public ICommand HideConfigurePanelCommand => new MpCommand(
            () => {
                //if(!IsConfigurePanelOpen) {
                //    return;
                //}
                IsConfigurePanelOpen = false;
            });
        #endregion

    }
}
