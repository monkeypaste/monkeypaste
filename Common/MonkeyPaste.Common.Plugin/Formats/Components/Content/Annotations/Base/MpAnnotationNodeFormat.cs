using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MonkeyPaste.Common.Plugin {
    public class MpAnnotationJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return (objectType == typeof(MpAnnotationNodeFormat));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            JObject jo = JObject.Load(reader);
            if(jo.ContainsKey("left")) {
                return jo.ToObject<MpImageAnnotationNodeFormat>(serializer);
            }
            return jo.ToObject<MpAnnotationNodeFormat>(serializer);
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
    public class MpAnnotationNodeFormat : 
        MpJsonObject, 
        MpILabelText,
        MpIIconResource, 
        MpIClampedValue,
        MpIAnnotationNode {
        #region Statics

        public static MpAnnotationNodeFormat Parse(string json) {
            MpAnnotationNodeFormat root = JsonConvert.DeserializeObject<MpAnnotationNodeFormat>(
                json, 
                new JsonSerializerSettings() {
                Converters = { 
                        new MpAnnotationJsonConverter() 
                    }});
            return root;
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

        #endregion

        #region Properties

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
