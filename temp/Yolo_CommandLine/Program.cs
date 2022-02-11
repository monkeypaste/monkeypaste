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
            if (args.Length == 0) {
                Console.WriteLine("Enter file path: ");
                args = new string[] { @"C:\Users\tkefauver\Desktop\cat.jpg" };
            }

            YoloScorer<YoloCocoP5Model> yolo = null;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Yolo.Assets.Weights.yolov5s.onnx")) {
                if (stream == null) {
                    throw new FileNotFoundException("Cannot find mappings file.");
                }
                yolo = new YoloScorer<YoloCocoP5Model>(stream, null);
            }
            
            double confidence = 0.3;
            byte[] bytes = null;

            if(args.Length == 1 && File.Exists(args[0])) {
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
                catch(Exception ex) {
                    Console.WriteLine($"Error reading file: " + args[0]);
                    Console.WriteLine(ex);
                    return;
                }
            } else {
                var requestParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginTransactionValueFormat>>(args.ToString());
                foreach (var requestParam in requestParts) {
                    if (requestParam.enumId == 1) {
                        confidence = Convert.ToDouble(requestParam.value);
                    }
                    if (requestParam.enumId == 2) {
                        try {
                            bytes = Convert.FromBase64String(requestParam.value);

                        }
                        catch (Exception ex) {
                            Console.WriteLine($"Error reading data: " + args[0]);
                            Console.WriteLine(ex);
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

            var response = new List<MpAnalyzerPluginResponseValueFormat>();
            List<YoloPrediction> predictions = yolo.Predict(bmp);
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

            string output = JsonConvert.SerializeObject(response);
            Console.WriteLine(output);
            Console.ReadKey();
        }
    }
}
