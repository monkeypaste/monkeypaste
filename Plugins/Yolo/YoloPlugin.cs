using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
//using Yolov5Net.Scorer;
//using Yolov5Net.Scorer.Models;
using System.Linq;
using MonkeyPaste.Common;
using System.Runtime.InteropServices.ComTypes;
using Yolov5Net.Scorer.Models;
using Yolov5Net.Scorer;

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

        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            double confidence = req.GetRequestParamDoubleValue(1);
            string imgBase64 = req.GetRequestParamStringValue(2);

            if(string.IsNullOrEmpty(imgBase64)) {
                return null;
            }
            var bytes = Convert.FromBase64String(imgBase64);
            Image bmp;
            try {
                using( var ms = new MemoryStream(bytes)) {
                    bmp = Image.FromStream(ms);
                }
            } catch(Exception ex) {
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = ex.Message
                };
            }

            MpImageAnnotationNodeFormat rootNode = new MpImageAnnotationNodeFormat() {
                label = $"Yolo Analysis",
                type = "RootAnnotation",
                left = 0,
                top = 0,
                right = bmp.Width,
                bottom = bmp.Height
            };

            try {
                List<YoloPrediction> predictions = null;
                try {
                    predictions = YoloWrapper.Predict(bmp);
                } catch(Exception ex) {
                    return new MpAnalyzerPluginResponseFormat() {
                        errorMessage = ex.Message
                    };
                }
                
                using (var graphics = System.Drawing.Graphics.FromImage(bmp)) {
                    double scale_x = 96.0d/(double)graphics.DpiX;
                    double scale_y = 96.0d/(double)graphics.DpiY;
                    MpConsole.WriteLine($"Scale x {scale_x} y {scale_y}");
                    foreach (var prediction in predictions) {
                        double score = Math.Round(prediction.Score, 2);

                        if (score >= confidence) {
                            var boxAnnotation = new MpImageAnnotationNodeFormat() {
                                score = (double)prediction.Score,
                                label = prediction.Label.Name,
                                left = prediction.Rectangle.X * scale_x,
                                top = prediction.Rectangle.Y * scale_y,
                                right = prediction.Rectangle.Width * scale_x,
                                bottom = prediction.Rectangle.Height * scale_y
                            };
                            MpConsole.WriteLine($"Yolo detected label '{prediction.Label.Name}' score '{prediction.Score}'");
                            if(rootNode.children == null) {
                                rootNode.children = new List<MpAnnotationNodeFormat>();
                            }
                            rootNode.children.Add(boxAnnotation);
                        }

                        graphics.DrawRectangles(new Pen(prediction.Label.Color, 1), new[] { prediction.Rectangle });

                        var (x, y) = (prediction.Rectangle.X - 3, prediction.Rectangle.Y - 23);

                        graphics.DrawString($"{prediction.Label.Name} ({score})",
                            new Font("Arial", 16, GraphicsUnit.Pixel), new SolidBrush(prediction.Label.Color),
                            new PointF(x, y));
                    }
                    var out_bmp = new Bitmap(bmp.Width, bmp.Height, graphics);
                    out_bmp.Save(@"C:\Users\tkefauver\Desktop\test.png");
                }
            } catch (Exception ex) {
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = ex.Message
                };
            }

            rootNode.body = $"{rootNode.children.Count} objects detected with {Math.Round(rootNode.children.Average(x => x.score), 2)} avg confidence";

            string ann_json = rootNode.SerializeJsonObject();

            var resp = new MpAnalyzerPluginResponseFormat() {
                dataObject = new MpPortableDataObject(
                    MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT,
                    ann_json)
            };

            return resp;
        }

    }
}
