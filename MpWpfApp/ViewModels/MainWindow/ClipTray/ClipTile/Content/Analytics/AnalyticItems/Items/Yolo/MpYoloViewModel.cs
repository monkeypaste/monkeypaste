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

        protected override async Task<MpRestTransaction> ExecuteAnalysis(object obj) {
            IsBusy = true;

            var paramLookup = SelectedPresetViewModel.ParamLookup;
            double minConfidence = paramLookup[(int)MpYoloParamType.Confidence].DoubleValue;

            var response = await MpYoloTransaction.DetectObjectsAsync(obj.ToString().ToBitmapSource().ToByteArray(), minConfidence);
            

            IsBusy = false;

            //return new Tuple<object, object>(response,minConfidence);
            return new MpRestTransaction() {
                Request = minConfidence,
                Response = response
            };
        }

        protected override async Task<MpCopyItem> ConvertToCopyItem(int parentCopyItemId, MpRestTransaction trans, bool suppressWrite = false) {
            object reqStr = trans.Request;
            object resultData = trans.Response;

            var app = MpPreferences.Instance.ThisAppSource.App;
            var url = await MpUrlBuilder.Create(AnalyticItem.EndPoint, null, reqStr.ToString());
            var source = await MpSource.Create(app, url);

            var yoloResponse = JsonConvert.DeserializeObject<MpYoloResponse>(resultData.ToString());
            if(yoloResponse == null) {
                return null;
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
                if(!suppressWrite) {
                    await dio.WriteToDatabaseAsync();
                }
            }

            var ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(parentCopyItemId);
            ci.ItemDescription = reqStr.ToString();
            if (!suppressWrite) {
                await ci.WriteToDatabaseAsync();
            }

            var scivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(parentCopyItemId);
            if (scivm == null) {
                return ci;
            }

            await scivm.Parent.InitializeAsync(scivm.Parent.HeadItem.CopyItem);

            scivm.Parent.ClearSelection(false);
            scivm.IsSelected = true;

            return ci;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
