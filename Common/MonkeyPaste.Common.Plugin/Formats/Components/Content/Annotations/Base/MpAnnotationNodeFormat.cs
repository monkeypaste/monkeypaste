using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common.Plugin {
    public class MpAnnotationNodeFormat :
        MpOmitNullJsonObject,
        MpILabelText,
        MpIIconResource,
        MpIClampedValue,
        MpIAnnotationNode,
        MpIPlainTextCompatible {
        #region Statics

        public static MpAnnotationNodeFormat Parse(string json) {
            //MpAnnotationNodeFormat root = JsonConvert.DeserializeObject<MpAnnotationNodeFormat>(
            //    json, 
            //    new JsonSerializerSettings() {
            //    Converters = { 
            //            new MpAnnotationJsonConverter() 
            //        }});
            //return root;

            var anf = JsonConvert.DeserializeObject(json);

            if (anf is JObject jobj) {
                return ToNode(jobj);
            }
            return null;
        }

        private static MpAnnotationNodeFormat ToNode(JObject jobj) {
            MpAnnotationNodeFormat anf = null;
            if (jobj.SelectToken("left") == null) {
                anf = JsonConvert.DeserializeObject<MpAnnotationNodeFormat>(jobj.ToString());
            } else {
                anf = JsonConvert.DeserializeObject<MpImageAnnotationNodeFormat>(jobj.ToString());
            }
            if (jobj.SelectToken("children") is JToken jtoken) {
                JArray children = JArray.Parse(jtoken.ToString());

                anf.children = children.Select(x => Parse(x.ToString())).ToList();
            } else {
                anf.children = null;
            }
            return anf;
        }

        #endregion

        #region Interfaces

        #region MpIIconResource Implementation
        [JsonIgnore]
        object MpIIconResource.IconResourceObj => icon;
        #endregion

        #region MpILabelText Implementation
        [JsonIgnore]
        string MpILabelText.LabelText => label;
        #endregion

        #region MpIClampedValue Implementation
        [JsonIgnore]
        double MpIClampedValue.min => minScore;
        [JsonIgnore]
        double MpIClampedValue.max => maxScore;
        [JsonIgnore]
        double MpIClampedValue.value => score;

        #endregion

        #region MpITreeNode Implementation
        [JsonIgnore]
        IEnumerable<MpITreeNode> MpITreeNode.Children => children;
        [JsonIgnore]
        public bool IsExpanded { get; set; }

        #endregion

        #region MpIPlainTextCompatible Implementation

        public string GetPlainText(int scope = 0) {
            string indent = string.Empty;
            for (int i = 0; i < scope; i++) {
                indent += "     ";
            }
            var sb = new StringBuilder();
            if (scope > 0) {
                sb.AppendLine();
            }
            if (!string.IsNullOrEmpty(type)) {
                sb.AppendLine($"{indent}Type: {type}");
            }
            if (!string.IsNullOrEmpty(label)) {
                sb.AppendLine($"{indent}Label: {label}");
            }
            if (!string.IsNullOrEmpty(body)) {
                sb.AppendLine($"{indent}Body: {body}");
            }
            if (!string.IsNullOrEmpty(footer)) {
                sb.AppendLine($"{indent}Footer: {footer}");
            }
            if (score > 0) {
                // presume that if scored and stored decimal value will always
                // be more than 0 if even very small
                sb.AppendLine($"{indent}Score: {score}");
            }
            if (children != null && children.Count > 0) {
                sb.Append(children.Select(x => x.GetPlainText(scope + 1)));
            }

            return sb.ToString();
        }
        #endregion

        #endregion

        #region Properties
        public virtual string guid { get; set; } = System.Guid.NewGuid().ToString();

        public virtual string type { get; set; }
        public virtual string label { get; set; }
        public virtual string body { get; set; }
        public virtual string footer { get; set; }
        public virtual object icon { get; set; }
        public virtual double minScore { get; set; } = 0;
        public virtual double maxScore { get; set; } = 1;
        public virtual double score { get; set; }

        public virtual bool isVisible { get; set; } = true;
        public virtual List<MpAnnotationNodeFormat> children { get; set; }

        public MpContentElementFormat style { get; set; }
        #endregion
    }

    public class MpImageAnnotationNodeFormat :
        MpAnnotationNodeFormat,
        MpIRectangle {

        #region MpIRectangle Implementation
        public double left { get; set; }
        public double top { get; set; }
        public double right { get; set; }
        public double bottom { get; set; }
        #endregion

        public MpImageAnnotationNodeFormat() : base() { }
        public MpImageAnnotationNodeFormat(MpRect rect) {
            left = rect.Left;
            top = rect.Top;
            right = rect.Right;
            bottom = rect.Bottom;
        }

        public override string ToString() {
            return $"left: {left} top: {top} right: {right} bottom: {bottom}";
        }
        //public new List<MpImageAnnotationNodeFormat> children { get; set; }

    }

    public class MpTextAnnotationNodeFormat :
        MpAnnotationNodeFormat,
        MpITextRange {

        #region MpITextRange Implementation
        public int Offset { get; set; }
        public int Length { get; set; }
        #endregion
    }

    public class MpContentElementFormat : MpIContentElement {
        public string type { get; set; }
        public string content { get; set; }
        public string bgColor { get; }
        public string fgColor { get; }
        public string fontSize { get; }
        public string fontFamily { get; }
        public string fontWeight { get; }


    }

}
