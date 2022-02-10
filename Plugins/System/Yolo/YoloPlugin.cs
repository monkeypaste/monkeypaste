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
using System.Diagnostics;

namespace Yolo {
    public class YoloPlugin  {
        private YoloScorer<YoloCocoP5Model> _yoloWrapper = null;
        public YoloPlugin() {
            //var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),"Assets\\Weights\\yolov5s.onnx");
            //Console.WriteLine("path " + fileName);
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Yolo.Assets.Weights.yolov5s.onnx")) {
                if (stream == null) {
                    throw new FileNotFoundException("Cannot find mappings file.");
                }
                _yoloWrapper = new YoloScorer<YoloCocoP5Model>(stream, null);
            }                
        }

        #region MpIAnalyzerPluginComponent Implementation

        public object Analyze(object args) {
            Console.WriteLine("Request: ");
            Console.WriteLine(args.ToString());
            var requestParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestValueFormat>>(args.ToString());
            double confidence = 0.7;
            byte[] bytes = null;
            foreach (var requestParam in requestParts) {
                if (requestParam.enumId == 0) {
                    confidence = Convert.ToDouble(requestParam.value);
                }
                if (requestParam.enumId == 1) {
                    bytes = ReadBytesFromFile(requestParam.value);
                }
                if (requestParam.enumId == 2) {
                    try {
                        bytes = Convert.FromBase64String(requestParam.value);

                    }
                    catch (Exception ex) {
                        return null;
                    }
                }
            }
            Bitmap bmp = null;
            try {
                using (var ms = new MemoryStream(bytes)) {
                    bmp = new Bitmap(ms);
                }
            }
            catch {
                return null;
            }

            if (bmp == null) {
                return null;
            }

            var response = new List<MpAnalyzerPluginResponseValueFormat>();
            List<YoloPrediction> predictions = _yoloWrapper.Predict(bmp);
            using (var graphics = System.Drawing.Graphics.FromImage(bmp)) {
                foreach (var item in predictions) {
                    double score = Math.Round(item.Score, 2);

                    if (score >= confidence) {
                        response.Add(new MpAnalyzerPluginResponseValueFormat() {
                            text = item.Label.Name,
                            box = new MpAnalyzerPluginBoxResponseValueFormat() {
                                x = item.Rectangle.X,
                                y = item.Rectangle.Y,
                                width = item.Rectangle.Width,
                                height = item.Rectangle.Height
                            },
                            decimalVal = score
                        });
                    }
                }
            }
            //Console.Write(JsonConvert.SerializeObject(response));

            return JsonConvert.SerializeObject(response);
        }


        #endregion

        private byte[] ReadBytesFromFile(string filePath) {
            if (!File.Exists(filePath)) {
                return null;
            }
            try {
                using (var fs = new FileStream(filePath, FileMode.Open)) {
                    int c;
                    var bytes = new List<byte>();

                    while ((c = fs.ReadByte()) != -1) {
                        bytes.Add((byte)c);
                    }

                    return bytes.ToArray();
                }   
            }
            catch {
                return null;
            }
        }
    }
}
