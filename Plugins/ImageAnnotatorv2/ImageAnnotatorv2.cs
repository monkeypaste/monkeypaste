using Alturos.Yolo;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;

namespace ImageAnnotatorv2 {
    public class ImageAnnotatorv2 : MpIAnalyzeAsyncComponent {
        private static readonly Object lock_obj = new Object();
        private string _model;
        private YoloWrapper _scorer = null;

        const string PARAM_ID_MODEL = "model";
        const string PARAM_ID_CONFIDENCE = "conf";
        const string PARAM_ID_CONTENT = "img64";
        const string PARAM_USE_CUDA = "cuda";

        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            double confidence = req.GetRequestParamDoubleValue(PARAM_ID_CONFIDENCE);
            string imgBase64 = req.GetRequestParamStringValue(PARAM_ID_CONTENT);

            var resp = await Task.Run(() => {
                try {
                    lock (lock_obj) {
                        LoadModel(req.GetRequestParamStringValue(PARAM_ID_MODEL), req.GetRequestParamBoolValue(PARAM_USE_CUDA));

                        var predictions = _scorer.Detect(Convert.FromBase64String(imgBase64));
                        // filter and convert predictions to annotations
                        var rootNode = new MpAnnotationNodeFormat() {
                            label = $"Yolo Analysis ({_model})",
                            children = predictions
                                .Where(x => x.Confidence >= confidence)
                                .Select(x => new MpImageAnnotationNodeFormat() {
                                    score = x.Confidence,
                                    type = "Object",
                                    label = x.Type,
                                    left = x.X,
                                    top = x.Y,
                                    right = x.X + x.Width,
                                    bottom = x.Y + x.Height
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
                catch (Exception ex) {
                    return new MpAnalyzerPluginResponseFormat() {
                        errorMessage = ex.Message
                    };
                }
            });
            return resp;
        }

        private void LoadModel(string model, bool useCuda) {
            var models = new Dictionary<string, (string, string, string, string)>() {
                {"v2",("YOLOv2","yolov2.cfg","yolov2.weights","coco.names") },
                {"v3",("YOLOv3","yolov3.cfg","yolov3.weights","coco.names") },
                {"v9",("YOLOv9","yolo9000.cfg","yolo9000.weights","9k.names") }
            };

            if (_model == model || !models.TryGetValue(model, out var fn_tup)) {
                // already loaded
                return;
            }
            string model_dir =
                Path.Combine(
                    Path.GetDirectoryName(typeof(ImageAnnotatorv2).Assembly.Location),
                    "Assets",
                    fn_tup.Item1);

            try {
                _scorer = new YoloWrapper(
                    Path.Combine(model_dir, fn_tup.Item2),
                    Path.Combine(model_dir, fn_tup.Item3),
                    Path.Combine(model_dir, fn_tup.Item4));
            }
            catch {
                _scorer = null;
                _model = null;
                throw;
            }
            if (_scorer != null) {
                _model = model;
            }
        }
    }
}
