using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.Globalization;
using Xamarin.Forms.Internals;
using System.IO;
using MpWpfApp.Properties;
using System.Windows.Markup;
using MonkeyPaste;

namespace MpWpfApp {

    public enum MpYoloParamType {
        None = 0,
        Confidence
    }

    public class MpYoloViewModel : MpAnalyticItemViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region State

        #endregion

        #region Model


        #endregion

        #endregion

        #region Constructors

        public MpYoloViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods

        protected override async Task<object> ExecuteAnalysis(object obj) {
            IsBusy = true;

            var paramLookup = SelectedPresetViewModel.ParamLookup;
            double minConfidence = paramLookup[(int)MpYoloParamType.Confidence].DoubleValue;

            var response = await MpYoloTransaction.Instance.DetectObjectsAsync(obj.ToString().ToBitmapSource().ToByteArray(), minConfidence);
            

            IsBusy = false;

            return new Tuple<object, object>(response,minConfidence);
        }

        protected override async Task ConvertToCopyItem(int parentCopyItemId, object resultData, object reqStr) {
            var app = MpPreferences.Instance.ThisAppSource.App;
            var url = await MpUrlBuilder.Create(AnalyticItem.EndPoint, null, reqStr.ToString());
            var source = await MpSource.Create(app, url);

            var yoloResponse = JsonConvert.DeserializeObject<MpYoloResponse>(resultData.ToString());
            if(yoloResponse == null) {
                return;
            }
            foreach(var yoloBox in yoloResponse.DetectedObjects) {
                var dio = new MpDetectedImageObject() {
                    X = yoloBox.X,
                    Y = yoloBox.Y,
                    Width = yoloBox.Width,
                    Height = yoloBox.Height,
                    Confidence = yoloBox.Score,
                    ObjectTypeName = yoloBox.Label,
                    DetectedImageObjectGuid = System.Guid.NewGuid(),
                    CopyItemId = parentCopyItemId
                };
                await dio.WriteToDatabaseAsync();
            }

            var ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(parentCopyItemId);
            ci.ItemDescription = reqStr.ToString();
            await ci.WriteToDatabaseAsync();

            var scivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(parentCopyItemId);
            if (scivm == null) {
                return;
            }

            await scivm.Parent.InitializeAsync(scivm.Parent.HeadItem.CopyItem);

            scivm.Parent.ClearSelection(false);
            scivm.IsSelected = true;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
