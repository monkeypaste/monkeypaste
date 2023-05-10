using MonkeyPaste.Common.Plugin;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace CoreShare {
    public class CoreShare : MpIAnalyzeAsyncComponent, MpIRequirePlatformInitialization {
        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            string data_to_share = req.GetRequestParamStringValue(1);
            await Share.RequestAsync(new ShareTextRequest {
                Text = data_to_share,
                Title = "Share Text"
            });
            return new MpAnalyzerPluginResponseFormat();
        }

        public MpPluginRequireInitializationResponseFormat Intialize(MpPluginRequireInitializationRequestFormat req) {
            return null;
        }
    }
}
