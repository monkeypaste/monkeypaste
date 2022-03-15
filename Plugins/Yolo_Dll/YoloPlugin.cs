using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models;
using System.Linq;

namespace Yolo_Dll {
    public class YoloPlugin : MpIAnalyzerPluginComponent {
        private YoloScorer<YoloCocoP5Model> _yoloWrapper = null;

        public YoloPlugin() {
            var fileName = "Yolo_Dll.Assets.Weights.yolov5s.onnx";
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(fileName)) {
                if (stream == null) {
                    throw new FileNotFoundException("Cannot find mappings file.", fileName);
                }
                _yoloWrapper = new YoloScorer<YoloCocoP5Model>(stream, null);
            }
        }

        public async Task<object> AnalyzeAsync(object args) {
            await Task.Delay(1);

            var reqParts = JsonConvert.DeserializeObject<MpAnalyzerPluginRequestFormat>(args.ToString());

            double confidence = Convert.ToDouble(reqParts.items.FirstOrDefault(x => x.paramId == 1).value);
            var bytes = Convert.FromBase64String(reqParts.items.FirstOrDefault(x => x.paramId == 2).value);

            Bitmap bmp;
            using (var ms = new MemoryStream(bytes)) {
                bmp = new Bitmap(ms);
            }

            var boxList = new List<MpPluginResponseAnnotationFormat>();
            
            List<YoloPrediction> predictions = _yoloWrapper.Predict(bmp);
            using (var graphics = System.Drawing.Graphics.FromImage(bmp)) {
                foreach (var item in predictions) {
                    double score = Math.Round(item.Score, 2);

                    if (score >= confidence) {
                        var boxAnnotation = new MpPluginResponseAnnotationFormat() {
                            score = new MpJsonPathProperty<double>((double)item.Score),
                            label = new MpJsonPathProperty(item.Label.Name),
                            box = new MpAnalyzerPluginImageTokenResponseValueFormat(
                                            (double)item.Rectangle.X,
                                            (double)item.Rectangle.Y,
                                            (double)item.Rectangle.Width,
                                            (double)item.Rectangle.Height)
                        };
                        boxList.Add(boxAnnotation);
                    }
                }
            }

            var response = new MpPluginResponseFormat() {
                annotations = new List<MpPluginResponseAnnotationFormat>() {
                    new MpPluginResponseAnnotationFormat() {
                        name = "Yolo_Dll Response",
                        children = boxList
                    }
                }
            };


            return JsonConvert.SerializeObject(response);
        }
    }
}
