using System;
using System.Collections.Generic;
using System.Text;

namespace Yolo {
    public class YoloRequest {
        public string Base64ImageStr { get; set; }

        public double Confidence { get; set; } = 0.7;
    }
    public class YoloResponseItem {
        public double Score { get; set; } = 0;

        public double X { get; set; } = 0;

        public double Y { get; set; } = 0;

        public double Width { get; set; } = 0;

        public double Height { get; set; } = 0;

        public string Label { get; set; } = string.Empty;
    }
}
