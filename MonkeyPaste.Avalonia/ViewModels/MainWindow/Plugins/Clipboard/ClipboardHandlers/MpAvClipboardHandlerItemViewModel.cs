using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia.Plugin;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
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



        //#region MpIBoxViewModel Implementation
        //public double X { get; set; }
        //public double Y { get; set; }
        //public double Width { get; }
        //public double Height { get; }

        //#endregion

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
            Items.OrderBy(x => x.SelectorLabel);

        public override MpAvHandledClipboardFormatViewModel SelectedItem { get; set; }
        #endregion


        #region MpAvTreeSelectorViewModelBase Overrides

        public override MpAvClipboardHandlerCollectionViewModel ParentTreeItem => Parent;

        #endregion



        #region State

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

        public MpAvPluginFormat PluginFormat { get; set; }

        public MpClipboardHandlerFormats ClipboardPluginFormat => PluginFormat == null ? null : PluginFormat.oleHandler;

        #endregion

        #endregion

        #endregion

        #region Constructors

        public MpAvClipboardHandlerItemViewModel(MpAvClipboardHandlerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpClipboardHandlerItemViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPluginFormat pf) {
            IsBusy = true;

            PluginFormat = pf as MpAvPluginFormat;
            bool is_plugin_valid = await ValidateClipboardHandlerAsync();
            if (!is_plugin_valid) {
                PluginFormat = null;
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
                    sb.AppendLine($"Clipboard reader/writer format names must be unique. Reader duplicates:");
                    dupNames.ForEach(x => sb.AppendLine(x.Key));
                    string test = sb.ToString();
                    error_notifications.Add(MpPluginLoader.CreateInvalidPluginNotification(sb.ToString(), PluginFormat));
                }
            }

            if (PluginFormat.oleHandler.writers != null) {
                var dupNames = PluginFormat.oleHandler.writers.GroupBy(x => x.formatName).Where(x => x.Count() > 1);
                if (dupNames.Count() > 0) {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Clipboard reader/writer format names must be unique. Writer duplicates:");
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
                    sb.AppendLine($"Clipboard format guids must be unique. Duplicate formats & guids:");
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
                    string msg = $"plugin error: all plugin 'paramName' fields must be unique for handler '{handler.displayName}'." + Environment.NewLine;
                    //msg += $" '{string.Join(" and ", paramNameGroup.Key.Select(x => $"'{x}'"))}'";
                    msg += $"paramName '{paramNameGroup.Key}' has multiple entries";
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

                PluginFormat = (await MpPluginLoader.ReloadPluginAsync(Path.Combine(PluginFormat.RootDirectory, "manifest.json"))) as MpAvPluginFormat;
                // loop through another validation pass
                return await ValidateClipboardHandlerAsync();
            }
            //MpConsole.WriteLine(output);

            return true;
        }

        private async Task<int> GetOrCreateIconIdAsync() {
            var bytes = await MpFileIo.ReadBytesFromUriAsync(PluginFormat.iconUri, PluginFormat.RootDirectory);
            var icon = await Mp.Services.IconBuilder.CreateAsync(bytes.ToBase64String());
            return icon.Id;
        }

        #endregion
    }
}
