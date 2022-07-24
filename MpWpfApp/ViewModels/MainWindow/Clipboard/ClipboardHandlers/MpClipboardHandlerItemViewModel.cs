using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.IO;
using System.ComponentModel;
using System.Collections;

namespace MpWpfApp {
    public class MpClipboardHandlerItemViewModel :
        MpSelectorViewModelBase<MpClipboardHandlerCollectionViewModel, MpHandledClipboardFormatViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel, 
        MpITreeItemViewModel,
        MpIBoxViewModel,
        INotifyDataErrorInfo {

        #region Private

       
        #endregion

        #region Properties

        #region View Models

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
        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

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

        #region INotifyDataErrorInfo Implementation
        public IEnumerable GetErrors(string propertyName) {
            throw new NotImplementedException();
        }

        public bool HasErrors { get; }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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

        public MpClipboardHandlerItemViewModel(MpClipboardHandlerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpClipboardHandlerItemViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPluginFormat pf) {
            if (!ValidateClipboardHandler(pf)) {
                return;
            }
            IsBusy = true;            

            PluginFormat = pf;

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

        public async Task<MpHandledClipboardFormatViewModel> CreateHandledClipboardFormatViewModelAsync(MpClipboardHandlerFormat format) {
            var hcfvm = new MpHandledClipboardFormatViewModel(this);
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

        private bool ValidateClipboardHandler(MpPluginFormat pf) {
            if(pf == null) {
                MpConsole.WriteTraceLine("plugin error, not registered");
                return false;
            }
            if(pf.clipboardHandler == null) {
                MpConsole.WriteTraceLine("clipboard handler empty, ignoring");
                return false;            
            }
            if(pf.clipboardHandler.readers.Count == 0 &&
               pf.clipboardHandler.writers.Count == 0) {
                MpConsole.WriteTraceLine($"Plugin '{pf.title}' is identified as a clipboard handler but has no readers or writerss, ignoring");
                return false;
            }

            bool isValid = true;
            var sb = new StringBuilder();

            var dupNames = pf.clipboardHandler.readers.GroupBy(x => x.clipboardName).Where(x => x.Count() > 1);
            if (dupNames.Count() > 0) {
                sb.AppendLine("clipboard format names must be unique, " + String.Join(",", dupNames) + " are duplicated in readers");
                isValid = false;
            }

            dupNames = pf.clipboardHandler.writers.GroupBy(x => x.clipboardName).Where(x => x.Count() > 1);
            if (dupNames.Count() > 0) {
                sb.AppendLine("clipboard format names must be unique, " + String.Join(",", dupNames) + " are duplicated in writers");
                isValid = false;
            }
            var dupGuids = pf.clipboardHandler.readers.GroupBy(x => x.handlerGuid).Where(x => x.Count() > 1);
            if (dupGuids.Count() > 0) {
                sb.AppendLine("clipboard guids must be unique, " + 
                    String.Join(",", pf.clipboardHandler.readers.Where(x => 
                    dupGuids.Any(y => y.Key == x.handlerGuid)).Select(x => x.handlerGuid))+" are duplicated reader guids");
                isValid = false;
            }

            dupGuids = pf.clipboardHandler.writers.GroupBy(x => x.handlerGuid).Where(x => x.Count() > 1);
            if (dupGuids.Count() > 0) {
                sb.AppendLine("clipboard guids must be unique, " +
                    String.Join(",", pf.clipboardHandler.writers.Where(x =>
                    dupGuids.Any(y => y.Key == x.handlerGuid)).Select(x => x.handlerGuid)) + " are duplicated writer guids");
                isValid = false;
            }

            if(isValid) {
                return true;
            }
            MpConsole.WriteLine(sb.ToString());
            return false;
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
