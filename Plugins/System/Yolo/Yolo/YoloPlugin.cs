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

namespace Yolo {
    public class YoloPlugin : MpIAnalyzerPluginComponent {
        private object _dobj;
        private YoloScorer<YoloCocoP5Model> _yoloWrapper = null;
        public YoloPlugin() {
            var fileName = "Yolo.Assets.Weights.yolov5s.onnx";
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(fileName)) {
                if (stream == null) {
                    throw new FileNotFoundException("Cannot find mappings file.", fileName);
                }
                _yoloWrapper = new YoloScorer<YoloCocoP5Model>(stream, null);
            }                
        }

        #region MpIAnalyzerPluginComponent Implementation


        public async Task<object> Analyze(object args) {
            await Task.Delay(1);

            var reqParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginTransactionValueFormat>>(args.ToString());

            var bytes = Convert.FromBase64String(reqParts.FirstOrDefault(x => x.enumId == 0).value);
            double confidence = Convert.ToDouble(reqParts.FirstOrDefault(x => x.enumId == 1).value);
            
            Bitmap bmp;
            using (var ms = new MemoryStream(bytes)) {
                bmp = new Bitmap(ms);
            }

            var response = new List<List<MpAnalyzerPluginTransactionValueFormat>>();
            List<YoloPrediction> predictions = _yoloWrapper.Predict(bmp);
            using (var graphics = System.Drawing.Graphics.FromImage(bmp)) {
                foreach (var item in predictions) {
                    double score = Math.Round(item.Score, 2);

                    if (score >= confidence) {
                        response.Add(new List<MpAnalyzerPluginTransactionValueFormat>() {
                                new MpAnalyzerPluginTransactionValueFormat() {
                                    name = "Score",
                                    value = ((double)item.Score).ToString()
                                },
                                new MpAnalyzerPluginTransactionValueFormat() {
                                    name = "X",
                                    value = ((double)item.Rectangle.X).ToString()
                                },
                                new MpAnalyzerPluginTransactionValueFormat() {
                                    name = "Y",
                                    value = ((double)item.Rectangle.Y).ToString()
                                },
                                new MpAnalyzerPluginTransactionValueFormat() {
                                    name = "Width",
                                    value = ((double)item.Rectangle.Width).ToString()
                                },
                                new MpAnalyzerPluginTransactionValueFormat() {
                                    name = "Height",
                                    value = ((double)item.Rectangle.Height).ToString()
                                },
                                new MpAnalyzerPluginTransactionValueFormat() {
                                    name = "Label",
                                    value = item.Label.Name
                                }
                            });
                    }
                }
            }
            return JsonConvert.SerializeObject(response);
        }

        public Task<string> AnalyzeFile(string path) {
            throw new NotImplementedException();
        }
        #endregion

    }
}
