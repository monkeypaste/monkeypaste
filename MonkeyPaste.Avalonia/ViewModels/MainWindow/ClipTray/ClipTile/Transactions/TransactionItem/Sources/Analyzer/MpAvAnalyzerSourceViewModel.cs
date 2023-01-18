using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyzerSourceViewModel : MpAvTransactionSourceViewModelBase {

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        public MpAvAnalyticItemPresetViewModel PresetViewModel =>
            MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == SourceObjId);

        #endregion

        #region State
        #endregion

        #region Model

        public MpPluginRequestFormatBase ParameterReqFormat { get; private set; }
        public int PresetId { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvAnalyzerSourceViewModel(MpAvTransactionItemViewModelBase parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpTransactionSource ts) {
            IsBusy = true;
            await base.InitializeAsync(ts);

            if(PresetViewModel == null) {
                ParameterReqFormat = null;
            } else {
                var req_lookup = MpJsonObject.DeserializeObject<Dictionary<string, object>>(SourceArg);
                if(req_lookup != null && 
                    req_lookup.TryGetValue("items",out var itemsObj) && itemsObj is JArray items_jarray) {
                    Dictionary<object, string> param_lookup = new Dictionary<object, string>();
                    foreach(var kvp_jtoken in items_jarray) {
                        if(kvp_jtoken.SelectToken("paramId",false) is JToken param_token &&
                            kvp_jtoken.SelectToken("value",false) is JToken val_token) {

                            param_lookup.Add(param_token.Value<string>(), val_token.Value<string>());
                        }
                    }
                    ParameterReqFormat = await MpPluginRequestBuilder.BuildRequestAsync(
                        paramFormats: PresetViewModel.ComponentFormat.parameters,
                        paramValues: param_lookup,
                        sourceContent: Parent.Parent.Parent.CopyItem);
                }
                
            }

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(PresetViewModel));
            OnPropertyChanged(nameof(Body));

            IsBusy = false;
        }

        #endregion

        #region Commands
        #endregion
    }
}
