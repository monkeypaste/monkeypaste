
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yolov5Net.Scorer.Models;
using Yolov5Net.Scorer;
using MonkeyPaste;

namespace MpWpfApp {
    public static class MpYoloTransaction {
        private static bool _isLoaded = false;
        private static YoloScorer<YoloCocoP5Model> _yoloWrapper = null;

        public static void Init() {
            _yoloWrapper = new YoloScorer<YoloCocoP5Model>("Assets/Weights/yolov5s.onnx", null);
            _isLoaded = true;
        }

        public static async Task<MpYoloResponse> DetectObjectsAsync(byte[] image, double minConfidence = 0.0) {            
            var response = new MpYoloResponse();
            await Task.Run(() => {
                if(!_isLoaded) {
                    Init();
                }
                using (var bmp = MpHelpers.ConvertBitmapSourceToBitmap(MpHelpers.ConvertByteArrayToBitmapSource(image))) {
                    List<YoloPrediction> predictions = _yoloWrapper.Predict(bmp);
                    using (var graphics = System.Drawing.Graphics.FromImage(bmp)) {
                        foreach (var item in predictions) {
                            double score = Math.Round(item.Score, 2);
                            
                            if (score >= minConfidence) {
                                var dio = new MpYoloDetectedObject() {
                                    Score = (double)item.Score,
                                    X = (double)item.Rectangle.X,
                                    Y = (double)item.Rectangle.Y,
                                    Width = (double)item.Rectangle.Width,
                                    Height = (double)item.Rectangle.Height,
                                    Label = item.Label.Name
                                };
                                response.DetectedObjects.Add(dio);

                                //graphics.DrawRectangles(
                                //    new System.Drawing.Pen(item.Label.Color, 1),
                                //    new[] { item.Rectangle });

                                //var (x, y) = (item.Rectangle.X - 3, item.Rectangle.Y - 23);

                                //graphics.DrawString($"{item.Label.Name} ({score})",
                                //    new System.Drawing.Font("Arial", 16, System.Drawing.GraphicsUnit.Pixel), new System.Drawing.SolidBrush(item.Label.Color),
                                //    new System.Drawing.PointF(x, y));
                            }
                        }

                    }
                }
            });
            return response;
        }
    }
}
