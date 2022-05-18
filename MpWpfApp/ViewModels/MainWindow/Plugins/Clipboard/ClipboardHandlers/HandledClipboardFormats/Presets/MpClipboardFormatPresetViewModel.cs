using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpClipboardFormatPresetViewModel : 
        MpSelectorViewModelBase<MpHandledClipboardFormatViewModel,MpAnalyticItemParameterViewModelBase>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpISidebarItemViewModel {

        #region Properties

        #region MpISelectableViewModel Implementation
        public bool IsSelected { get; set; }
        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISidebarItemViewModel Implemntation

        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public bool IsSidebarVisible { get; set; } = false;
        public MpISidebarItemViewModel NextSidebarItem => null;
        public MpISidebarItemViewModel PreviousSidebarItem => MpClipboardHandlerCollectionViewModel.Instance;

        #endregion

        #region Model

        public MpAnalyticItemPreset Preset { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpClipboardFormatPresetViewModel(MpHandledClipboardFormatViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpAnalyticItemPreset aip) {
            IsBusy = true;

            Items.Clear();

            Preset = aip;//await MpDb.GetItemAsync<MpAnalyticItemPreset>(aip.Id);

            var presetValues = await MpDataModelProvider.GetAnalyticItemPresetValuesByPresetId(Preset.Id);
            foreach (var paramFormat in Preset.AnalyzerFormat.parameters) {
                if (!presetValues.Any(x => x.ParamId == paramFormat.paramId)) {
                    string paramVal = string.Empty;
                    if (paramFormat.values != null && paramFormat.values.Count > 0) {
                        if (paramFormat.values.Any(x => x.isDefault)) {
                            paramVal = paramFormat.values.Where(x => x.isDefault).Select(x => x.value).ToList().ToCsv();
                        } else {
                            paramVal = paramFormat.values[0].value;
                        }
                    }
                    var newPresetVal = await MpAnalyticItemPresetParameterValue.Create(
                        presetId: Preset.Id,
                        paramEnumId: paramFormat.paramId,
                        value: paramVal,
                        format: paramFormat);

                    presetValues.Add(newPresetVal);
                }
            }
            presetValues.ForEach(x => x.ParameterFormat = Preset.AnalyzerFormat.parameters.FirstOrDefault(y => y.paramId == x.ParamId));

            foreach (var paramVal in presetValues) {
                //var naipvm = await CreateParameterViewModel(paramVal);
                //Items.Add(naipvm);
            }


            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            //OnPropertyChanged(nameof(ShortcutViewModel));
            OnPropertyChanged(nameof(Items));
            Items.ForEach(x => x.Validate());
            HasModelChanged = false;

            IsBusy = false;
        }

        #endregion
    }
}
