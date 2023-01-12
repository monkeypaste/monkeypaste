using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models;
using System.Linq;
using MonkeyPaste.Common;
using System.Runtime.InteropServices.ComTypes;

namespace Yolo {
    public class YoloPlugin : MpIAnalyzeComponent {
        private static YoloScorer<YoloCocoP5Model> _yoloWrapper = null;
        public static YoloScorer<YoloCocoP5Model> YoloWrapper {
            get {
                if(_yoloWrapper == null) {
                    var fileName = "Yolo.Assets.Weights.yolov5s.onnx";
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

            Bitmap bmp;
            try {
                using (var ms = new MemoryStream(bytes)) {
                    bmp = new Bitmap(ms);
                }
            } catch(Exception ex) {
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = ex.Message
                };
            }

            var boxList = new List<MpImageAnnotationNodeFormat>();

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
                    foreach (var item in predictions) {
                        double score = Math.Round(item.Score, 2);

                        if (score >= confidence) {
                            var boxAnnotation = new MpImageAnnotationNodeFormat() {
                                value = (double)item.Score,
                                label = item.Label.Name,
                                x = item.Rectangle.X,
                                y = item.Rectangle.Y,
                                width = item.Rectangle.Width,
                                height = item.Rectangle.Height
                            };
                            boxList.Add(boxAnnotation);
                        }
                    }
                }
            } catch (Exception ex) {
                return new MpAnalyzerPluginResponseFormat() {
                    errorMessage = ex.Message
                };
            }



            var resp = new MpAnalyzerPluginResponseFormat() {
                dataObject = new MpPortableDataObject(
                    MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT,
                    new MpAnnotationNodeFormat() {
                        Children = boxList
                    }.SerializeJsonObject())
            };
            resp.dataObject.SetData(
                MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, new List<string> { req.GetRequestParamStringValue(5) });

            return resp;
        }

    }
}
