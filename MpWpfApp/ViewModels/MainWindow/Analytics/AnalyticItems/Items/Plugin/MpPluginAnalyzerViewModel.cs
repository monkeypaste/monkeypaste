using MonkeyPaste;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using  MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpPluginAnalyzerViewModel : MpAnalyticItemViewModel {
        #region Properties

        #region Model

        //public string ParameterFormatJsonStr { get; set; }

        public MpAnalyzerPluginFormat AnalyzerPluginFormat { get; set; }

        public MonkeyPaste.Plugin.MpIAnalyzerPluginComponent AnalyzerPluginComponent { get; set; }
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

            AnalyzerPluginFormat = plugin.types[typeIdx].analyzers[analyzerIdx];
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
            if(AnalyzerPluginFormat.inputType.image) {
                inputFormat = MpCopyItemType.Image;
            } // TODO add other formats as plugins are implemented


            string resourcePath = null;
            if(AnalyzerPluginComponent is MpCommandLinePlugin clp) {
                resourcePath = clp.Endpoint;
            } else {
                // TODO will need to add MpHttpPlugin and check if the component is local/remote
                // when its remote set endpoint, etc. (def need to stay flexible w/ api token stuff)
            }


            AnalyticItem = await MpAnalyticItem.Create(
                guid: AnalyzerPluginFormat.guid,
                title: plugin.title,
                description: plugin.description,
                endPoint: AnalyzerPluginComponent.GetType().AssemblyQualifiedName,
                inputFormat: inputFormat,
                iconId: icon.Id,
                parameterFormatResourcePath: resourcePath,
                apiKey: string.Empty);


            

            // Init Presets
            //ParameterFormatJsonStr = JsonConvert.SerializeObject(AnalyzerPluginFormat.parametersResourcePath);

            PresetViewModels.Clear();

            if(AnalyticItem.Presets == null || AnalyticItem.Presets.Count == 0) {
                //for new plugins create default presets
                for (int i = 0; i < AnalyzerPluginFormat.presets.Count; i++) {
                    var preset = AnalyzerPluginFormat.presets[i];

                    var aip = await MpAnalyticItemPreset.Create(
                        analyticItem: AnalyticItem,
                        isDefault: i == 0,
                        label: preset.label,
                        icon: AnalyticItem.Icon,
                        sortOrderIdx: i,
                        description: preset.description);

                    foreach(var paramVal in preset.values) {
                        var aipv = await MpAnalyticItemPresetParameterValue.Create(
                            parentItem: aip,
                            paramEnumId: paramVal.enumId,
                            value: paramVal.value);

                        aip.PresetParameterValues.Add(aipv);
                    }

                    AnalyticItem.Presets.Add(aip);
                }
            } 
            if (AnalyticItem.Presets == null || AnalyticItem.Presets.Count == 0) {
                AnalyticItem.Presets = new List<MpAnalyticItemPreset>() { null };
            } 

            foreach (var preset in AnalyticItem.Presets) {
                var naipvm = await CreatePresetViewModel(preset);
                PresetViewModels.Add(naipvm);
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

            var requestItems = new List<MpAnalyzerPluginRequestValueFormat>();

            var paramLookup = SelectedPresetViewModel.ParamLookup;
            foreach(var kvp in paramLookup) {
                MpAnalyzerPluginRequestValueFormat request = new MpAnalyzerPluginRequestValueFormat();

                var paramFormat = AnalyzerPluginFormat.parameters.FirstOrDefault(x => x.EnumId == kvp.Key);
                if(paramFormat == null) {
                    continue;
                }
                if(paramFormat.ParameterType == MpAnalyticItemParameterType.Content) {
                    // TODO (maybe)need to implement a request format so other properties can be passed
                    request = new MpAnalyzerPluginRequestValueFormat() {
                        enumId = kvp.Key,
                        value = obj.ToString()
                    };
                } else {
                    request = new MpAnalyzerPluginRequestValueFormat() {
                        enumId = kvp.Key,
                        value = kvp.Value.CurrentValue
                    };
                }
                requestItems.Add(request);
            }

            var resultObj = await AnalyzerPluginComponent.AnalyzeAsync(JsonConvert.SerializeObject(requestItems));

            var results = new List<MpAnalyzerPluginResponseValueFormat>();
            var temp = JsonConvert.DeserializeObject<MpAnalyzerPluginResponseValueFormat>(resultObj.ToString());

            if (temp == null) {
                results = JsonConvert.DeserializeObject<List<MpAnalyzerPluginResponseValueFormat>>(resultObj.ToString());
            } else {
                results.Add(temp);
            }

            
            if (AnalyzerPluginFormat.outputType.box) {
                var bbl = new List<MpDetectedImageObject>();

                foreach (var item in results) {
                    var dio = new MpDetectedImageObject() {
                        X = item.box.x,
                        Y = item.box.y,
                        Width = item.box.width,
                        Height = item.box.height,
                        ObjectTypeName = item.text,
                        Confidence = item.decimalVal
                    };
                    bbl.Add(dio);
                }
                await Task.WhenAll(bbl.Select(x => x.WriteToDatabaseAsync()));

            }
            IsBusy = false;

            return null;
        }
    }
}
