using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models;

namespace Yolo {
    public class ImageAnnotator : MpIAnalyzeAsyncComponent {
        private string _model;
        private YoloScorer<YoloCocoP5Model> _scorer = null;

        const int PARAM_ID_MODEL = 1;
        const int PARAM_ID_CONFIDENCE = 2;
        const int PARAM_ID_CONTENT = 3;

        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            double confidence = req.GetRequestParamDoubleValue(PARAM_ID_CONFIDENCE);
            string imgBase64 = req.GetRequestParamStringValue(PARAM_ID_CONTENT);

            try {
                LoadModel(req.GetRequestParamStringValue(PARAM_ID_MODEL));

                var bytes = Convert.FromBase64String(imgBase64);
                using (var ms = new MemoryStream(bytes)) {
                    using (var img = await Image.LoadAsync<Rgba32>(ms)) {

                        List<YoloPrediction> predictions = _scorer.Predict(img);
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
                        rootNode.body = $"{rootNode.children.Count} objects detected with {Math.Round(rootNode.children.Average(x => x.score), 2)} avg confidence";

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

        private void LoadModel(string model) {
            if (_model == model) {
                // already loaded
                return;
            }

            string model_path =
                Path.Combine(
                    Path.GetDirectoryName(typeof(ImageAnnotator).Assembly.Location),
                    "Assets",
                    "Weights",
                    model);
            using (var stream = File.OpenRead(model_path)) {
                if (stream == null) {
                    throw new FileNotFoundException($"Cannot find mappings file: '{model_path}'");
                }
                _scorer = new YoloScorer<YoloCocoP5Model>(stream, null);
                _model = model;
            }
        }
    }
}
