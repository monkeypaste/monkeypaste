using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginResponseItemBaseFormat : MpJsonObject {
        public string name { get; set; } = string.Empty;

        public MpJsonPathProperty queryPath { get; set; } = new MpJsonPathProperty("$");

        public MpJsonPathProperty label { get; set; } = new MpJsonPathProperty(string.Empty);

        public MpPluginResponseAppearanceFormat appearance { get; set; } = new MpPluginResponseAppearanceFormat();

        public MpJsonPathProperty<double> score { get; set; } = new MpJsonPathProperty<double>();

        public double minScore { get; set; } = 0;
        public double maxScore { get; set; } = 1;

        public List<MpPluginResponseAnnotationFormat> children { get; set; } = new List<MpPluginResponseAnnotationFormat>();
        public List<MpPluginResponseAnnotationFormat> dynamicChildren { get; set; } = new List<MpPluginResponseAnnotationFormat>();

        public MpPluginResponseItemBaseFormat() { }

        public MpPluginResponseItemBaseFormat(string content) : this(content, 1) { }
        public MpPluginResponseItemBaseFormat(double score) : this(string.Empty, score) { }
        public MpPluginResponseItemBaseFormat(string label, double score) {
            this.label = new MpJsonPathProperty(label);
            this.score = new MpJsonPathProperty<double>(score);
        }
    }

}
