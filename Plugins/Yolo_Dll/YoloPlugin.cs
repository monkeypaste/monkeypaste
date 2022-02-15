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

            var reqParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());

            double confidence = Convert.ToDouble(reqParts.FirstOrDefault(x => x.enumId == 1).value);
            var bytes = Convert.FromBase64String(reqParts.FirstOrDefault(x => x.enumId == 2).value);            
            
            Bitmap bmp;
            using (var ms = new MemoryStream(bytes)) {
                bmp = new Bitmap(ms);
            }

            var boxList = new List<MpAnalyzerPluginImageTokenResponseValueFormat>();

            List<YoloPrediction> predictions = _yoloWrapper.Predict(bmp);
            using (var graphics = System.Drawing.Graphics.FromImage(bmp)) {
                foreach (var item in predictions) {
                    double score = Math.Round(item.Score, 2);

                    if (score >= confidence) {
                        var box = new MpAnalyzerPluginImageTokenResponseValueFormat() {
                            x = (double)item.Rectangle.X,
                            y = (double)item.Rectangle.Y,
                            width = (double)item.Rectangle.Width,
                            height = (double)item.Rectangle.Height,
                            score = (double)item.Score,
                            label = item.Label.Name,
                            description = Enum.GetName(typeof(YoloLabelKind), item.Label.Kind)
                        };
                        boxList.Add(box);
                    }
                }
            }
            return JsonConvert.SerializeObject(boxList);
        }
    }
}
