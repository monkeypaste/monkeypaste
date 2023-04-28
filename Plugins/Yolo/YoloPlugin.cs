using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
//using Yolov5Net.Scorer;
//using Yolov5Net.Scorer.Models;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models;

namespace Yolo {
    public class YoloPlugin : MpIAnalyzeComponent {
        private static YoloScorer<YoloCocoP5Model> _yoloWrapper = null;
        public static YoloScorer<YoloCocoP5Model> YoloWrapper {
            get {
                if (_yoloWrapper == null) {
                    var fileName = "Yolo.Assets.Weights.yolov5n.onnx";
                    var assembly = Assembly.GetExecutingAssembly();
                    using (var stream = assembly.GetManifestResourceStream(fileName)) {
                        if (stream == null) {
                            throw new FileNotFoundException("Cannot find mappings file.", fileName);
                        }
                        _yoloWrapper = new YoloScorer<YoloCocoP5Model>(stream, null);
                    }
                }
                return _yoloWrapper;
            }
        }

        private void Test(Image image) {
            List<YoloPrediction> predictions = YoloWrapper.Predict(image);
            using (var graphics = Graphics.FromImage(image)) {
                foreach (var prediction in predictions) {
                    double score = Math.Round(prediction.Score, 2);

                    graphics.DrawRectangles(new Pen(prediction.Label.Color, 1),
                        new[] { prediction.Rectangle });

                    var (x, y) = (prediction.Rectangle.X - 3, prediction.Rectangle.Y - 23);

                    graphics.DrawString($"{prediction.Label.Name} ({score})",
                        new Font("Arial", 16, GraphicsUnit.Pixel), new SolidBrush(prediction.Label.Color),
                    new PointF(x, y));
                }

                image.Save(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\Plugins\Yolo\Assets\result.jpg");

            }
        }
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            double confidence = req.GetRequestParamDoubleValue(1);
            string imgBase64 = req.GetRequestParamStringValue(2);

            if (string.IsNullOrEmpty(imgBase64)) {
                return null;
            }
            try {
                var bytes = Convert.FromBase64String(imgBase64);
                using (var ms = new MemoryStream(bytes)) {
                    var img = Image.FromStream(ms);

                    var rootNode = new MpAnnotationNodeFormat() {
                        label = $"Yolo Analysis"
                    };

                    List<YoloPrediction> predictions = null;
                    try {
                        predictions = YoloWrapper.Predict(img);
                    }
                    catch (Exception ex) {
                        return new MpAnalyzerPluginResponseFormat() {
                            errorMessage = ex.Message
                        };
                    }

                    using (var graphics = System.Drawing.Graphics.FromImage(img)) {
                        foreach (var prediction in predictions) {
                            double score = Math.Round(prediction.Score, 2);

                            if (score >= confidence) {
                                var boxAnnotation = new MpImageAnnotationNodeFormat() {
                                    score = prediction.Score,
                                    type = "Object",
                                    label = prediction.Label.Name,
                                    left = prediction.Rectangle.X,
                                    top = prediction.Rectangle.Y,
                                    right = prediction.Rectangle.X + prediction.Rectangle.Width,
                                    bottom = prediction.Rectangle.Y + prediction.Rectangle.Height
                                };
                                MpConsole.WriteLine($"Yolo detected label '{prediction.Label.Name}' score '{prediction.Score}' box: '{boxAnnotation}'");
                                if (rootNode.children == null) {
                                    rootNode.children = new List<MpAnnotationNodeFormat>();
                                }
                                rootNode.children.Add(boxAnnotation);
                            }
                        }
                        rootNode.body = $"{rootNode.children.Count} objects detected with {Math.Round(rootNode.children.Average(x => x.score), 2)} avg confidence";

                        string ann_json = rootNode.SerializeJsonObject();

                        var resp = new MpAnalyzerPluginResponseFormat() {
                            dataObject = new Dictionary<string, object> {
                                { MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT, ann_json }
                            }
                        };

                        return resp;
                    }
                }
            }
            catch (Exception ex) {
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = ex.Message
                };
            }
        }

    }
}
