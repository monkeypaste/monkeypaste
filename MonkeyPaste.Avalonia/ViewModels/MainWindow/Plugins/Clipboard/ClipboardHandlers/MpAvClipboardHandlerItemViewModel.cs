﻿using MonkeyPaste.Common;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardHandlerItemViewModel :
        MpAvTreeSelectorViewModelBase<MpAvClipboardHandlerCollectionViewModel, MpAvHandledClipboardFormatViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel,
        MpIAsyncComboBoxItemViewModel//,
                                     //MpIBoxViewModel
        {

        #region Private
        #endregion

        #region Interfaces

        #region MpIAsyncComboBoxItemViewModel Implementation
        int MpIComboBoxItemViewModel.IconId => PluginIconId;
        string MpIComboBoxItemViewModel.Label => HandlerName;

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpIMenuItemViewModel Implementation

        public MpAvMenuItemViewModel ContextMenuItemViewModel { get; }

        #endregion

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvHandledClipboardFormatViewModel> Writers =>
            Items.Where(x => x.IsWriter);
        public IEnumerable<MpAvHandledClipboardFormatViewModel> Readers =>
            Items.Where(x => !x.IsWriter);

        public IEnumerable<MpAvHandledClipboardFormatViewModel> TitleSortedItems =>
            Items
            .Where(x => x.IsWriter == IsFilterForWriters)
            .OrderBy(x => x.Title);

        public override MpAvHandledClipboardFormatViewModel SelectedItem { get; set; }
        #endregion


        #region MpAvTreeSelectorViewModelBase Overrides

        public override MpAvClipboardHandlerCollectionViewModel ParentTreeItem => Parent;

        #endregion



        #region State

        public bool IsFilterForWriters { get; set; }
        public int SelectedTitleSortedItemIdx {
            get {
                return TitleSortedItems.IndexOf(SelectedItem);
            }
            set {
                if (SelectedTitleSortedItemIdx != value) {
                    SelectedItem = value >= 0 && value < Items.Count ?
                        TitleSortedItems.ElementAt(value) : null;
                    OnPropertyChanged(nameof(SelectedTitleSortedItemIdx));
                }
            }

        }

        #endregion

        #region Model

        public int PluginIconId { get; private set; }


        #region Plugin

        public string HandlerName {
            get {
                if (PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.title;
            }
        }
        public string PluginGuid { get; private set; }
        public MpRuntimePlugin PluginFormat {
            get {
                var kvp = MpPluginLoader.PluginManifestLookup.FirstOrDefault(x => x.Value.guid == PluginGuid);
                if (kvp.IsDefault()) {
                    return null;
                }
                return kvp.Value;
            }
        }

        public MpClipboardComponent ClipboardPluginFormat => PluginFormat == null ? null : PluginFormat.oleHandler;

        #endregion

        #endregion

        #endregion

        #region Constructors

        public MpAvClipboardHandlerItemViewModel(MpAvClipboardHandlerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpClipboardHandlerItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(string ole_guid) {
            IsBusy = true;
            PluginGuid = ole_guid;

            bool is_plugin_valid = await ValidateClipboardHandlerAsync();
            if (!is_plugin_valid) {
                PluginGuid = null;
                IsBusy = false;
                return;
            }

            PluginIconId = await GetOrCreateIconIdAsync();

            foreach (var reader in ClipboardPluginFormat.readers) {
                var hcfvm = await CreateHandledClipboardFormatViewModelAsync(reader);
                Items.Add(hcfvm);
            }

            foreach (var writer in ClipboardPluginFormat.writers) {
                var hcfvm = await CreateHandledClipboardFormatViewModelAsync(writer);
                Items.Add(hcfvm);
            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            var invalidItems = Items.Where(x => !x.IsValid);
            for (int i = 0; i < invalidItems.Count(); i++) {
                Items.Remove(Items[i]);
            }

            OnPropertyChanged(nameof(Items));
            IsBusy = false;
        }

        public async Task<MpAvHandledClipboardFormatViewModel> CreateHandledClipboardFormatViewModelAsync(MpClipboardHandlerFormat format) {
            var hcfvm = new MpAvHandledClipboardFormatViewModel(this);
            await hcfvm.InitializeAsync(format);
            return hcfvm;
        }

        public async Task<MpOlePluginResponse> IssueOleRequestAsync(MpOlePluginRequest req, bool is_read) {
            string method_name =
                is_read ?
                    nameof(MpIOleReaderComponent.ProcessOleReadRequestAsync) :
                    nameof(MpIOleWriterComponent.ProcessOleWriteRequestAsync);
            string on_type = is_read ?
                typeof(MpIOleReaderComponent).FullName :
                typeof(MpIOleWriterComponent).FullName;
            var resp = await PluginFormat.IssueRequestAsync<MpOlePluginResponse>(method_name, on_type, req, clone_resp: false);
            return resp;
        }
        #endregion

        #region Private Methods

        private void MpClipboardHandlerItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        if (SelectedItem == null) {
                            if (Items.Count > 0) {
                                Items[0].IsSelected = true;
                            }
                        }
                    }
                    break;
                case nameof(SelectedItem):
                    Items.ForEach(x => x.IsSelected = x == SelectedItem);
                    OnPropertyChanged(nameof(SelectedTitleSortedItemIdx));
                    if (SelectedItem == null) {
                        IsFilterForWriters = false;
                    } else {
                        IsFilterForWriters = SelectedItem.IsWriter;
                    }
                    break;
                case nameof(IsFilterForWriters):
                    OnPropertyChanged(nameof(TitleSortedItems));
                    if (!TitleSortedItems.Contains(SelectedItem)) {
                        // on io toggle try to select format compliment
                        MpAvHandledClipboardFormatViewModel to_select = TitleSortedItems.FirstOrDefault();
                        if (SelectedItem != null &&
                            TitleSortedItems.FirstOrDefault(x => x.HandledFormat == SelectedItem.HandledFormat)
                                is MpAvHandledClipboardFormatViewModel compliment_vm) {
                            to_select = compliment_vm;
                        }
                        SelectedItem = to_select;
                        OnPropertyChanged(nameof(SelectedTitleSortedItemIdx));
                    }
                    break;
                case nameof(SelectedTitleSortedItemIdx):
                    Parent.OnPropertyChanged(nameof(Parent.SelectedPresetViewModel));
                    break;
            }
        }

        private async Task<bool> ValidateClipboardHandlerAsync() {
            if (PluginFormat == null) {
                MpConsole.WriteTraceLine("plugin error, not registered");
                return false;
            }
            if (PluginFormat.oleHandler == null) {
                MpConsole.WriteTraceLine("clipboard handler empty, ignoring");
                return false;
            }
            //if (PluginFormat.oleHandler.readers.Count == 0 &&
            //   PluginFormat.oleHandler.writers.Count == 0) {
            //    MpConsole.WriteTraceLine($"Plugin '{PluginFormat.title}' is identified as a clipboard handler but has no readers or writerss, ignoring");
            //    return false;
            //}

            //var sb = new StringBuilder();
            var error_notifications = new List<MpNotificationFormat>();

            if (PluginFormat.oleHandler.readers != null) {
                var dupNames = PluginFormat.oleHandler.readers.GroupBy(x => x.formatName).Where(x => x.Count() > 1);
                if (dupNames.Count() > 0) {
                    var sb = new StringBuilder();
                    //sb.AppendLine($"Clipboard reader/writer format names must be unique. Reader duplicates:");
                    sb.AppendLine(UiStrings.InvalidHandlerEx1);
                    dupNames.ForEach(x => sb.AppendLine(x.Key));
                    string test = sb.ToString();
                    error_notifications.Add(MpPluginLoader.CreateInvalidPluginNotification(sb.ToString(), PluginFormat));
                }
            }

            if (PluginFormat.oleHandler.writers != null) {
                var dupNames = PluginFormat.oleHandler.writers.GroupBy(x => x.formatName).Where(x => x.Count() > 1);
                if (dupNames.Count() > 0) {
                    var sb = new StringBuilder();
                    //sb.AppendLine($"Clipboard reader/writer format names must be unique. Writer duplicates:");
                    sb.AppendLine(UiStrings.InvalidHandlerEx2);
                    dupNames.ForEach(x => sb.AppendLine(x.Key));
                    error_notifications.Add(MpPluginLoader.CreateInvalidPluginNotification(sb.ToString(), PluginFormat));
                }
            }


            var allHandlers =
                PluginFormat.oleHandler.readers
                .Union(PluginFormat.oleHandler.writers)
                .ToList();

            var dupGuids = allHandlers.GroupBy(x => x.formatGuid).Where(x => x.Count() > 1);
            if (dupGuids.Count() > 0) {
                foreach (var dupGuid_group in dupGuids) {
                    var sb = new StringBuilder();
                    //sb.AppendLine($"Clipboard format guids must be unique. Duplicate formats & guids:");
                    sb.AppendLine(UiStrings.InvalidHandlerEx3);
                    dupGuid_group.ForEach(x => sb.AppendLine($"'{x.displayName}': {x.formatGuid}"));
                    error_notifications.Add(MpPluginLoader.CreateInvalidPluginNotification(sb.ToString(), PluginFormat));
                }
            }

            foreach (var handler in allHandlers) {
                var paramNameGroups = handler.parameters.GroupBy(x => x.paramId);
                foreach (var paramNameGroup in paramNameGroups) {
                    if (paramNameGroup.Count() <= 1) {
                        continue;
                    }
                    // TODO each notification type should probably have a pop up link to help...
                    // In this case more info should be given about paramName falling back to label, etc.
                    //string msg = $"plugin error: all plugin 'paramName' fields must be unique for handler '{handler.displayName}'." + Environment.NewLine;
                    //msg += $"paramName '{paramNameGroup.Key}' has multiple entries";
                    //string msg = $"Plugin Error: All plugin '{0}' fields must be unique for handler '{1}'.{2}paramName '{3}' has multiple entries";
                    string msg = UiStrings.InvalidHandlerEx4.Format(
                        UiStrings.CommonParamNameLabel,
                        handler.displayName,
                        Environment.NewLine,
                        paramNameGroup.Key);
                    error_notifications.Add(MpPluginLoader.CreateInvalidPluginNotification(msg, PluginFormat));
                }
            }

            bool needs_fixing = error_notifications.Count > 0;
            if (needs_fixing) {
                // only need first error to recurse

                var invalid_nf = error_notifications[0];

                invalid_nf.RetryAction = (args) => {
                    needs_fixing = false;
                    return null;
                };

                var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(invalid_nf);
                if (result == MpNotificationDialogResultType.Ignore) {
                    // ignoring these errors flags plugin to be completely ignored
                    return false;
                }
                while (needs_fixing) {
                    await Task.Delay(100);
                }

                _ = await MpPluginLoader.ReloadPluginAsync(PluginFormat.guid);
                OnPropertyChanged(nameof(PluginFormat));
                // loop through another validation pass
                bool is_valid = await ValidateClipboardHandlerAsync();
                return is_valid;
            }
            //MpConsole.WriteLine(output);

            return true;
        }

        private async Task<int> GetOrCreateIconIdAsync() {
            var bytes = await MpFileIo.ReadBytesFromUriAsync(PluginFormat.iconUri, PluginFormat.ManifestDir);
            var icon = await Mp.Services.IconBuilder.CreateAsync(bytes.ToBase64String());
            return icon.Id;
        }

        #endregion

        #region Commands
        #endregion
    }
}
