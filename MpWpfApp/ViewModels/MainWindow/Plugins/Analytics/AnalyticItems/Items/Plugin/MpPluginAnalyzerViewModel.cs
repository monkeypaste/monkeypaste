using MonkeyPaste;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using  MonkeyPaste.Plugin;
using System.Diagnostics;

namespace MpWpfApp {
    public class MpPluginAnalyzerViewModel : MpAnalyticItemViewModel {
        #region Properties

        #region Model

        public MpPluginFormat PluginFormat { get; set; }

        public MpAnalyzerPluginFormat AnalyzerPluginFormat => PluginFormat == null ? null : PluginFormat.analyzer;

        public MpIAnalyzerPluginComponent AnalyzerPluginComponent => PluginFormat == null ? null : PluginFormat.LoadedComponent as MpIAnalyzerPluginComponent;
        #endregion

        #endregion

        #region Constructors

        public MpPluginAnalyzerViewModel() : base(null) { }

        public MpPluginAnalyzerViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) { 
            
        }

        #endregion

        public async Task InitializeAsync(MpPluginFormat analyzerPlugin) {
            if (IsLoaded) {
                return;
            }
            IsBusy = true;

            PluginFormat = analyzerPlugin;
            if(AnalyzerPluginComponent == null) {
                throw new Exception("Cannot find component");
            }

            MpIcon icon = null;

            if(!string.IsNullOrEmpty(analyzerPlugin.iconUrl)) {
                var bytes = await MpFileIo.ReadBytesFromUriAsync(analyzerPlugin.iconUrl);
                icon = await MpIcon.Create(bytes.ToBitmapSource().ToBase64String(), false);
            }
            icon = icon == null ? MpPreferences.ThisAppSource.App.Icon : icon;

            MpAnalyzerInputFormatFlags inputFlags = MpAnalyzerInputFormatFlags.None;
            if(AnalyzerPluginFormat.inputType.image) {
                inputFlags |= MpAnalyzerInputFormatFlags.Image;
            } // TODO add other formats as plugins are implemented


            //string resourcePath = null;
            //if(this.AnalyzerPluginComponent is MpCommandLinePlugin clp) {
            //    resourcePath = clp.Endpoint;
            //} else {
            //    // TODO will need to add MpHttpPlugin and check if the component is local/remote
            //    // when its remote set endpoint, etc. (def need to stay flexible w/ api token stuff)
            //}


            //AnalyticItem = await MpAnalyticItem.Create(
            //    guid: analyzerPlugin.guid,
            //    title: analyzerPlugin.title,
            //    description: analyzerPlugin.description,
            //    endPoint: this.AnalyzerPluginComponent.GetType().AssemblyQualifiedName,
            //    inputFormat: inputFlags,
            //    iconId: icon.Id,
            //    parameterFormatResourcePath: resourcePath,
            //    apiKey: string.Empty);


            

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

            foreach (var preset in AnalyzerPluginFormat.presets) {
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
        protected override async Task<MpAnalyzerTransaction> ExecuteAnalysis(object obj) {
            if(AnalyzerPluginFormat == null) {
                return null;
            }

            IsBusy = true;

            var requestItems = new List<MpAnalyzerPluginRequestItemFormat>();

            foreach(var kvp in SelectedPresetViewModel.ParamLookup) {
                MpAnalyzerPluginRequestItemFormat requestItem = new MpAnalyzerPluginRequestItemFormat();

                var paramFormat = AnalyzerPluginFormat.parameters.FirstOrDefault(x => x.enumId == kvp.Key);
                if(paramFormat == null) {
                    continue;
                }
                if(paramFormat.parameterControlType == MpAnalyticItemParameterControlType.Hidden) {
                    // TODO (maybe)need to implement a request format so other properties can be passed
                    requestItem = new MpAnalyzerPluginRequestItemFormat() {
                        enumId = kvp.Key,
                        value = obj.ToString()
                    };
                } else {
                    requestItem = new MpAnalyzerPluginRequestItemFormat() {
                        enumId = kvp.Key,
                        value = kvp.Value.CurrentValue
                    };
                }
                requestItems.Add(requestItem);
            }

            var request = JsonConvert.SerializeObject(requestItems);
            var resultObj = await AnalyzerPluginComponent.AnalyzeAsync(request);

            if(resultObj == null) {
                Debugger.Break();
                return null;
            }

            var transaction = new MpAnalyzerTransaction() {
                Request = request
            };

            if(AnalyzerPluginFormat.outputType.box) {
                var boxes = JsonConvert.DeserializeObject<List<MpAnalyzerPluginBoxResponseValueFormat>>(resultObj.ToString());
                var diol = new List<MpDetectedImageObject>();
                foreach (var item in boxes) { 
                    var dio = new MpDetectedImageObject() {
                        X = item.X,
                        Y = item.Y,
                        Width = item.Width,
                        Height = item.Height,
                        Label = item.Label,
                        Description = item.Description,
                        Score = item.Score                        
                    };
                    diol.Add(dio);
                }
                transaction.Response = diol;
            }

            IsBusy = false;

            return transaction;
        }
    }
}
