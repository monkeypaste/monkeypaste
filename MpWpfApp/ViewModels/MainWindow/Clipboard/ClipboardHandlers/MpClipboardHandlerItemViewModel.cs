using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpClipboardHandlerItemViewModel :
        MpSelectorViewModelBase<MpClipboardHandlerCollectionViewModel, MpHandledClipboardFormatViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel, 
        MpITreeItemViewModel {

        #region Properties

        #region View Models


        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => Parent;
        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel MenuItemViewModel { get; }

        #endregion

        #region Model

        public MpPluginFormat PluginFormat { get; set; }

        public MpClipboardHandlerFormats ClipboardPluginFormat => PluginFormat == null ? null : PluginFormat.clipboardHandler;

        #endregion

        #endregion

        #region Constructors

        public MpClipboardHandlerItemViewModel(MpClipboardHandlerCollectionViewModel parent) : base(parent) { }


        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpPluginFormat pf) {
            IsBusy = true;

            PluginFormat = pf;

            foreach(var hcf in ClipboardPluginFormat.handledFormats) {
                var hcfvm = await CreateHandledClipboardFormatViewModel(PluginFormat, ClipboardPluginFormat.handledFormats.IndexOf(hcf));
                Items.Add(hcfvm);
            }
            while(Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));
            IsBusy = false;
        }

        public async Task<MpHandledClipboardFormatViewModel> CreateHandledClipboardFormatViewModel(MpPluginFormat pf, int handlerIdx) {
            var hcfvm = new MpHandledClipboardFormatViewModel(this);
            await hcfvm.InitializeAsync(pf,handlerIdx);
            return hcfvm;
        }

        #endregion
    }
}
