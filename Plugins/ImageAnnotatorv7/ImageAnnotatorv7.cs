using IVilson.AI.Yolov7net;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Drawing;

namespace ImageAnnotatorv7 {
    public class ImageAnnotatorv7 : MpIAnalyzeAsyncComponent {

        private string _model;
        private Yolov8 _scorer = null;

        const string PARAM_ID_MODEL = "model";
        const string PARAM_ID_CONFIDENCE = "conf";
        const string PARAM_ID_CONTENT = "img64";
        const string PARAM_USE_CUDA = "cuda";

        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            double confidence = req.GetRequestParamDoubleValue(PARAM_ID_CONFIDENCE);
            string imgBase64 = req.GetRequestParamStringValue(PARAM_ID_CONTENT);

            var resp = await Task.Run(() => {

                try {
                    LoadModel(req.GetRequestParamStringValue(PARAM_ID_MODEL), req.GetRequestParamBoolValue(PARAM_USE_CUDA));

                    var bytes = Convert.FromBase64String(imgBase64);
                    using (var ms = new MemoryStream(bytes)) {
                        using (var img = Image.FromStream(ms)) {

                            var predictions = _scorer.Predict(img);
                            // filter and convert predictions to annotations
                            var rootNode = new MpAnnotationNodeFormat() {
                                label = $"Yolo Analysis",
                                children = predictions
                                    .Where(x => x.Score >= confidence)
                                    .Select(x => new MpImageAnnotationNodeFormat() {
                                        score = x.Score,
                                        type = "Object",
                                        label = x.Label.Name,
                                        left = x.Rectangle.X,
                                        top = x.Rectangle.Y,
                                        right = x.Rectangle.X + x.Rectangle.Width,
                                        bottom = x.Rectangle.Y + x.Rectangle.Height
                                    }).Cast<MpAnnotationNodeFormat>().ToList()
                            };
                            rootNode.body = $"{rootNode.children.Count} objects detected";

                            return new MpAnalyzerPluginResponseFormat() {
                                dataObjectLookup = new Dictionary<string, object> {
                                { MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT, rootNode.SerializeJsonObject() }
                            }
                            };
                        }


                    }
                }
                catch (Exception ex) {
                    return new MpAnalyzerPluginResponseFormat() {
                        errorMessage = ex.Message
                    };
                }
            });
            return resp;
        }

        private void LoadModel(string model, bool useCuda) {
            if (_model == model) {
                // already loaded
                return;
            }

            string model_path =
                Path.Combine(
                    Path.GetDirectoryName(typeof(ImageAnnotatorv7).Assembly.Location),
                    "Assets",
                    model);
            try {
                _scorer = new Yolov8(model_path, useCuda);
            }
            catch {
                _scorer = null;
                throw;
            }
            if (_scorer != null) {
                _model = model;
                _scorer.SetupYoloDefaultLabels();
            }
        }
    }
}
