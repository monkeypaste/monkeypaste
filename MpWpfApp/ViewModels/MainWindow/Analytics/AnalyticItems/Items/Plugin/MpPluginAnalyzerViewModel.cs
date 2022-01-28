using MonkeyPaste;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MonkeyPaste.Plugin;
using static SQLite.SQLite3;

namespace MpWpfApp {
    public class MpPluginAnalyzerViewModel : MpAnalyticItemViewModel {
        #region Properties

        #region Model

        //public string ParameterFormatJsonStr { get; set; }

        public MpAnalyzerPluginFormat AnalyzerPluginFormat { get; set; }

        public MpIAnalyzerPluginComponent AnalyzerPluginComponent { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpPluginAnalyzerViewModel() : base(null) { }

        public MpPluginAnalyzerViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) { 
            
        }

        #endregion

        public async Task InitializeAsync(MpPlugin plugin, int typeIdx,int analyzerIdx) {
            if (IsLoaded) {
                return;
            }
            IsBusy = true;

            AnalyzerPluginFormat = plugin.types[typeIdx].analyzer[analyzerIdx];
            var compObj = plugin.Components.FirstOrDefault(x => x is MpIAnalyzerPluginComponent);
            if(compObj == null) {
                throw new Exception("Cannot find component");
            }
            AnalyzerPluginComponent = compObj as MpIAnalyzerPluginComponent;

            MpIcon icon = null;

            if(!string.IsNullOrEmpty(plugin.iconUrl)) {
                var bytes = await MpFileIo.ReadBytesFromUriAsync(plugin.iconUrl);
                icon = await MpIcon.Create(bytes.ToBitmapSource().ToBase64String(), false);
            }
            icon = icon == null ? MpPreferences.ThisAppSource.App.Icon : icon;

            MpCopyItemType inputFormat = MpCopyItemType.None;
            if(AnalyzerPluginFormat.inputTypes[0].image) {
                inputFormat = MpCopyItemType.Image;
            }

            MpOutputFormatType outputFormat = MpOutputFormatType.None;
            if (AnalyzerPluginFormat.outputTypes[0].text) {
                outputFormat |= MpOutputFormatType.Text;
            }
            if (AnalyzerPluginFormat.outputTypes[0].boundingbox) {
                outputFormat |= MpOutputFormatType.BoundingBox;
            }
            AnalyticItem = await MpAnalyticItem.Create(
                guid: AnalyzerPluginFormat.guid,
                title: plugin.title,
                description: plugin.description,
                endPoint: AnalyzerPluginComponent.GetType().AssemblyQualifiedName,
                inputFormat: inputFormat,
                outputFormat: outputFormat,
                iconId: icon.Id,
                parameterFormatResourcePath: AnalyzerPluginFormat.parametersResourcePath,
                apiKey: string.Empty);


            var presets = new List<MpAnalyticItemPreset>();
            foreach (var presetFormat in AnalyzerPluginFormat.presets) {
                int idx = AnalyzerPluginFormat.presets.IndexOf(presetFormat);
                int presetId = MpHelpers.Rand.Next(1000, int.MaxValue);
                var aipvl = new List<MpAnalyticItemPresetParameterValue>();
                foreach(var paramVal in presetFormat.values) {
                    var aippv = new MpAnalyticItemPresetParameterValue() {
                        AnalyticItemPresetParameterValueGuid = System.Guid.NewGuid(),
                        AnalyticItemPresetId = presetId,
                        ParameterEnumId = paramVal.enumId,
                        Value = paramVal.value
                    };
                    aipvl.Add(aippv);
                }
                var aip = new MpAnalyticItemPreset() {
                    Id = presetId,
                    AnalyticItem = AnalyticItem,
                    AnalyticItemId = AnalyticItem.Id,
                    IsDefault = idx == 0,
                    Label = presetFormat.label,
                    SortOrderIdx = AnalyzerPluginFormat.presets.IndexOf(presetFormat),
                    PresetParameterValues = aipvl
                };
                presets.Add(aip);
            }

            // Init Presets
            //ParameterFormatJsonStr = JsonConvert.SerializeObject(AnalyzerPluginFormat.parametersResourcePath);

            PresetViewModels.Clear();

            if (AnalyticItem.Presets.Count == 0) {
                var naipvm = await CreatePresetViewModel(null);
                PresetViewModels.Add(naipvm);
            } else {
                foreach (var preset in AnalyticItem.Presets.OrderBy(x => x.SortOrderIdx)) {
                    var naipvm = await CreatePresetViewModel(preset);
                    PresetViewModels.Add(naipvm);
                }
            }

            PresetViewModels.OrderBy(x => x.SortOrderIdx);

            var defPreset = PresetViewModels.FirstOrDefault(x => x.IsDefault);
            MpAssert.Assert(defPreset, $"Error no default preset for anayltic item {AnalyticItem.Title}");


            OnPropertyChanged(nameof(IconId));
            OnPropertyChanged(nameof(PresetViewModels));

            IsBusy = false;
        }
        protected override async Task<MpRestTransaction> ExecuteAnalysis(object obj) {
            if(AnalyzerPluginFormat == null) {
                return null;
            }

            IsBusy = true;
            MonkeyPaste.MpAnalyzerPluginRequestFormat requestFormat = new MonkeyPaste.MpAnalyzerPluginRequestFormat();

            var request = new List<MonkeyPaste.MpAnalyzerPluginTransactionValueFormat>();

            var paramLookup = SelectedPresetViewModel.ParamLookup;
            foreach(var reqVal in requestFormat.values) {

                string curVal;
                if(reqVal.enumId == 0) {
                    //for content parameter
                    curVal = obj.ToString();
                } else {
                    curVal = paramLookup[reqVal.enumId].CurrentValue;
                }
                request.Add(new MonkeyPaste.MpAnalyzerPluginTransactionValueFormat() {
                    enumId = reqVal.enumId,
                    valueType = reqVal.valueType,
                    name = reqVal.name,
                    value = curVal
                });
            }

            var resultObj = await AnalyzerPluginComponent.Analyze(request);

            var results = new List<MonkeyPaste.MpAnalyzerPluginTransactionValueFormat>();
            var temp = JsonConvert.DeserializeObject<MonkeyPaste.MpAnalyzerPluginTransactionValueFormat>(resultObj.ToString());

            if (temp == null) {
                results = JsonConvert.DeserializeObject<List<MonkeyPaste.MpAnalyzerPluginTransactionValueFormat>>(resultObj.ToString());
            } else {
                results.Add(temp);
            }

            if (AnalyzerPluginFormat.outputTypes[0].boundingbox) {
                var bbl = new List<MpDetectedImageItem>();

                foreach(var result in results) {
                        
                }
            }
            IsBusy = false;

            return null;
        }
    }
}
