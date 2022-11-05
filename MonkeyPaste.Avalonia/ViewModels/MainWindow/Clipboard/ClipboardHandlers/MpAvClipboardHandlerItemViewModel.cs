using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System.IO;
using System.ComponentModel;
using System.Collections;
using Avalonia.Controls.Notifications;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardHandlerItemViewModel :
        MpAvSelectorViewModelBase<MpAvClipboardHandlerCollectionViewModel, MpAvHandledClipboardFormatViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel, 
        MpITreeItemViewModel,
        MpIBoxViewModel {

        #region Private


        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvHandledClipboardFormatViewModel> Writers => Items.Where(x => x.IsWriter);
        public IEnumerable<MpAvHandledClipboardFormatViewModel> Readers => Items.Where(x => !x.IsWriter);

        #endregion

        #region MpIBoxViewModel Implementation
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; }
        public double Height { get; }

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => Parent;
        public IEnumerable<MpITreeItemViewModel> Children => Items;// new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel { get; }

        #endregion

        #region State

        #region Drag Drop

        public bool IsDraggingToExternal { get; set; }

        #endregion

        #endregion

        #region Model

        public int PluginIconId { get; private set; }


        #region Plugin

        public string HandlerName {
            get {
                if(PluginFormat == null) {
                    return string.Empty;
                }
                return PluginFormat.title;
            }
        }

        public MpPluginFormat PluginFormat { get; set; }

        public MpClipboardHandlerFormats ClipboardPluginFormat => PluginFormat == null ? null : PluginFormat.clipboardHandler;

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

            PluginFormat = pf;
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

            while(Items.Any(x=>x.IsBusy)) {
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
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    if(IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        if(SelectedItem == null) {
                            if(Items.Count > 0) {
                                Items[0].IsSelected = true;
                            }
                        }
                    }
                    break;
            }
        }

        private async Task<bool> ValidateClipboardHandlerAsync() {
            if(PluginFormat == null) {
                MpConsole.WriteTraceLine("plugin error, not registered");
                return false;
            }
            if(PluginFormat.clipboardHandler == null) {
                MpConsole.WriteTraceLine("clipboard handler empty, ignoring");
                return false;            
            }
            if(PluginFormat.clipboardHandler.readers.Count == 0 &&
               PluginFormat.clipboardHandler.writers.Count == 0) {
                MpConsole.WriteTraceLine($"Plugin '{PluginFormat.title}' is identified as a clipboard handler but has no readers or writerss, ignoring");
                return false;
            }

            //var sb = new StringBuilder();
            var error_notifications = new List<MpNotificationFormat>();

            var dupNames = PluginFormat.clipboardHandler.readers.GroupBy(x => x.clipboardName).Where(x => x.Count() > 1);
            if (dupNames.Count() > 0) {
                string msg = $"plugin error: clipboard format names must be unique, {string.Join(",", dupNames)} are duplicated in readers";
                error_notifications.Add(CreateInvalidNotification(msg,PluginFormat));
            }

            dupNames = PluginFormat.clipboardHandler.writers.GroupBy(x => x.clipboardName).Where(x => x.Count() > 1);
            if (dupNames.Count() > 0) {
                string msg = $"plugin error: clipboard format names must be unique, {string.Join(",", dupNames)} are duplicated in writers";
                error_notifications.Add(CreateInvalidNotification(msg,PluginFormat));
            }

            var allHandlers = new List<MpClipboardHandlerFormat>();
            allHandlers.AddRange(PluginFormat.clipboardHandler.readers);
            allHandlers.AddRange(PluginFormat.clipboardHandler.writers);

            var dupGuids = allHandlers.GroupBy(x => x.handlerGuid).Where(x => x.Count() > 1);
            if (dupGuids.Count() > 0) {
                foreach(var dupGuid_group in dupGuids) {
                    string msg = "plugin error: clipboard 'handlerGuid' must be unique." + Environment.NewLine;
                    msg += $"'{string.Join(" and ", dupGuid_group.Select(x => $"'{x.displayName}'"))}'";
                    msg += $" have matching 'handlerGuid': '{dupGuid_group.Key}'";
                    error_notifications.Add(CreateInvalidNotification(msg,PluginFormat));
                }
            }

            foreach (var handler in allHandlers) {
                var paramNameGroups = handler.parameters.GroupBy(x => x.paramName);
                foreach(var paramNameGroup in paramNameGroups) {
                    if(paramNameGroup.Count() <= 1) {
                        continue;
                    }
                    // TODO each notification type should probably have a pop up link to help...
                    // In this case more info should be given about paramName falling back to label, etc.
                    string msg = $"plugin error: all plugin 'paramName' fields must be unique for handler '{handler.displayName}'." + Environment.NewLine;
                    //msg += $" '{string.Join(" and ", paramNameGroup.Key.Select(x => $"'{x}'"))}'";
                    msg += $"paramName '{paramNameGroup.Key}' has multiple entries";
                    error_notifications.Add(CreateInvalidNotification(msg, PluginFormat));
                }
                
            }

            bool needs_fixing = error_notifications.Count > 0;
            if(needs_fixing) {
                // only need first error to recurse

                var invalid_nf = error_notifications[0];

                invalid_nf.RetryAction = (args) => {
                    needs_fixing = false;
                };

                var result = await MpNotificationBuilder.ShowNotificationAsync(invalid_nf);
                if (result == MpNotificationDialogResultType.Ignore) {
                    // ignoring these errors flags plugin to be completely ignored
                    return false;
                }
                while (needs_fixing) {
                    await Task.Delay(100);
                }

                PluginFormat = await MpPluginLoader.ReloadPluginAsync(Path.Combine(PluginFormat.RootDirectory, "manifest.json"));
                // loop through another validation pass
                return await ValidateClipboardHandlerAsync();
            }
            //MpConsole.WriteLine(output);

            return true;
        }

        private MpNotificationFormat CreateInvalidNotification(string msg, MpPluginFormat pf) {
            return new MpNotificationFormat() {
                Title = $"{pf.title} Error",
                Body = msg,
                NotificationType = MpNotificationType.InvalidPlugin,
                FixCommand = new MpCommand(() => MpFileIo.OpenFileBrowser(Path.Combine(pf.RootDirectory,"manifest.json")))
            };
        }

        private async Task<int> GetOrCreateIconIdAsync() {
            var bytes = await MpFileIo.ReadBytesFromUriAsync(PluginFormat.iconUri, PluginFormat.RootDirectory);
            var icon = await MpIcon.Create(
                iconImgBase64: bytes.ToBase64String(),
                createBorder: false);
            return icon.Id;
        }

        #endregion
    }
}
