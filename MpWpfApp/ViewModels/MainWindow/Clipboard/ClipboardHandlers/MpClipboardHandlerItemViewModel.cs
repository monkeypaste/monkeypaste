using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpClipboardHandlerItemViewModel :
        MpSelectorViewModelBase<MpClipboardHandlerCollectionViewModel, MpHandledClipboardFormatViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel, 
        MpITreeItemViewModel,
        MpIBoxViewModel {

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

        public MpMenuItemViewModel MenuItemViewModel { get; }

        #endregion

        #region State

        #region Drag Drop

        public bool IsDraggingToExternal { get; set; }
        #endregion

        #endregion

        #region Model
        public int IconId {
            get {
                if(Items == null || Items.Count == 0) {
                    return 0;
                }
                return Items[0].IconId;
            }
        }
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

        #region Constructors

        public MpClipboardHandlerItemViewModel(MpClipboardHandlerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpClipboardHandlerItemViewModel_PropertyChanged;
        }


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
            OnPropertyChanged(nameof(IconId));
            IsBusy = false;
        }

        public async Task<MpHandledClipboardFormatViewModel> CreateHandledClipboardFormatViewModel(MpPluginFormat pf, int handlerIdx) {
            var hcfvm = new MpHandledClipboardFormatViewModel(this);
            await hcfvm.InitializeAsync(pf,handlerIdx);
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


        #endregion
    }
}
