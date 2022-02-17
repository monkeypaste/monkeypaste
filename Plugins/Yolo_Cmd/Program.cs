using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Yolov5Net.Scorer.Models;
using Yolov5Net.Scorer;
using static System.Net.Mime.MediaTypeNames;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System.Drawing;

namespace Yolo_Cmd {
    class Program {
        public static void Main(string[] args) {
            YoloScorer<YoloCocoP5Model> yolo = null;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Yolo_Cmd.Assets.Weights.yolov5s.onnx")) {
                if (stream == null) {
                    throw new FileNotFoundException("Cannot find mappings file.");
                }
                yolo = new YoloScorer<YoloCocoP5Model>(stream, null);
            }

            double confidence = 0.3;
            byte[] bytes = null;

            if (args.Length == 1 && File.Exists(args[0])) {
                try {
                    using (var fs = new FileStream(args[0], FileMode.Open)) {
                        int c;
                        var byteList = new List<byte>();

                        while ((c = fs.ReadByte()) != -1) {
                            byteList.Add((byte)c);
                        }

                        bytes = byteList.ToArray();
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error reading file: " + args[0]);
                    Console.WriteLine(ex);
                    return;
                }
            } else {
                string decodedStr = Base64Decode(args[0].ToString());
                var requestParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(decodedStr);
                foreach (var requestParam in requestParts) {
                    if (requestParam.enumId == 1) {
                        confidence = Convert.ToDouble(requestParam.value);
                    }
                    if (requestParam.enumId == 2) {
                        string filePath = requestParam.value;
                        if (!File.Exists(filePath)) {
                            return;
                        }
                        try {
                            using (var fs = new FileStream(filePath, FileMode.Open)) {
                                int c;
                                var byteList = new List<byte>();

                                while ((c = fs.ReadByte()) != -1) {
                                    byteList.Add((byte)c);
                                }
                                bytes = byteList.ToArray();
                            }
                        }
                        catch (Exception ex) {
                            //Console.WriteTraceLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                            return;
                        }
                    }
                }
            }
            Bitmap bmp = null;
            try {
                using (var ms = new MemoryStream(bytes)) {
                    bmp = new Bitmap(ms);
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error converting to image");
                Console.WriteLine(ex);
                return;
            }

            if (bmp == null) {
                Console.WriteLine($"Couldn't convert to image");
                return;
            }

            var boxList = new List<MpPluginResponseAnnotationFormat>();

            List<YoloPrediction> predictions = yolo.Predict(bmp);
            using (var graphics = System.Drawing.Graphics.FromImage(bmp)) {
                foreach (var item in predictions) {
                    double score = Math.Round(item.Score, 2);

                    if (score >= confidence) {
                        var boxAnnotation = new MpPluginResponseAnnotationFormat(item.Label.Name, (double)item.Score) {
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
            var responseObj = new MpPluginResponseAnnotationFormat() {
                children = boxList
            };

            string output = JsonConvert.SerializeObject(responseObj);
            Console.WriteLine(output);
            //Console.ReadKey();
        }
        public static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
