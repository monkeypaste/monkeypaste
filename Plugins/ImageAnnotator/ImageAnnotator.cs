using Compunet.YoloV8;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SixLabors.ImageSharp;

namespace ImageAnnotator {
    public class ImageAnnotator : MpIAnalyzeComponentAsync, MpIUnloadPluginComponent {

        private YoloV8 _scorer = null;

        const string PARAM_ID_CONFIDENCE = "conf";
        const string PARAM_ID_CONTENT = "img64";

        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            double confidence = req.GetRequestParamDoubleValue(PARAM_ID_CONFIDENCE);
            string imgBase64 = req.GetRequestParamStringValue(PARAM_ID_CONTENT);

            try {
                if (_scorer == null) {
                    _scorer = new YoloV8(Path.Combine(Path.GetDirectoryName(typeof(ImageAnnotator).Assembly.Location), "yolov8n.onnx"));
                }

                var bytes = Convert.FromBase64String(imgBase64);
                using (var ms = new MemoryStream(bytes)) {
                    using (var img = await Image.LoadAsync(ms)) {
                        var result = await _scorer.DetectAsync(img);
                        // filter and convert predictions to annotations
                        var rootNode = new MpAnnotationNodeFormat() {
                            label = $"Yolo Analysis",
                            children = result.Boxes
                                .Where(x => x.Confidence >= confidence)
                                .Select(x => new MpImageAnnotationNodeFormat() {
                                    score = x.Confidence,
                                    type = "Object",
                                    label = x.Class.Name,
                                    left = x.Bounds.X,
                                    top = x.Bounds.Y,
                                    right = x.Bounds.X + x.Bounds.Width,
                                    bottom = x.Bounds.Y + x.Bounds.Height
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
        }

        public void Unload() {
            MpConsole.WriteLine("I'm unloading see ya later!");
        }
    }
}
